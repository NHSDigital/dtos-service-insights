using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Specialized;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.BIAnalyticsManagementService;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using System.Text;
using System.Globalization;

namespace NHS.ServiceInsights.BIAnalyticsManagementServiceTests;

[TestClass]
public class GetParticipantScreeningEpisodeTests
{
    private readonly Mock<ILogger<GetParticipantScreeningEpisode>> _mockLogger = new();
    private Mock<IHttpRequestService> _httpRequestService = new();
    private Mock<HttpRequestData> _mockRequest;
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
                SrcSysProcessedDatetime = DateTime.Parse("2023-07-05 10:30:00"),
                EpisodeId = 135890,
                ScreeningName = "John Doe",
                RecordInsertDatetime = DateTime.Parse("2023-07-05 10:30:00")
            },
            new ParticipantScreeningEpisode
            {
                SrcSysProcessedDatetime = DateTime.Parse("2023-07-05 11:30:00"),
                NhsNumber = 281479,
                ScreeningName = "Jane Smith",
                RecordInsertDatetime = DateTime.Parse("2023-07-05 11:30:00")
            }
        }
    };

    public GetParticipantScreeningEpisodeTests()
    {
        Environment.SetEnvironmentVariable("GetParticipantScreeningEpisodeDataUrl", "http://localhost:6007/api/GetParticipantScreeningEpisodeData");
        _function = new GetParticipantScreeningEpisode(_mockLogger.Object, _httpRequestService.Object);
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

        var startDate = DateTime.Parse(queryParam["startDate"]);
        var endDate = DateTime.Parse(queryParam["endDate"]);
        var expectedUri = $"http://localhost:6007/api/GetParticipantScreeningEpisodeData?page={queryParam["page"]}&pageSize={queryParam["pageSize"]}&startDate={startDate.ToString(CultureInfo.InvariantCulture)}&endDate={endDate.ToString(CultureInfo.InvariantCulture)}";

        var jsonResponse = JsonSerializer.Serialize(episodesDataPage);

        _httpRequestService
            .Setup(service => service.SendGet(expectedUri))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Information),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Episode data retrieved")),
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
    public async Task Run_Should_Return_Unsuccessful_Status_Code_If_Call_To_Data_Function_Returns_Unsuccessful_Status_Code()
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

        var startDate = DateTime.Parse(queryParam["startDate"]);
        var endDate = DateTime.Parse(queryParam["endDate"]);
        var expectedUri = $"http://localhost:6007/api/GetParticipantScreeningEpisodeData?page={queryParam["page"]}&pageSize={queryParam["pageSize"]}&startDate={startDate.ToString(CultureInfo.InvariantCulture)}&endDate={endDate.ToString(CultureInfo.InvariantCulture)}";

        var jsonResponse = JsonSerializer.Serialize(episodesDataPage);

        _httpRequestService
            .Setup(service => service.SendGet(expectedUri))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to retrieve episodes. Status Code:")),
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

        var startDate = DateTime.Parse(queryParam["startDate"]);
        var endDate = DateTime.Parse(queryParam["endDate"]);
        var expectedUri = $"http://localhost:6007/api/GetParticipantScreeningEpisodeData?page={queryParam["page"]}&pageSize={queryParam["pageSize"]}&startDate={startDate.ToString(CultureInfo.InvariantCulture)}&endDate={endDate.ToString(CultureInfo.InvariantCulture)}";

        var jsonResponse = JsonSerializer.Serialize(episodesDataPage);

        _httpRequestService
            .Setup(service => service.SendGet(expectedUri))
            .ThrowsAsync(new HttpRequestException("Exception: System.Net.Http.HttpRequestException:"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception when calling the GetParticipantScreeningEpisodeData function.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Bad_Request_When_StartDate_IsGreaterThan_EndDate()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "2024-07-05 08:30:00" },
            { "endDate", "2023-07-05 08:30:00" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        response.Body.Position = 0;
        using (StreamReader reader = new StreamReader(response.Body))
        {
            string result = await reader.ReadToEndAsync();
            Assert.AreEqual("The startDate is greater than the endDate.", result);
        }
    }

    [TestMethod]
    public async Task Run_Should_Return_Bad_Request_When_Page_Is_Invalid()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "abc" },
            { "pageSize", "2" },
            { "startDate", "2024-07-05 08:30:00" },
            { "endDate", "2024-07-06 08:30:00" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        response.Body.Position = 0;
        using (StreamReader reader = new StreamReader(response.Body))
        {
            string result = await reader.ReadToEndAsync();
            Assert.AreEqual("The page number is invalid.", result);
        }
    }

    [TestMethod]
    public async Task Run_Should_Return_Bad_Request_When_StartDate_Or_EndDate_Is_Invalid()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "invalid-date" },
            { "endDate", "invalid-date" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        response.Body.Position = 0;
        using (StreamReader reader = new StreamReader(response.Body))
        {
            string result = await reader.ReadToEndAsync();
            Assert.AreEqual("The startDate or endDate is invalid.", result);
        }
    }

    [TestMethod]
    public async Task Run_Should_Return_Bad_Request_When_StartDate_Is_Greater_Than_Today()
    {
        // Arrange
        var futureDate = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss");
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", futureDate },
            { "endDate", futureDate }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        response.Body.Position = 0;
        using (StreamReader reader = new StreamReader(response.Body))
        {
            string result = await reader.ReadToEndAsync();
            Assert.AreEqual("The startDate is greater than today date.", result);
        }
    }

    [TestMethod]
    public async Task Run_Should_Return_Bad_Request_When_EndDate_Is_Greater_Than_Today()
    {
        // Arrange
        var futureDate = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss");
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "2024-07-05 08:30:00" },
            { "endDate", futureDate }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        response.Body.Position = 0;
        using (StreamReader reader = new StreamReader(response.Body))
        {
            string result = await reader.ReadToEndAsync();
            Assert.AreEqual("The endDate is greater than today date.", result);
        }
    }

    [TestMethod]
    public async Task Run_Should_Return_Bad_Request_When_PageSize_Is_Invalid()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "abc" },
            { "startDate", "2024-07-05 08:30:00" },
            { "endDate", "2024-07-06 08:30:00" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        response.Body.Position = 0;
        using (StreamReader reader = new StreamReader(response.Body))
        {
            string result = await reader.ReadToEndAsync();
            Assert.AreEqual("The pageSize is invalid.", result);
        }
    }
}
