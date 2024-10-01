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
    }

    [TestMethod]
    public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "episodeId", "745396" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        var url = "http://localhost:6060/api/GetEpisode?EpisodeId=745396";

        _mockHttpRequestService
            .Setup(service => service.SendGet(url))
            .ThrowsAsync(new HttpRequestException("System.Net.Http.HttpRequestException."));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Issue when getting episode from db.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }


    [TestMethod]
    public async Task RetrieveData_MakesExpectedHttpRequestsToUrls()
    {
        // Arrange
        string episodeId = "745396";

        var queryParam = new NameValueCollection
        {
            { "EpisodeId", episodeId }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        var baseUrl = Environment.GetEnvironmentVariable("GetEpisodeUrl");
        var url = $"{baseUrl}?EpisodeId={episodeId}";

        var episodeJson = "{\"episode_id\": \"745396\"}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(url))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(episodeJson, Encoding.UTF8, "application/json")
            });

        string nhsNumber = "1111111112";

        var baseParticipantUrl = Environment.GetEnvironmentVariable("GetParticipantUrl");
        var participantUrl = $"{baseParticipantUrl}?nhs_number={nhsNumber}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(participantUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(url), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendGet(participantUrl), Times.Once);
    }

    [TestMethod]
    public async Task CreateDataAssets_ShouldSendEpisodeAndProfileToDownstreamFunctions()
    {
        // Arrange
        var screeningEpisode = new Episode
        {
            EpisodeId = Guid.NewGuid().ToString(),
            ParticipantId = "test",
            ScreeningId = "test",
            NhsNumber = "1111111110",
            EpisodeTypeId = "D",
            EpisodeOpenDate = null,
            AppointmentMadeFlag = "TRUE",
            FirstOfferedAppointmentDate = "2000-01-02",
            ActualScreeningDate = "2000-01-02",
            EarlyRecallDate = "NULL",
            CallRecallStatusAuthorisedBy = "SCREENING_OFFICE",
            EndCodeId = "DC",
            EndCodeLastUpdated = "2000-01-02",
            OrganisationId = "PCO",
            BatchId = "ECHO",
            RecordInsertDatetime = "NULL",
            RecordUpdateDatetime = "NULL"
        };

        var screeningProfile = new Participant
        {
            nhs_number = Guid.NewGuid().ToString(),
            next_test_due_date = null,
            gp_practice_id = null,
            subject_status_code = "null",
            is_higher_risk = "null",
            higher_risk_next_test_due_date = null,
            removal_reason = "null",
            removal_date = "null",
            bso_organisation_id = "NORMAL",
            early_recall_date = null,
            latest_invitation_date = "false",
            preferred_language = "false",
            higher_risk_referral_reason_code = "null",
            date_irradiated = "null",
            is_higher_risk_active = null,
            gene_code = "null",
            ntdd_calculation_method = "null"
        };

        // Act
        //var (screeningEpisodeUrl, screeningProfileUrl) = _function.GetConfigurationUrls();
        //await _function.SendToCreateParticipantScreeningEpisodeAsync(screeningEpisode, screeningEpisodeUrl);
        //await _function.SendToCreateParticipantScreeningProfileAsync(screeningProfile, screeningProfileUrl);

        // Assert
        //_mockHttpRequestService.Verify(x => x.SendPost("CreateParticipantScreeningEpisodeUrl", It.IsAny<string>()), Times.Once);
        //_mockHttpRequestService.Verify(x => x.SendPost("CreateParticipantScreeningProfileUrl", It.IsAny<string>()), Times.Once);
    }
}
