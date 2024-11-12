using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Specialized;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.BIAnalyticsDataService;

namespace NHS.ServiceInsights.BIAnalyticsDataServiceTests;

[TestClass]
public class GetParticipantScreeningEpisodeTests
{
    private readonly Mock<ILogger<GetParticipantScreeningEpisode>> _mockLogger = new();
    private readonly Mock<IParticipantScreeningEpisodeRepository> _mockParticipantScreeningEpisodeRepository = new();
    private Mock<HttpRequestData> _mockRequest = new();
    private readonly SetupRequest _setupRequest = new();
    private readonly GetParticipantScreeningEpisode _function;

    private readonly EpisodesDataPage episodesDataPage = new EpisodesDataPage
    {
        TotalResults = 2,
        TotalPages = 1,
        TotalRemainingPages = 0,
        episodes = new List<ParticipantScreeningEpisode>
        {
            new ParticipantScreeningEpisode
            {
                Id = 1,
                EpisodeId = "135890",
                ScreeningName = "John Doe",
                RecordInsertDatetime = DateTime.Parse("2023-07-05 10:30:00")
            },
            new ParticipantScreeningEpisode
            {
                Id = 2,
                EpisodeId = "281479",
                ScreeningName = "Jane Smith",
                RecordInsertDatetime = DateTime.Parse("2023-07-05 11:30:00")
            }
        }
    };

    public GetParticipantScreeningEpisodeTests()
    {
        _function = new GetParticipantScreeningEpisode(_mockLogger.Object, _mockParticipantScreeningEpisodeRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_OK_With_EpisodesDataPage_Json_Data()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "2023-07-05 08:30:00" },
            { "endDate", "2023-07-05 08:30:00" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockParticipantScreeningEpisodeRepository.Setup(r => r.GetParticipantScreeningEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>())).Returns(Task.FromResult(episodesDataPage));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Information),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetParticipantScreeningEpisode: Participant episodes found successfully")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        response.Body.Seek(0, SeekOrigin.Begin);
        var episodesDataPageResult = await JsonSerializer.DeserializeAsync<EpisodesDataPage>(response.Body);
        Assert.IsTrue(episodesDataPageResult.episodes.Count() == 2);
        Assert.IsTrue(episodesDataPageResult.TotalResults == 2);
        Assert.IsTrue(episodesDataPageResult.TotalPages == 1);
        Assert.IsTrue(episodesDataPageResult.TotalRemainingPages == 0);
    }

    [TestMethod]
    public async Task Run_Should_Return_NotFound_When_It_Doesnt_Find_Any_Episodes()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "2023-07-05 08:30:00" },
            { "endDate", "2023-07-05 08:30:00" }
        };

        var emptyEpisodesDataPage = new EpisodesDataPage(){
            TotalResults = 0,
            TotalPages = 0,
            TotalRemainingPages = 0,
            episodes = new List<ParticipantScreeningEpisode>()
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockParticipantScreeningEpisodeRepository.Setup(r => r.GetParticipantScreeningEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>())).Returns(Task.FromResult(emptyEpisodesDataPage));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetParticipantScreeningEpisode: Could not find any participant episodes")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_Exception_Is_Thrown()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "2023-07-05 08:30:00" },
            { "endDate", "2023-07-05 08:30:00" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockParticipantScreeningEpisodeRepository.Setup(r => r.GetParticipantScreeningEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>())).Throws(new Exception("Database error"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetParticipantScreeningEpisode: Failed to get participant episodes from the database.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
    }
}
