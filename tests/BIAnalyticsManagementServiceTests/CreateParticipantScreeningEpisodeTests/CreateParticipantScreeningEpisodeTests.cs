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
                        "SCREENING_OFFICE\",\"EndCodeId\":1000,\"EndCodeLastUpdated\":\"2000-01-01\",\"OrganisationId\":428765,\"BatchId\":\"ECHO\",\"RecordInsertDatetime\":\"2000-01-01\",\"RecordUpdateDatetime\":\"2000-01-01\"," +
                        "\"SrcSysProcessedDatetime\":\"2000-01-01\"}";
    private string screeningDataJson = "{\"ScreeningId\":1,\"ScreeningName\":\"Breast Screening\",\"ScreeningType\":\"BS\",\"ScreeningAcronym\":\"BSCA\",\"ScreeningWorkflowId\":null}";
    private string organisationDataJson = "{\"OrganisationId\":11,\"ScreeningName\":\"Breast Screening\",\"OrganisationCode\":\"AGA\",\"OrganisationName\":\"Gateshead\",\"OrganisationType\":null,\"IsActive\":null}";

    public CreateParticipantScreeningEpisodeTests()
    {

        Environment.SetEnvironmentVariable("GetEpisodeUrl", "http://localhost:6060/api/GetEpisode");
        Environment.SetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl", "http://localhost:6010/api/CreateParticipantScreeningEpisode");
        Environment.SetEnvironmentVariable("GetScreeningDataUrl", "http://localhost:6082/api/GetScreeningData");
        Environment.SetEnvironmentVariable("GetReferenceDataUrl", "http://localhost:6081/api/GetReferenceData");
        _function = new CreateParticipantScreeningEpisode(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldLogError_WhenEpisodeIsNotValid()
    {
        // Arrange
        string data = "{\"EpisodeId\":\"INVALID\",\"NhsNumber\":9876543210,\"ScreeningId\":1,\"EpisodeType\":\"TestEpisodeType\",\"EpisodeTypeDescription\":\"TestEpisodeTypeDescription\",\"EpisodeOpenDate\":\"2024-11-21\",\"AppointmentMadeFlag\":1,\"FirstOfferedAppointmentDate\":\"2024-12-01\",\"ActualScreeningDate\":\"2024-12-05\",\"EarlyRecallDate\":\"2025-06-01\",\"CallRecallStatusAuthorisedBy\":\"Dr. Smith\",\"EndCode\":\"TestEndCode\",\"EndCodeDescription\":\"TestEndCodeDescription\",\"EndCodeLastUpdated\":\"2024-11-21T14:35:00\",\"FinalActionCode\":\"TestFinalActionCode\",\"FinalActionCodeDescription\":\"TestFinalActionCodeDescription\",\"ReasonClosedCode\":\"TestReasonClosedCode\",\"ReasonClosedCodeDescription\":\"TestReasonClosedCodeDescription\",\"EndPoint\":\"https://api.example.com/endpoint\",\"OrganisationId\":111111,\"BatchId\":\"BATCH789\",\"SrcSysProcessedDatetime\":\"2024-11-21T14:35:00\"}";
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
        string data = "{\"EpisodeId\":12345,\"NhsNumber\":9876543210,\"ScreeningId\":1,\"EpisodeType\":\"TestEpisodeType\",\"EpisodeTypeDescription\":\"TestEpisodeTypeDescription\",\"EpisodeOpenDate\":\"2024-11-21\",\"AppointmentMadeFlag\":1,\"FirstOfferedAppointmentDate\":\"2024-12-01\",\"ActualScreeningDate\":\"2024-12-05\",\"EarlyRecallDate\":\"2025-06-01\",\"CallRecallStatusAuthorisedBy\":\"Dr. Smith\",\"EndCode\":\"TestEndCode\",\"EndCodeDescription\":\"TestEndCodeDescription\",\"EndCodeLastUpdated\":\"2024-11-21T14:35:00\",\"FinalActionCode\":\"TestFinalActionCode\",\"FinalActionCodeDescription\":\"TestFinalActionCodeDescription\",\"ReasonClosedCode\":\"TestReasonClosedCode\",\"ReasonClosedCodeDescription\":\"TestReasonClosedCodeDescription\",\"EndPoint\":\"https://api.example.com/endpoint\",\"OrganisationId\":11,\"BatchId\":\"BATCH789\",\"SrcSysProcessedDatetime\":\"2024-11-21T14:35:00\"}";
        var binaryData = new BinaryData(data);
        EventGridEvent eventGridEvent = new EventGridEvent("Episode Created", "CreateParticipantScreeningEpisode", "1.0", binaryData);

        long screening_id = 1;

        var baseScreeningDataServiceUrl = Environment.GetEnvironmentVariable("GetScreeningDataUrl");
        var screeningDataUrl = $"{baseScreeningDataServiceUrl}?screening_id={screening_id}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(screeningDataUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(screeningDataJson, Encoding.UTF8, "application/json")
            });

        long organisationId = 11;

        var baseReferenceServiceUrl = Environment.GetEnvironmentVariable("GetReferenceDataUrl");
        var getReferenceDataUrl = $"{baseReferenceServiceUrl}?organisation_id={organisationId}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(getReferenceDataUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });


        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(screeningDataUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendGet(getReferenceDataUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl"), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldLogError_WhenExceptionIsThrownOnCallCreateParticipantScreeningEpisodeUrl()
    {
        // Arrange
        string data = "{\"EpisodeId\":12345,\"NhsNumber\":9876543210,\"ScreeningId\":1,\"EpisodeType\":\"TestEpisodeType\",\"EpisodeTypeDescription\":\"TestEpisodeTypeDescription\",\"EpisodeOpenDate\":\"2024-11-21\",\"AppointmentMadeFlag\":1,\"FirstOfferedAppointmentDate\":\"2024-12-01\",\"ActualScreeningDate\":\"2024-12-05\",\"EarlyRecallDate\":\"2025-06-01\",\"CallRecallStatusAuthorisedBy\":\"Dr. Smith\",\"EndCode\":\"TestEndCode\",\"EndCodeDescription\":\"TestEndCodeDescription\",\"EndCodeLastUpdated\":\"2024-11-21T14:35:00\",\"FinalActionCode\":\"TestFinalActionCode\",\"FinalActionCodeDescription\":\"TestFinalActionCodeDescription\",\"ReasonClosedCode\":\"TestReasonClosedCode\",\"ReasonClosedCodeDescription\":\"TestReasonClosedCodeDescription\",\"EndPoint\":\"https://api.example.com/endpoint\",\"OrganisationId\":11,\"BatchId\":\"BATCH789\",\"SrcSysProcessedDatetime\":\"2024-11-21T14:35:00\"}";
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
