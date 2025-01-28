using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.MeshIntegrationService;
using NHS.MESH.Client.Contracts.Services;
using NHS.MESH.Client.Models;
using NHS.ServiceInsights.Model;
using Microsoft.Extensions.Options;

namespace NHS.ServiceInsights.MeshIntegrationServiceTests;

[TestClass]
public class RetrieveMeshFileTests
{

    private readonly Mock<ILogger<RetrieveMeshFile>> _mockLogger = new();
    private readonly Mock<ILogger<MeshToBlobTransferHandler>> _mockMeshTransferLogger = new();
    private readonly Mock<IMeshInboxService> _mockMeshInboxService = new();
    private readonly Mock<IMeshOperationService> _mockMeshOperationService = new();
    // private readonly Mock<IExceptionHandler> _mockExceptionHandler = new();
    private readonly Mock<IBlobStorageHelper> _mockBlobStorageHelper = new();

    private readonly IMeshToBlobTransferHandler _meshToBlobTransferHandler;
    private readonly RetrieveMeshFile _retrieveMeshFile;

    private readonly Mock<IOptions<RetrieveMeshFileConfig>> _mockOptions = new();

    private const string mailboxId = "TestMailBox";

    public RetrieveMeshFileTests()
    {
        Environment.SetEnvironmentVariable("BSSMailBox", mailboxId);
        Environment.SetEnvironmentVariable("AzureWebJobsStorage", "BlobStorage_ConnectionString");
        Environment.SetEnvironmentVariable("BSSContainerName", "BlobStorage_DestinationContainer");

        _mockOptions.Setup(i => i.Value).Returns(new RetrieveMeshFileConfig
        {
            BSSMailBox = mailboxId
        });

        _meshToBlobTransferHandler = new MeshToBlobTransferHandler(_mockMeshTransferLogger.Object, _mockBlobStorageHelper.Object, _mockMeshInboxService.Object, _mockMeshOperationService.Object);
        _retrieveMeshFile = new RetrieveMeshFile(_mockLogger.Object, _meshToBlobTransferHandler, _mockBlobStorageHelper.Object, _mockOptions.Object);
    }

