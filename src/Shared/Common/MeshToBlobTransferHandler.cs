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

        // Check for GZIP magic bytes (1F 8B)
        bool isGzip = result.Response.FileAttachment.Content.Length > 2 &&
                  result.Response.FileAttachment.Content[0] == 0x1F &&
                  result.Response.FileAttachment.Content[1] == 0x8B;

        if (isGzip)
        {
            try
            {
                _logger.LogInformation("Detected GZIP file by magic bytes, decompressing: {fileName}", fileName);
                var decompressedFileContent = GZIPHelpers.DeCompressBuffer(result.Response.FileAttachment.Content);
                if (decompressedFileContent != null && decompressedFileContent.Length > 0)
                {
                    _logger.LogInformation("Decompression successful for file: {fileName}", fileName);
                    string originalFileName = GZIPHelpers.GetOriginalFileName(result.Response.FileAttachment.Content) ?? fileName;
                    // Return the decompressed file
                    return new BlobFile(decompressedFileContent, originalFileName);
                }
                else
                {
                    _logger.LogWarning("Decompression returned empty content for file: {fileName}", fileName);
                    // Failed decompression
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decompress GZIP file: {fileName}", fileName);
                // return null;
                // Return the file with a prefix of the message id so it goes to the poison container
                return new BlobFile(result.Response.FileAttachment.Content, $"{messageId}_{fileName}");
            }
        }
        // Return the file as is
        return new BlobFile(result.Response.FileAttachment.Content, fileName);
    }

}
