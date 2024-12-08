using Moq;
using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.BIAnalyticsManagementService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Common;
using System.Collections.Specialized;
using Azure.Messaging.EventGrid;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsServiceTests;

[TestClass]
public class CreateParticipantScreeningEpisodeTests
{
    private Mock<ILogger<CreateParticipantScreeningEpisode>> _mockLogger = new();
    private Mock<IHttpRequestService> _mockHttpRequestService = new();
    private CreateParticipantScreeningEpisode _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();
    private string episodeJson = "{\"EpisodeId\":245395,\"ParticipantId\":123,\"ScreeningId\":123,\"NhsNumber\":1111111112,\"EpisodeTypeId\":11111,\"EpisodeOpenDate\":\"2000-01-01\"," +
                        "\"AppointmentMadeFlag\":1,\"FirstOfferedAppointmentDate\":\"2000-01-01\",\"ActualScreeningDate\":\"2000-01-01\",\"EarlyRecallDate\":\"2000-01-01\",\"CallRecallStatusAuthorisedBy\":\"" +
                        "SCREENING_OFFICE\",\"EndCodeId\":1000,\"EndCodeLastUpdated\":\"2000-01-01\",\"OrganisationId\":428765,\"BatchId\":\"ECHO\",\"RecordInsertDatetime\":\"2000-01-01\",\"RecordUpdateDatetime\":\"2000-01-01\"}";

    public CreateParticipantScreeningEpisodeTests()
    {

        Environment.SetEnvironmentVariable("GetEpisodeUrl", "http://localhost:6060/api/GetEpisode");
        Environment.SetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl", "http://localhost:6010/api/CreateParticipantScreeningEpisode");
        _function = new CreateParticipantScreeningEpisode(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldLogError_WhenEpisodeIsNotValid()
    {
        // Arrange
        string data = "{\"EpisodeId\":\"INVALID\",\"EpisodeIdSystem\":null,\"ScreeningId\":1,\"NhsNumber\":9876543210,\"EpisodeTypeId\":null,\"EpisodeOpenDate\":\"2024-11-21\",\"AppointmentMadeFlag\":1,\"FirstOfferedAppointmentDate\":\"2024-12-01\",\"ActualScreeningDate\":\"2024-12-05\",\"EarlyRecallDate\":\"2025-06-01\",\"CallRecallStatusAuthorisedBy\":\"Dr. Smith\",\"EndCodeId\":null,\"EndCodeLastUpdated\":\"2024-11-21T14:35:00\",\"ReasonClosedCodeId\":null,\"FinalActionCodeId\":null,\"EndPoint\":\"https://api.example.com/endpoint\",\"OrganisationId\":111111,\"BatchId\":\"BATCH789\",\"RecordInsertDatetime\":\"2024-12-04T14:23:04.587\",\"RecordUpdateDatetime\":\"2024-12-04T14:39:06.4500865Z\",\"EndCode\":null,\"EpisodeType\":null,\"FinalActionCode\":null,\"ReasonClosedCode\":null}";
        var binaryData = new BinaryData(data);
        EventGridEvent eventGridEvent = new EventGridEvent("Episode Created", "CreateParticipantScreeningEpisode", "1.0", binaryData);

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Unable to deserialize event data to Episode object."),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateParticipantScreeningEpisode_ShouldSendEpisodeToDownstreamFunctions()
    {
        // Arrange
        string data = "{\"EpisodeId\":12345,\"EpisodeIdSystem\":null,\"ScreeningId\":1,\"NhsNumber\":9876543210,\"EpisodeTypeId\":null,\"EpisodeOpenDate\":\"2024-11-21\",\"AppointmentMadeFlag\":1,\"FirstOfferedAppointmentDate\":\"2024-12-01\",\"ActualScreeningDate\":\"2024-12-05\",\"EarlyRecallDate\":\"2025-06-01\",\"CallRecallStatusAuthorisedBy\":\"Dr. Smith\",\"EndCodeId\":null,\"EndCodeLastUpdated\":\"2024-11-21T14:35:00\",\"ReasonClosedCodeId\":null,\"FinalActionCodeId\":null,\"EndPoint\":\"https://api.example.com/endpoint\",\"OrganisationId\":111111,\"BatchId\":\"BATCH789\",\"RecordInsertDatetime\":\"2024-12-04T14:23:04.587\",\"RecordUpdateDatetime\":\"2024-12-04T14:39:06.4500865Z\",\"EndCode\":null,\"EpisodeType\":null,\"FinalActionCode\":null,\"ReasonClosedCode\":null}";
        var binaryData = new BinaryData(data);
        EventGridEvent eventGridEvent = new EventGridEvent("Episode Created", "CreateParticipantScreeningEpisode", "1.0", binaryData);

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost(Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl"), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldLogError_WhenExceptionIsThrownOnCallCreateParticipantScreeningEpisodeUrl()
    {
        // Arrange
        string data = "{\"EpisodeId\":12345,\"EpisodeIdSystem\":null,\"ScreeningId\":1,\"NhsNumber\":9876543210,\"EpisodeTypeId\":null,\"EpisodeOpenDate\":\"2024-11-21\",\"AppointmentMadeFlag\":1,\"FirstOfferedAppointmentDate\":\"2024-12-01\",\"ActualScreeningDate\":\"2024-12-05\",\"EarlyRecallDate\":\"2025-06-01\",\"CallRecallStatusAuthorisedBy\":\"Dr. Smith\",\"EndCodeId\":null,\"EndCodeLastUpdated\":\"2024-11-21T14:35:00\",\"ReasonClosedCodeId\":null,\"FinalActionCodeId\":null,\"EndPoint\":\"https://api.example.com/endpoint\",\"OrganisationId\":111111,\"BatchId\":\"BATCH789\",\"RecordInsertDatetime\":\"2024-12-04T14:23:04.587\",\"RecordUpdateDatetime\":\"2024-12-04T14:39:06.4500865Z\",\"EndCode\":null,\"EpisodeType\":null,\"FinalActionCode\":null,\"ReasonClosedCode\":null}";
        var binaryData = new BinaryData(data);
        EventGridEvent eventGridEvent = new EventGridEvent("Episode Created", "CreateParticipantScreeningEpisode", "1.0", binaryData);

        var CreateParticipantScreeningEpisodeUrl = "http://localhost:6010/api/CreateParticipantScreeningEpisode";

        _mockHttpRequestService
            .Setup(service => service.SendPost(CreateParticipantScreeningEpisodeUrl, It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("System.Net.Http.HttpRequestException"));

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to create participant screening episode.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
