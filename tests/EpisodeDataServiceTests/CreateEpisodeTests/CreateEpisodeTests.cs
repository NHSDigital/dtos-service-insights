namespace NHS.ServiceInsights.EpisodeDataServiceTests;

using Moq;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.CohortManager.Tests.TestUtils;
using NHS.ServiceInsights.EpisodeDataService;
using Data.Models;
using Data;

[TestClass]
public class CreateEpisodeTests
{
    private readonly Mock<ILogger<CreateEpisode>> _logger = new();
    private readonly Mock<IEpisodeRepository> _episodeRepository = new();
    private Mock<HttpRequestData> _request;
    private readonly SetupRequest _setupRequest = new();
    private readonly CreateEpisode _sut;

    public CreateEpisodeTests()
    {
        _sut = new CreateEpisode(_logger.Object, _episodeRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Episode_Is_Invalid()
    {
        // Arrange
        var json = JsonSerializer.Serialize("Invalid episode");
        _request = _setupRequest.Setup(json);

        // Act
        var result = _sut.Run(_request.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_OK_When_Repository_Creates_Episode()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = "1234567890"
        };

        var json = JsonSerializer.Serialize(episode);
        _request = _setupRequest.Setup(json);

        // Act
        var result = _sut.Run(_request.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServiceError_When_Repository_Throw_Exception()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = "1234567890"
        };

        var json = JsonSerializer.Serialize(episode);
        _request = _setupRequest.Setup(json);

        _episodeRepository.Setup(repo => repo.CreateEpisode(It.IsAny<Episode>())).Throws<Exception>();

        // Act
        var result = _sut.Run(_request.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
