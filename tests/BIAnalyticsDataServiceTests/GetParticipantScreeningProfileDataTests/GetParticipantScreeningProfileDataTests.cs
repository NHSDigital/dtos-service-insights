using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.BIAnalyticsDataService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;

namespace NHS.ServiceInsights.BIAnalyticsDataServiceTests;

[TestClass]
public class GetParticipantScreeningProfileDataTests
{
    private readonly Mock<ILogger<CreateParticipantScreeningProfile>> _mockLogger = new();
    private readonly Mock<IParticipantScreeningProfileRepository> _mockParticipantScreeningProfileRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly CreateParticipantScreeningProfile _function;

    private readonly ParticipantScreeningProfile ValidParticipantScreeningProfile = new ParticipantScreeningProfile()
    {
        NhsNumber = "123456789",
        ScreeningName = "TestScreeningName",
        PrimaryCareProvider = "TestPrimaryCareProvider",
        PreferredLanguage = "TestPreferredLanguage",
        ReasonForRemoval = "TestReasonForRemoval",
        ReasonForRemovalDt = "TestReasonForRemovalDt",
        NextTestDueDate = "2019-08-01",
        NextTestDueDateCalculationMethod = "TestCalculationMethod",
        ParticipantScreeningStatus = "TestParticipantScreeningStatus",
        ScreeningCeasedReason = "TestScreeningCeasedReason",
        IsHigherRisk = "Yes",
        IsHigherRiskActive = "Yes",
        HigherRiskNextTestDueDate = "2019-08-01",
        HigherRiskReferralReasonCode = "TestCode",
        HrReasonCodeDescription = "TestHrReasonCodeDescription",
        DateIrradiated = "2019-08-01",
        GeneCode = "123456789",
        GeneCodeDescription = "TestGeneCodeDescription",
        RecordInsertDatetime = DateTime.Parse("2019-08-01"),
    };

    public CreateParticipantScreeningProfileTests()
    {
        _function = new CreateParticipantScreeningProfile(_mockLogger.Object, _mockParticipantScreeningProfileRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_OK_When_Profile_Data_Is_Saved()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidParticipantScreeningProfile);
        _mockRequest = _setupRequest.Setup(json);
        _mockParticipantScreeningProfileRepository.Setup(r => r.CreateParticipantProfile(It.IsAny<ParticipantScreeningProfile>())).Returns(Task.FromResult(true));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Information),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateParticipantScreeningProfile: participant profile saved successfully.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_Profile_Data_Is_Not_Saved()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidParticipantScreeningProfile);
        _mockRequest = _setupRequest.Setup(json);
        _mockParticipantScreeningProfileRepository.Setup(r => r.CreateParticipantProfile(It.IsAny<ParticipantScreeningProfile>())).Returns(Task.FromResult(false));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateParticipantScreeningProfile: Could not save participant profile. Data: ")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Json_Is_Not_Valid()
    {
        // Arrange
        var json = "InvalidProfile";
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateParticipantScreeningProfile: Could not read Json data.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_Saving_Data_throws_Exception()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidParticipantScreeningProfile);
        _mockRequest = _setupRequest.Setup(json);
        _mockParticipantScreeningProfileRepository.Setup(r => r.CreateParticipantProfile(It.IsAny<ParticipantScreeningProfile>())).Throws<Exception>();

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateParticipantScreeningProfile: Failed to save participant profile to the database.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
