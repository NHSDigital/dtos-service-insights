using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.BIAnalyticsService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using System.Collections.Specialized;


namespace NHS.ServiceInsights.BIAnalyticsServiceTests;
[TestClass]
public class CreateDataAssetsTests
{
    private Mock<ILogger<CreateDataAssets>> _mockLogger = new();
    private Mock<IHttpRequestService> _mockHttpRequestService = new();
    private CreateDataAssets _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();

    private string episodeJson = "{\"EpisodeId\":\"245395\",\"ParticipantId\":\"123\",\"ScreeningId\":\"123\",\"NhsNumber\":\"1111111112\",\"EpisodeTypeId\":\"C\",\"EpisodeOpenDate\":\"2000-01-01\"," +
                        "\"AppointmentMadeFlag\":\"TRUE\",\"FirstOfferedAppointmentDate\":\"2000-01-01\",\"ActualScreeningDate\":\"2000-01-01\",\"EarlyRecallDate\":\"2000-01-01\",\"CallRecallStatusAuthorisedBy\":\"" +
                        "SCREENING_OFFICE\",\"EndCodeId\":\"SC\",\"EndCodeLastUpdated\":\"2000-01-01\",\"OrganisationId\":\"PBO\",\"BatchId\":\"ECHO\",\"RecordInsertDatetime\":\"2000-01-01\",\"RecordUpdateDatetime\":\"2000-01-01\"}";

    private string participantJson = "{\"nhs_number\":\"1111111112\",\"next_test_due_date\":\"2000-01-01\",\"gp_practice_id\":\"39\",\"subject_status_code\":\"NORMAL\",\"is_higher_risk\":\"false\",\"higher_risk_next_test_du" +
                                "e_date\":\"2000-01-01\",\"removal_reason\":\"reason\",\"removal_date\":\"2000-01-01\",\"bso_organisation_id\":\"00002\",\"early_recall_date\":\"2000-01-01\",\"latest_invitation_date\":\"2000-01-01\",\"prefer" +
                                "red_language\":\"english\",\"higher_risk_referral_reason_code\":\"code\",\"date_irradiated\":\"2000-01-01\",\"is_higher_risk_active\":\"false\",\"gene_code\":\"geneCode\",\"ntdd_calculation_method\":\"method\"}";

    public CreateDataAssetsTests()
    {

        Environment.SetEnvironmentVariable("GetEpisodeUrl", "http://localhost:6060/api/GetEpisode");
        Environment.SetEnvironmentVariable("GetParticipantUrl", "http://localhost:6061/api/GetParticipant");
        Environment.SetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl", "http://localhost:6010/api/CreateParticipantScreeningEpisode");
        Environment.SetEnvironmentVariable("CreateParticipantScreeningProfileUrl", "http://localhost:6011/api/CreateParticipantScreeningProfile");
        _function = new CreateDataAssets(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenEpisodeIdIsNotProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "EpisodeId", null }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "episodeId is null or empty."),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrownOnCallToGetEpisode()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "episodeId", "245395" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        var getEpisodeUrl = "http://localhost:6060/api/GetEpisode?EpisodeId=245395";

        _mockHttpRequestService
            .Setup(service => service.SendGet(getEpisodeUrl))
            .ThrowsAsync(new HttpRequestException("System.Net.Http.HttpRequestException"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Issue when getting episode from http://localhost:6060/api/GetEpisode?EpisodeId=245395. ")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrownOnCallToGetParticipant()
    {
        // Arrange
        string episodeId = "745396";

        var queryParam = new NameValueCollection
        {
            { "EpisodeId", episodeId }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        var baseGetEpisodeUrl = Environment.GetEnvironmentVariable("GetEpisodeUrl");
        var getEpisodeUrl = $"{baseGetEpisodeUrl}?EpisodeId={episodeId}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(getEpisodeUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(episodeJson, Encoding.UTF8, "application/json")
            });

        var getParticipantUrl = "http://localhost:6061/api/GetParticipant?nhs_number=1111111112";

        _mockHttpRequestService
            .Setup(service => service.SendGet(getParticipantUrl))
            .ThrowsAsync(new HttpRequestException("System.Net.Http.HttpRequestException"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Issue when getting participant from http://localhost:6061/api/GetParticipant?nhs_number=1111111112.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateDataAssets_ShouldSendEpisodeAndProfileToDownstreamFunctions()
    {
        // Arrange
        string episodeId = "745396";

        var queryParam = new NameValueCollection
        {
            { "EpisodeId", episodeId }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        var baseGetEpisodeUrl = Environment.GetEnvironmentVariable("GetEpisodeUrl");
        var getEpisodeUrl = $"{baseGetEpisodeUrl}?EpisodeId={episodeId}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(getEpisodeUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(episodeJson, Encoding.UTF8, "application/json")
            });

        string nhsNumber = "1111111112";

        var baseParticipantUrl = Environment.GetEnvironmentVariable("GetParticipantUrl");
        var participantUrl = $"{baseParticipantUrl}?nhs_number={nhsNumber}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(participantUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(participantJson, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(getEpisodeUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendGet(participantUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl"), It.IsAny<string>()), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl"), It.IsAny<string>()), Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }
}
