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
public class CreateParticipantScreeningEpisodeDataTests
{
    private readonly Mock<ILogger<CreateParticipantScreeningEpisode>> _mockLogger = new();
    private readonly Mock<IParticipantScreeningEpisodeRepository> _mockParticipantScreeningEpisodeRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly CreateParticipantScreeningEpisode _function;

    private readonly ParticipantScreeningEpisode ValidParticipantScreeningEpisode = new ParticipantScreeningEpisode()
    {
        EpisodeId = "1",
        ScreeningName = "TestScreeningName",
        NhsNumber = "123456789",
        EpisodeType = "TestEpisodeType",
        EpisodeTypeDescription = "TestEpisodeTypeDescription",
        EpisodeOpenDate = "2019-08-01",
        AppointmentMadeFlag = "Y",
        FirstOfferedAppointmentDate = "2019-08-01",
        ActualScreeningDate = "2019-08-01",
        EarlyRecallDate = "2019-08-01",
        CallRecallStatusAuthorisedBy = "TestCallRecallStatusAuthorisedBy",
        EndCode = "0000",
        EndCodeDescription = "TestEndCodeDescription",
        EndCodeLastUpdated = "2019-08-01",
        OrganisationCode = "0001",
        OrganisationName = "TestOrganisationName",
        BatchId = "0002",
        RecordInsertDatetime = "2019-08-01"
    };

    public CreateParticipantScreeningEpisodeDataTests()
    {
        _function = new CreateParticipantScreeningEpisode(_mockLogger.Object, _mockParticipantScreeningEpisodeRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_OK_When_Episode_Is_Saved()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidParticipantScreeningEpisode);
        _mockRequest = _setupRequest.Setup(json);
        _mockParticipantScreeningEpisodeRepository.Setup(r => r.CreateParticipantEpisode(It.IsAny<ParticipantScreeningEpisode>())).Returns(Task.FromResult(true));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Information),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateParticipantScreeningEpisode: participant episode saved successfully.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_Episode_Is_Not_Saved()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidParticipantScreeningEpisode);
        _mockRequest = _setupRequest.Setup(json);
        _mockParticipantScreeningEpisodeRepository.Setup(r => r.CreateParticipantEpisode(It.IsAny<ParticipantScreeningEpisode>())).Returns(Task.FromResult(false));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateParticipantScreeningEpisode: Could not save participant episode. Data: ")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Json_Is_Not_Valid()
    {
        // Arrange
        var json = "InvalidEpisode";
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateParticipantScreeningEpisode: Could not read Json data.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_Saving_Episode_throws_Exception()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidParticipantScreeningEpisode);
        _mockRequest = _setupRequest.Setup(json);
        _mockParticipantScreeningEpisodeRepository.Setup(r => r.CreateParticipantEpisode(It.IsAny<ParticipantScreeningEpisode>())).Throws<Exception>();

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateParticipantScreeningEpisode: Failed to save participant episode to the database.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