    [TestMethod]
    public async Task Run_DownloadSingleFileFromMesh_Success()
    {
        //arrange

        var messageId = "MessageId";
        var fileName = "bss_episodes.csv";
        var content = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse([messageId]);
        MeshResponse<HeadMessageResponse> headResponse = MeshResponseTestHelper.CreateSuccessfulMeshHeadResponse(mailboxId, messageId, fileName);
        MeshResponse<GetMessageResponse> messageResponse = MeshResponseTestHelper.CreateSuccessfulGetMessageResponse(mailboxId, messageId, fileName, content, "application/csv");
        MeshResponse<AcknowledgeMessageResponse> acknowledgeMessageResponse = MeshResponseTestHelper.CreateSuccessfulAcknowledgeResponse(messageId);

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(headResponse);
        _mockMeshInboxService.Setup(i => i.GetMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(messageResponse);
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(true);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(acknowledgeMessageResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);


        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());


        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);

    }


    [TestMethod]
    public async Task Run_DownloadSingleChunkedFileFromMesh_Success()
    {
        //arrange

        var messageId = "MessageId";
        var fileName = "bss_episodes.csv";

        var content = new List<byte[]> { new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 } };

        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse([messageId]);
        MeshResponse<HeadMessageResponse> headResponse = MeshResponseTestHelper.CreateSuccessfulMeshHeadResponse(mailboxId, messageId, fileName, "DATA", "1:2", 2);
        MeshResponse<GetChunkedMessageResponse> messageResponse = MeshResponseTestHelper.CreateSuccessfulGetChunkedMessageResponse(mailboxId, messageId, fileName, "application/csv", content);
        MeshResponse<AcknowledgeMessageResponse> acknowledgeMessageResponse = MeshResponseTestHelper.CreateSuccessfulAcknowledgeResponse(messageId);

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(headResponse);
        _mockMeshInboxService.Setup(i => i.GetChunkedMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(messageResponse);
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(true);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(acknowledgeMessageResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);


        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());


        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetChunkedMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task Run_DownloadTwoSingleFilesFromMesh_Success()
    {
        //arrange
        List<string> messages = new List<string>{
            "Message1",
            "Message2"
        };
        byte[] byteData = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse(messages);

        var headResponses = new Queue<MeshResponse<HeadMessageResponse>>();
        var messageResponses = new Queue<MeshResponse<GetMessageResponse>>();
        foreach (var message in messages)
        {
            headResponses.Enqueue(MeshResponseTestHelper.CreateSuccessfulMeshHeadResponse(mailboxId, message));
            messageResponses.Enqueue(MeshResponseTestHelper.CreateSuccessfulGetMessageResponse(mailboxId, message, "File.Txt", byteData, "application/csv"));
        }
        MeshResponse<AcknowledgeMessageResponse> acknowledgeMessageResponse = MeshResponseTestHelper.CreateSuccessfulAcknowledgeResponse("DummyData");

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, It.IsAny<string>())).ReturnsAsync(headResponses.Dequeue());
        _mockMeshInboxService.Setup(i => i.GetMessageByIdAsync(mailboxId, It.IsAny<string>())).ReturnsAsync(messageResponses.Dequeue());
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(true);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, It.IsAny<string>())).ReturnsAsync(acknowledgeMessageResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);

        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());

        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);

        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(4));
        _mockMeshInboxService.Verify(i => i.GetMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.Exactly(2));
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

    }

    [TestMethod]
    public async Task Run_NoFilesAvailableInMesh_NoAttemptsToDownload()
    {
        //arrange
        List<string> messages = new List<string>();
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse(messages);


        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);


        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());

        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);

        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockMeshInboxService.Verify(i => i.GetChunkedMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.Never);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_SingleFileAvailableCannotUploadToBlob_DoesNotMarkAsAcknowledged()
    {
        //arrange
        var messageId = "MessageId";
        var fileName = "bss_episodes.csv";
        var content = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse([messageId]);
        MeshResponse<HeadMessageResponse> headResponse = MeshResponseTestHelper.CreateSuccessfulMeshHeadResponse(mailboxId, messageId, fileName);
        MeshResponse<GetMessageResponse> messageResponse = MeshResponseTestHelper.CreateSuccessfulGetMessageResponse(mailboxId, messageId, fileName, content, "application/csv");
        MeshResponse<AcknowledgeMessageResponse> acknowledgeMessageResponse = MeshResponseTestHelper.CreateSuccessfulAcknowledgeResponse(messageId);

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(headResponse);
        _mockMeshInboxService.Setup(i => i.GetMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(messageResponse);
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(false);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(acknowledgeMessageResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);

        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());

        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_SingleFileFailsToGetMessageHead_DoesNotAttemptToDownload()
    {
        //arrange
        var messageId = "MessageId";
        var fileName = "bss_episodes.csv";
        var content = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse([messageId]);
        MeshResponse<HeadMessageResponse> headResponse = new MeshResponse<HeadMessageResponse>
        {
            IsSuccessful = false,
            Error = new APIErrorResponse
            {
                ErrorCode = "Error",
                ErrorDescription = "Failed to Head Message"
            }
        };
        MeshResponse<GetMessageResponse> messageResponse = MeshResponseTestHelper.CreateSuccessfulGetMessageResponse(mailboxId, messageId, fileName, content, "application/csv");
        MeshResponse<AcknowledgeMessageResponse> acknowledgeMessageResponse = MeshResponseTestHelper.CreateSuccessfulAcknowledgeResponse(messageId);

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(headResponse);
        _mockMeshInboxService.Setup(i => i.GetMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(messageResponse);
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(false);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(acknowledgeMessageResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);

        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());

        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.Never);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_DownloadSingleFile_FailToGetMessage()
    {
        //arrange

        var messageId = "MessageId";
        var fileName = "bss_episodes.csv";
        var content = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse([messageId]);
        MeshResponse<HeadMessageResponse> headResponse = MeshResponseTestHelper.CreateSuccessfulMeshHeadResponse(mailboxId, messageId, fileName);
        MeshResponse<GetMessageResponse> messageResponse = new MeshResponse<GetMessageResponse>
        {
            IsSuccessful = false,
            Error = new APIErrorResponse
            {
                ErrorCode = "Error",
                ErrorDescription = "Failed to get Message"
            }

        };
        MeshResponse<AcknowledgeMessageResponse> acknowledgeMessageResponse = MeshResponseTestHelper.CreateSuccessfulAcknowledgeResponse(messageId);

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(headResponse);
        _mockMeshInboxService.Setup(i => i.GetMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(messageResponse);
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(true);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(acknowledgeMessageResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);


        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());


        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.Never);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

    }

    [TestMethod]
    public async Task Run_DownloadMessages_FailedToGetMessages()
    {
        //arrange
        MeshResponse<CheckInboxResponse> inboxResponse = new MeshResponse<CheckInboxResponse>
        {
            IsSuccessful = false,
            Error = new APIErrorResponse
            {
                ErrorCode = "Error",
                ErrorDescription = "Failed to CheckInboxMessage"
            }
        };
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, It.IsAny<string>())).ReturnsAsync(It.IsAny<MeshResponse<HeadMessageResponse>>);
        _mockMeshInboxService.Setup(i => i.GetMessageByIdAsync(mailboxId, It.IsAny<string>())).ReturnsAsync(It.IsAny<MeshResponse<GetMessageResponse>>);
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(true);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, It.IsAny<string>())).ReturnsAsync(It.IsAny<MeshResponse<AcknowledgeMessageResponse>>);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);

        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());


        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockMeshInboxService.Verify(i => i.GetMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.Never);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_DownloadSingleChunkFile_FailToGetMessage()
    {
        //arrange

        var messageId = "MessageId";
        var fileName = "bss_episodes.csv";
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse([messageId]);
        MeshResponse<HeadMessageResponse> headResponse = MeshResponseTestHelper.CreateSuccessfulMeshHeadResponse(mailboxId, messageId, fileName, "DATA", "1:2", 2);
        MeshResponse<GetChunkedMessageResponse> messageResponse = new MeshResponse<GetChunkedMessageResponse>
        {
            IsSuccessful = false,
            Error = new APIErrorResponse
            {
                ErrorCode = "Error",
                ErrorDescription = "Failed to get Message"
            }

        };
        MeshResponse<AcknowledgeMessageResponse> acknowledgeMessageResponse = MeshResponseTestHelper.CreateSuccessfulAcknowledgeResponse(messageId);

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(headResponse);
        _mockMeshInboxService.Setup(i => i.GetChunkedMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(messageResponse);
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(true);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(acknowledgeMessageResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);


        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());


        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetChunkedMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.Never);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

    }

    [TestMethod]
    [DataRow("invalid_file.csv", "application/csv")]
    [DataRow("bss_episodes.json", "application/json")]
    public async Task Run_DoesNotTransferFile_IfFileNameIsInvalid(string invalidFileName, string contentType)
    {
        //arrange
        var messageId = "MessageId";
        var content = System.Text.Encoding.UTF8.GetBytes("{}");
        MeshResponse<HandshakeResponse> handshakeResponse = MeshResponseTestHelper.CreateSuccessfulHandshakeResponse(mailboxId);
        MeshResponse<CheckInboxResponse> inboxResponse = MeshResponseTestHelper.CreateSuccessfulCheckInboxResponse([messageId]);
        MeshResponse<HeadMessageResponse> headResponse = MeshResponseTestHelper.CreateSuccessfulMeshHeadResponse(mailboxId, messageId, invalidFileName);
        MeshResponse<GetMessageResponse> messageResponse = MeshResponseTestHelper.CreateSuccessfulGetMessageResponse(mailboxId, messageId, invalidFileName, content, contentType);
        MeshResponse<AcknowledgeMessageResponse> acknowledgeMessageResponse = MeshResponseTestHelper.CreateSuccessfulAcknowledgeResponse(messageId);

        _mockMeshInboxService.Setup(i => i.GetMessagesAsync(mailboxId)).ReturnsAsync(inboxResponse);
        _mockMeshInboxService.Setup(i => i.GetHeadMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(headResponse);
        _mockMeshInboxService.Setup(i => i.GetMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(messageResponse);
        _mockBlobStorageHelper.Setup(i => i.UploadFileToBlobStorage("BlobStorage_ConnectionString", "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false)).ReturnsAsync(true);
        _mockMeshInboxService.Setup(i => i.AcknowledgeMessageByIdAsync(mailboxId, messageId)).ReturnsAsync(acknowledgeMessageResponse);
        _mockMeshOperationService.Setup(i => i.MeshHandshakeAsync(mailboxId)).ReturnsAsync(handshakeResponse);

        //act
        await _retrieveMeshFile.RunAsync(new Microsoft.Azure.Functions.Worker.TimerInfo());

        //assert
        _mockMeshInboxService.Verify(i => i.GetMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetHeadMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        _mockMeshInboxService.Verify(i => i.GetMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockBlobStorageHelper.Verify(i => i.UploadFileToBlobStorage(It.IsAny<string>(), "BlobStorage_DestinationContainer", It.IsAny<BlobFile>(), false), Times.Never);
        _mockMeshInboxService.Verify(i => i.AcknowledgeMessageByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
