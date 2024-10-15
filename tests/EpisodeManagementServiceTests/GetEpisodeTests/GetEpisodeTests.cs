using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.EpisodeManagementService;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.EpisodeManagementServiceTests;

[TestClass]
public class GetEpisodeTests
{
    private Mock<ILogger<GetEpisode>> _mockLogger = new();
    private Mock<IHttpRequestService> _httpRequestService = new();
    private GetEpisode _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();

    public GetEpisodeTests()
    {

        Environment.SetEnvironmentVariable("GetEpisodeUrl", "http://localhost:6070/api/GetEpisode");
        _function = new GetEpisode(_mockLogger.Object, _httpRequestService.Object);
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
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Please enter a valid Episode ID."),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnNotFound_WhenEpisodeIsNotFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "EpisodeId", "12345" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        var expectedUri = "http://localhost:6070/api/GetEpisode?EpisodeId=12345";

        _httpRequestService
            .Setup(service => service.SendGet(expectedUri))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        _httpRequestService.Verify(service =>
            service.SendGet(expectedUri), Times.Once());
    }

    [TestMethod]
    public async Task Run_ShouldReturnOk_WhenEpisodeIsFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "EpisodeId", "245395" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        var expectedUri = "http://localhost:6070/api/GetEpisode?EpisodeId=245395";

        var jsonResponse = "{\"EpisodeId\": \"245395\", \"Status\": \"Active\"}";

        _httpRequestService
            .Setup(service => service.SendGet(expectedUri))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        response.Body.Seek(0, SeekOrigin.Begin);
        var episode = await JsonSerializer.DeserializeAsync<JsonElement>(response.Body);
        Assert.AreEqual("245395", episode.GetProperty("EpisodeId").GetString());

        _httpRequestService.Verify(service =>
            service.SendGet(expectedUri), Times.Once());
    }

    [TestMethod]
    public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "episodeId", "245395" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        var expectedUri = "http://localhost:6070/api/GetEpisode?EpisodeId=245395";

        _httpRequestService
            .Setup(service => service.SendGet(expectedUri))
            .ThrowsAsync(new HttpRequestException("Exception: System.Net.Http.HttpRequestException:"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Failed to call the GetEpisode Data Service.") &&
                                                    state.ToString().Contains("Exception: System.Net.Http.HttpRequestException:")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
