using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.EpisodeDataService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using System.Collections.Specialized;

namespace NHS.ServiceInsights.EpisodeDataServiceTests;

[TestClass]
public class GetEpisodeTests
{
    private readonly Mock<ILogger<GetEpisode>> _mockLogger = new();
    private readonly Mock<IEpisodeRepository> _mockEpisodeRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly GetEpisode _function;

    public GetEpisodeTests()
    {
        _function = new GetEpisode(_mockLogger.Object, _mockEpisodeRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Episode_Is_Not_Provided()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "EpisodeId", null }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

        _mockLogger.Verify(log => log.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Episode not found.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

    }


}
