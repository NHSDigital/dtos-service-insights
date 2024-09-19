using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.AnalyticsDataService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using System.Security.Cryptography.X509Certificates;

namespace NHS.ServiceInsights.SaveTransformedDataTests;

[TestClass]
public class SaveTransformedDataTests
{
    private readonly Mock<ILogger<SaveTransformedData>> _mockLogger = new();
    private readonly Mock<IAnalyticsRepository> _mockAnalyticsRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly SaveTransformedData _function;

    private readonly Analytic ValidAnalytic = new Analytic() {
        EpisodeId = "1",
        EpisodeType = "TestType",
        EpisodeDate = "2019-08-01",
        AppointmentMade = "2019-08-01",
        DateOfFoa = "2019-08-01",
        DateOfAs = "2019-08-01",
        EarlyRecallDate = "2019-08-01",
        CallRecallStatusAuthorisedBy = "TestPerson",
        EndCode = "0",
        EndCodeLastUpdated = "2019-08-01",
        BsoOrganisationCode = "0",
        BsoBatchId = "0",
        NhsNumber = "123435",
        GpPracticeId = "0",
        BsoOrganisationId = "0",
        NextTestDueDate = "2019-08-01",
        SubjectStatusCode = "2019-08-01",
        LatestInvitationDate = "2019-08-01",
        RemovalReason = "TestRemovalReason",
        RemovalDate = "2019-08-01",
        CeasedReason = "TestCeasedReason",
        ReasonForCeasedCode = "0",
        ReasonDeducted = "TestReasonDeducted",
        IsHigherRisk = "Y",
        HigherRiskNextTestDueDate = "2019-08-01",
        HigherRiskReferralReasonCode = "2019-08-01",
        DateIrradiated = "2019-08-01",
        IsHigherRiskActive = "2019-08-01",
        GeneCode = "0",
        NtddCalculationMethod = "TestMethod",
        PreferredLanguage = "English"
    };

    public SaveTransformedDataTests()
    {
        _function = new SaveTransformedData(_mockLogger.Object, _mockAnalyticsRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_OK_When_Analytics_Data_Is_Saved()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidAnalytic);
        _mockRequest = _setupRequest.Setup(json);
        _mockAnalyticsRepository.Setup(r => r.SaveData(It.IsAny<Analytic>())).Returns(true);

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Information),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SaveTransformedData: Analytics data saved successfully.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_Analytics_Data_Is_Not_Saved()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidAnalytic);
        _mockRequest = _setupRequest.Setup(json);
        _mockAnalyticsRepository.Setup(r => r.SaveData(It.IsAny<Analytic>())).Returns(false);

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SaveTransformedData: Could not save analytics data. Data: ")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Json_Is_Not_Valid()
    {
        // Arrange
        var json = "InvalidAnalytic";
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SaveTransformedData: Could not read Json data.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_Saving_Data_throws_Exception()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidAnalytic);
        _mockRequest = _setupRequest.Setup(json);
        _mockAnalyticsRepository.Setup(r => r.SaveData(It.IsAny<Analytic>())).Throws<Exception>();

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SaveTransformedData: Failed to save analytics data to the database.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
