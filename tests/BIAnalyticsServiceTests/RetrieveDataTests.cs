using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.BIAnalyticsService;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;
using NHS.ServiceInsights.Common;
using System.Text;

namespace NHS.ServiceInsights.BIAnalyticsServiceTests;
[TestClass]
public class RetrieveDataTests
{
    private Mock<ILogger<RetrieveData>> _mockLogger = new();
    private Mock<IHttpRequestService> _mockHttpRequestService = new();
    private RetrieveData _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();

    public RetrieveDataTests()
    {

        Environment.SetEnvironmentVariable("GetEpisodeUrl", "http://localhost:6060/api/GetEpisode");
        Environment.SetEnvironmentVariable("GetParticipantUrl", "http://localhost:6061/api/GetParticipant");
        _function = new RetrieveData(_mockLogger.Object, _mockHttpRequestService.Object);
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
    public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "episodeId", "745396" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        var url = "http://localhost:6060/api/GetEpisode/?EpisodeId=745396";

        _mockHttpRequestService
            .Setup(service => service.SendGet(url))
            .ThrowsAsync(new HttpRequestException("Exception: System.Net.Http.HttpRequestException:"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Failed to call the GetEpisode Data Service.")),
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
}
