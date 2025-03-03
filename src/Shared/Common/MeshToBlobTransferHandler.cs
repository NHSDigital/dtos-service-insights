using System.Reflection;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Model;
using NHS.MESH.Client.Contracts.Services;
using NHS.MESH.Client.Helpers;
using NHS.MESH.Client.Helpers.ContentHelpers;
using NHS.MESH.Client.Models;

namespace NHS.ServiceInsights.Common;

public class MeshToBlobTransferHandler : IMeshToBlobTransferHandler
{
    private readonly IMeshInboxService _meshInboxService;
    private readonly IMeshOperationService _meshOperationService;
    private readonly IBlobStorageHelper _blobStorageHelper;
    private readonly ILogger<MeshToBlobTransferHandler> _logger;

    private string _blobConnectionString;
    private string _mailboxId;
    private string _destinationContainer;
    private string _poisonContainer;

    public MeshToBlobTransferHandler(ILogger<MeshToBlobTransferHandler> logger, IBlobStorageHelper blobStorageHelper, IMeshInboxService meshInboxService, IMeshOperationService meshOperationService)
    {
        _logger = logger;
        _meshInboxService = meshInboxService;
        _blobStorageHelper = blobStorageHelper;
        _meshOperationService = meshOperationService;

        // Display version of GzipHelper assembly
        LogGzipHelperVersion();
    }

    private void LogGzipHelperVersion()
    {
        var gzipHelperAssembly = Assembly.GetAssembly(typeof(GZIPHelpers));
        var version = gzipHelperAssembly?.GetName().Version?.ToString() ?? "Unknown";
        if (gzipHelperAssembly == null)
        {
            _logger.LogWarning("GZIPHelper assembly could not be loaded.");
            return;
        }
        _logger.LogInformation("GZIPHelper assembly version: {Version}", version);
    }

    public async Task<bool> MoveFilesFromMeshToBlob(Func<MessageMetaData, bool> predicate, Func<MessageMetaData, string> fileNameFunction, string mailboxId, string blobConnectionString, string destinationContainer, string poisonContainer, bool executeHandshake = false)
    {
        _blobConnectionString = blobConnectionString;
        _mailboxId = mailboxId;
        _destinationContainer = destinationContainer;
        _poisonContainer = poisonContainer;

        int messageCount;
        if (executeHandshake)
        {
            var meshValidationResponse = await _meshOperationService.MeshHandshakeAsync(mailboxId);

            if (!meshValidationResponse.IsSuccessful)
            {
                _logger.LogError("Error While handshaking with MESH. ErrorCode: {ErrorCode}, ErrorDescription: {ErrorDescription}", meshValidationResponse.Error?.ErrorCode, meshValidationResponse.Error?.ErrorDescription);
                return false;
            }
        }

        do
        {
            var checkForMessages = await _meshInboxService.GetMessagesAsync(mailboxId);
            if (!checkForMessages.IsSuccessful)
            {
                _logger.LogCritical("Error while connecting getting Messages from MESH. ErrorCode: {ErrorCode}, ErrorDescription: {ErrorDescription}", checkForMessages.Error?.ErrorCode, checkForMessages.Error?.ErrorDescription);
                return false;
            }

            messageCount = checkForMessages.Response.Messages.Count();

            _logger.LogInformation("{messageCount} Messages were found within mailbox {mailboxId}", messageCount, mailboxId);

            if (messageCount == 0)
            {
                break;
            }

            var messagesMoved = await MoveAllMessagesToBlobStorage(checkForMessages.Response.Messages, predicate);

            _logger.LogInformation("{messagesMoved} out of {messageCount} Messages were moved from mailbox: {mailboxId} to Blob Storage", messagesMoved, messageCount, mailboxId);

            if (messagesMoved == 0 && messageCount == 500)
            {
                _logger.LogCritical("Mailbox is full of messages that do not meet the predicate for transfer to Blob Storage");
                return false;
            }
        }
        while (messageCount == 500);

        return true;
    }

    private async Task<int> MoveAllMessagesToBlobStorage(IEnumerable<string> messages, Func<MessageMetaData, bool> predicate)
    {
        var messagesMovedToBlobStorage = 0;
        foreach (var message in messages)
        {
            var messageHead = await _meshInboxService.GetHeadMessageByIdAsync(_mailboxId, message);

            if (!messageHead.IsSuccessful)
            {
                _logger.LogCritical("Error while getting Message Head from MESH. ErrorCode: {ErrorCode}, ErrorDescription: {ErrorDescription}", messageHead.Error?.ErrorCode, messageHead.Error?.ErrorDescription);
                continue;
            }
            var container = predicate(messageHead.Response.MessageMetaData) ? _destinationContainer : _poisonContainer;
            if (!predicate(messageHead.Response.MessageMetaData))
            {
                _logger.LogInformation("Message: {MessageId} with fileName: {FileName} did not meet the predicate for transferring to inbound Blob Storage", messageHead.Response.MessageMetaData.MessageId, messageHead.Response.MessageMetaData.FileName);
            }
            bool wasMessageDownloaded = await TransferMessageToBlobStorage(messageHead.Response.MessageMetaData, container);
            if (!wasMessageDownloaded)
            {
                _logger.LogCritical("Message: {MessageId} was not able to be transferred to Blob Storage", messageHead.Response.MessageMetaData.MessageId);
                continue;
            }
            var acknowledgeResponse = await _meshInboxService.AcknowledgeMessageByIdAsync(_mailboxId, messageHead.Response.MessageMetaData.MessageId);
            if (!acknowledgeResponse.IsSuccessful)
            {
                _logger.LogCritical("Message: {MessageId} was not able to be acknowledged, Message will be removed from Blob Storage", messageHead.Response.MessageMetaData.MessageId);
            }
            messagesMovedToBlobStorage++;
        }

        return messagesMovedToBlobStorage;
    }

