using Moq;
using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.BIAnalyticsManagementService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Common;
using System.Collections.Specialized;

namespace NHS.ServiceInsights.BIAnalyticsServiceTests;

[TestClass]
public class CreateParticipantScreeningProfileTests
{
    private Mock<ILogger<CreateParticipantScreeningProfile>> _mockLogger = new();
    private Mock<IHttpRequestService> _mockHttpRequestService = new();
    private CreateParticipantScreeningProfile _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();

    private string participantJson = "{\"nhs_number\": \"1111111112\",\"next_test_due_date\":\"2000-01-01\",\"gp_practice_id\":\"39\",\"subject_status_code\":\"NORMAL\",\"is_higher_risk\": \"True\",\"higher_risk_next_test_du" +
                                "e_date\":\"2000-01-01\",\"removal_reason\":\"reason\",\"removal_date\":\"2000-01-01\",\"bso_organisation_id\":\"00002\",\"early_recall_date\":\"2000-01-01\",\"latest_invitation_date\":\"2000-01-01\",\"prefer" +
                                "red_language\":\"english\",\"higher_risk_referral_reason_code\":\"code\",\"date_irradiated\":\"2000-01-01\",\"is_higher_risk_active\": \"False\",\"gene_code\":\"geneCode\",\"ntdd_calculation_method\":\"method\"}";
    private string demographicsJson = "{\"PrimaryCareProvider\":\"A81002\",\"PreferredLanguage\":\"EN\"}";
    public CreateParticipantScreeningProfileTests()
    {
        Environment.SetEnvironmentVariable("GetParticipantUrl", "http://localhost:6061/api/GetParticipant");
        Environment.SetEnvironmentVariable("CreateParticipantScreeningProfileUrl", "http://localhost:6011/api/CreateParticipantScreeningProfile");
        Environment.SetEnvironmentVariable("DemographicsServiceUrl", "http://localhost:6080/api/GetDemographicsData");
        _function = new CreateParticipantScreeningProfile(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenNhsNumberIsNotProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "nhs_number", null }
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
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "nhsNumber is null or empty."),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrownOnCallToGetParticipant()
    {
        // Arrange
        string nhsNumber = "1111111112";

        var queryParam = new NameValueCollection
        {
            { "nhs_Number", nhsNumber }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        var getParticipantUrl = "http://localhost:6061/api/GetParticipant?nhs_number=1111111112";

        _mockHttpRequestService
            .Setup(service => service.SendGet(getParticipantUrl))
            .ThrowsAsync(new HttpRequestException("System.Net.Http.HttpRequestException"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to deserialise or retrieve participant from http://localhost:6061/api/GetParticipant?nhs_number=1111111112.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateParticipantScreeningProfile_ShouldSendProfileToDownstreamFunctions()
    {
        // Arrange
        string nhsNumber = "1111111112";

        var queryParam = new NameValueCollection
        {
            { "nhs_Number", nhsNumber }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        var baseParticipantUrl = Environment.GetEnvironmentVariable("GetParticipantUrl");
        var participantUrl = $"{baseParticipantUrl}?nhs_number={nhsNumber}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(participantUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(participantJson, Encoding.UTF8, "application/json")
            });

        var baseDemographicsServiceUrl = Environment.GetEnvironmentVariable("DemographicsServiceUrl");
        var demographicsServiceUrl = $"{baseDemographicsServiceUrl}?nhs_number={nhsNumber}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(demographicsServiceUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(demographicsJson, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(participantUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendGet(demographicsServiceUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl"), It.IsAny<string>()), Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }
}
