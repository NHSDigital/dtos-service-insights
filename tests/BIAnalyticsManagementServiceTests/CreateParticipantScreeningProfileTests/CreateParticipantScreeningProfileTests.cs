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
public class CreateParticipantScreeningProfileTests
{
    private Mock<ILogger<CreateParticipantScreeningProfile>> _mockLogger = new();
    private Mock<IHttpRequestService> _mockHttpRequestService = new();
    private CreateParticipantScreeningProfile _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();

    private string participantJson = "{\"NhsNumber\": 1111111112,\"ScreeningName\": \"Breast Screening\",\"ScreeningId\":1,\"NextTestDueDate\": \"2019-08-01\",\"NextTestDueDateCalculationMethod\": \"ROUTINE\",\"ParticipantScreeningStatus\": \"NORMAL\", \"ScreeningCeasedReason\": \"PERSONAL_WELFARE\",\"IsHigherRisk\": 1,\"IsHigherRiskActive\": 1,\"HigherRiskNextTestDueDate\": \"2020-02-01\",\"HigherRiskReferralReasonCode\": \"\",\"DateIrradiated\": \"2019-12-01\",\"GeneCode\": \"BRCA1\"}";
    private string demographicsJson = "{\"PrimaryCareProvider\":\"A81002\",\"PreferredLanguage\":\"EN\"}";
    private string screeningDataJson = "{\"ScreeningId\":1,\"ScreeningName\":\"Breast Screening\",\"ScreeningType\":\"BS\",\"ScreeningAcronym\":\"BSCA\",\"ScreeningWorkflowId\":null}";
    public CreateParticipantScreeningProfileTests()
    {
        Environment.SetEnvironmentVariable("GetParticipantUrl", "http://localhost:6061/api/GetParticipant");
        Environment.SetEnvironmentVariable("CreateParticipantScreeningProfileUrl", "http://localhost:6011/api/CreateParticipantScreeningProfile");
        Environment.SetEnvironmentVariable("DemographicsServiceUrl", "http://localhost:6080/api/GetDemographicsData");
        Environment.SetEnvironmentVariable("GetScreeningDataUrl", "http://localhost:6082/api/GetScreeningData");
        _function = new CreateParticipantScreeningProfile(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldLogError_WhenProfileIsNotValid()
    {
        // Arrange
        string data = "{\"NhsNumber\": \"INVALID\",\"ScreeningName\": \"Breast Screening\",\"ScreeningId\":1,\"NextTestDueDate\": \"2019-08-01\",\"NextTestDueDateCalculationMethod\": \"ROUTINE\",\"ParticipantScreeningStatus\": \"NORMAL\", \"ScreeningCeasedReason\": \"PERSONAL_WELFARE\",\"IsHigherRisk\": 1,\"IsHigherRiskActive\": 1,\"HigherRiskNextTestDueDate\": \"2020-02-01\",\"HigherRiskReferralReasonCode\": \"\",\"DateIrradiated\": \"2019-12-01\",\"GeneCode\": \"BRCA1\"}";
        var binaryData = new BinaryData(data);
        EventGridEvent eventGridEvent = new EventGridEvent("Profile Created", "CreateParticipantScreeningProfile", "1.0", binaryData);

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Unable to deserialize event data to Participant object."),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Run_ShouldLogError_WhenExceptionIsThrownOnCallCreateParticipantScreeningProfileUrl()
    {
        // Arrange
        string data = "{\"NhsNumber\": 1111111112,\"ScreeningName\": \"Breast Screening\",\"ScreeningId\":1,\"NextTestDueDate\": \"2019-08-01\",\"NextTestDueDateCalculationMethod\": \"ROUTINE\",\"ParticipantScreeningStatus\": \"NORMAL\", \"ScreeningCeasedReason\": \"PERSONAL_WELFARE\",\"IsHigherRisk\": 1,\"IsHigherRiskActive\": 1,\"HigherRiskNextTestDueDate\": \"2020-02-01\",\"HigherRiskReferralReasonCode\": \"\",\"DateIrradiated\": \"2019-12-01\",\"GeneCode\": \"BRCA1\"}";
        var binaryData = new BinaryData(data);
        EventGridEvent eventGridEvent = new EventGridEvent("Profile Created", "CreateParticipantScreeningProfile", "1.0", binaryData);

        var createParticipantScreeningProfileUrl = "http://localhost:6011/api/CreateParticipantScreeningProfile";

        _mockHttpRequestService
            .Setup(service => service.SendPost(createParticipantScreeningProfileUrl, It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("System.Net.Http.HttpRequestException"));

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to create participant screening profile.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateParticipantScreeningProfile_ShouldSendProfileToDownstreamFunctions()
    {
        // Arrange
        string data = "{\"NhsNumber\": 1111111112,\"ScreeningName\": \"Breast Screening\",\"ScreeningId\":1,\"NextTestDueDate\": \"2019-08-01\",\"NextTestDueDateCalculationMethod\": \"ROUTINE\",\"ParticipantScreeningStatus\": \"NORMAL\", \"ScreeningCeasedReason\": \"PERSONAL_WELFARE\",\"IsHigherRisk\": 1,\"IsHigherRiskActive\": 1,\"HigherRiskNextTestDueDate\": \"2020-02-01\",\"HigherRiskReferralReasonCode\": \"\",\"DateIrradiated\": \"2019-12-01\",\"GeneCode\": \"BRCA1\"}";
        var binaryData = new BinaryData(data);
        EventGridEvent eventGridEvent = new EventGridEvent("Profile Created", "CreateParticipantScreeningProfile", "1.0", binaryData);

        long NhsNumber = 1111111112;

        var baseDemographicsServiceUrl = Environment.GetEnvironmentVariable("DemographicsServiceUrl");
        var demographicsServiceUrl = $"{baseDemographicsServiceUrl}?nhs_number={NhsNumber}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(demographicsServiceUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(demographicsJson, Encoding.UTF8, "application/json")
            });


        long screening_id = 1;

        var baseScreeningDataServiceUrl = Environment.GetEnvironmentVariable("GetScreeningDataUrl");
        var screeningDataUrl = $"{baseScreeningDataServiceUrl}?screening_id={screening_id}";

        _mockHttpRequestService
            .Setup(service => service.SendGet(screeningDataUrl))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(screeningDataJson, Encoding.UTF8, "application/json")
            });

        // Act
        await _function.Run(eventGridEvent);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(demographicsServiceUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendGet(screeningDataUrl), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl"), It.IsAny<string>()), Times.Once);
    }
}
