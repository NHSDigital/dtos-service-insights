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
    public async Task Run_ShouldReturnBadRequest_WhenEpisodeIdIsInvalid()
    {
        // Arrange
        EventGridEvent eventGridEvent = new EventGridEvent("Episode Created", "CreateParticipantScreeningEpisode", "1.0", "INVALID");

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "episodeId is invalid"),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrownOnCallToGetEpisode()
    {
        // Arrange
        EventGridEvent eventGridEvent = new EventGridEvent("Episode Created", "CreateParticipantScreeningEpisode", "1.0", 12345);

        var getEpisodeUrl = "http://localhost:6060/api/GetEpisode?EpisodeId=245395";

        _mockHttpRequestService
            .Setup(service => service.SendGet(getEpisodeUrl))
            .ThrowsAsync(new HttpRequestException("System.Net.Http.HttpRequestException"));

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to deserialise or retrieve episode from http://localhost:6060/api/GetEpisode?EpisodeId=12345.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateParticipantScreeningEpisode_ShouldSendEpisodeToDownstreamFunctions()
    {
        // Arrange
        EventGridEvent eventGridEvent = new EventGridEvent("Episode Created", "CreateParticipantScreeningEpisode", "1.0", 12345);

        var baseGetEpisodeUrl = Environment.GetEnvironmentVariable("GetEpisodeUrl");
        var getEpisodeUrl = $"{baseGetEpisodeUrl}?EpisodeId=12345";

        _mockHttpRequestService
            .Setup(service => service.SendGet(getEpisodeUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(episodeJson, Encoding.UTF8, "application/json")
            });

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(getEpisodeUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl"), It.IsAny<string>()), Times.Once);
    }
}