    private async Task<bool> TransferMessageToBlobStorage(MessageMetaData messageHead, string container)
    {
        if (messageHead.MessageType != "DATA") { return false; }

        BlobFile? blobFile;

        if ((messageHead.TotalChunks ?? 1) > 1)
        {
            blobFile = await DownloadChunkedFile(messageHead.MessageId);
        }
        else
        {
            blobFile = await DownloadFile(messageHead.MessageId);
        }

        if (blobFile == null)
        {
            return false;
        }

        if (blobFile.FileName.StartsWith(messageHead.MessageId))
        {
            container = _poisonContainer;
            _logger.LogInformation("Message: {MessageId} with fileName: {FileName} is being moved to poison container due to failed gzip decompression", messageHead.MessageId, blobFile.FileName);
        }

        var uploadedToBlob = await _blobStorageHelper.UploadFileToBlobStorage(_blobConnectionString, container, blobFile);

        if (uploadedToBlob)
        {
            _logger.LogInformation("Message: {MessageId} with fileName: {FileName} was uploaded to Blob Storage container: {Container}", messageHead.MessageId, blobFile.FileName, container);
        }
        else
        {
            _logger.LogError("Message: {MessageId} with fileName: {FileName} failed to upload to Blob Storage container: {Container}", messageHead.MessageId, blobFile.FileName, container);
        }

        return uploadedToBlob;
    }

    private async Task<BlobFile?> DownloadChunkedFile(string messageId)
    {
        var result = await _meshInboxService.GetChunkedMessageByIdAsync(_mailboxId, messageId);
        if (!result.IsSuccessful)
        {
            _logger.LogError("Failed to download chunked message from MESH MessageId: {messageId}", messageId);
            return null;
        }

        var meshFile = await FileHelpers.ReassembleChunkedFile(result.Response.FileAttachments);

        return new BlobFile(meshFile.Content, meshFile.FileName);
    }

    private async Task<BlobFile?> DownloadFile(string messageId)
    {
        var result = await _meshInboxService.GetMessageByIdAsync(_mailboxId, messageId);
        if (!result.IsSuccessful)
        {
            _logger.LogError("Failed to download single message from MESH MessageId: {messageId}", messageId);
            return null;
        }

        string fileName = result.Response.FileAttachment.FileName;
        byte[] fileContent = result.Response.FileAttachment.Content;

        // Log file size before decompression
        _logger.LogInformation("File attachment size: {Size} bytes for MessageId: {MessageId}, FileName: {FileName}", fileContent.Length, messageId, fileName);

        // Check for GZIP magic bytes (1F 8B)
        bool isGzip = fileContent.Length > 2 && fileContent[0] == 0x1F && fileContent[1] == 0x8B;

        if (isGzip)
        {
            try
            {
                _logger.LogInformation("Detected GZIP file, decompressing: {fileName}", fileName);

                // Log the first few bytes of the file content for debugging
                _logger.LogInformation("First 10 bytes of file content: {Bytes}", BitConverter.ToString(fileContent.Take(10).ToArray()));

                var decompressedFileContent = GZIPHelpers.DeCompressBuffer(fileContent);

                // Log the result of the decompression
                if (decompressedFileContent != null)
                {
                    _logger.LogInformation("Decompressed file size: {Size} bytes", decompressedFileContent.Length);
                }
                else
                {
                    _logger.LogWarning("Decompression returned null for file: {fileName}", fileName);
                }

                // Check if decompression returned null or empty content
                if (decompressedFileContent == null || decompressedFileContent.Length == 0)
                {
                    _logger.LogWarning("Decompression returned empty content for file: {fileName}", fileName);

                    // Move the original file to the poison container for further analysis
                    return new BlobFile(fileContent, $"{messageId}_{fileName}");
                }

                string originalFileName = Path.GetFileNameWithoutExtension(fileName);
                _logger.LogInformation("Decompression successful for GZIP file: {fileName}", originalFileName);

                // Log file size after decompression
                _logger.LogInformation("File size after decompression: {Size} bytes", decompressedFileContent.Length);

                return new BlobFile(decompressedFileContent, originalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decompress GZIP file: {fileName}", fileName);

                // Move the original file to the poison container for further analysis
                return new BlobFile(fileContent, $"{messageId}_{fileName}");
            }
        }

        return new BlobFile(fileContent, fileName);
    }
}
