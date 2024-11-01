using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.EpisodeDataService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;

namespace NHS.ServiceInsights.EpisodeDataServiceTests;

[TestClass]
public class CreateEpisodeTests
{
    private readonly Mock<ILogger<CreateEpisode>> _mockLogger = new();
    private readonly Mock<IEpisodeRepository> _mockEpisodeRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly CreateEpisode _function;
    private readonly Mock<IEndCodeLkpRepository> _mockEndCodeLkpRepository = new();
    private readonly Mock<IEpisodeTypeLkpRepository> _mockEpisodeTypeLkpRepository = new();
    private readonly Mock<IOrganisationLkpRepository> _mockOrganisationLkpRepository = new();

    public CreateEpisodeTests()
    {
        _function = new CreateEpisode(_mockLogger.Object, _mockEpisodeRepository.Object, _mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object, _mockOrganisationLkpRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Episode_Is_Invalid()
    {
        // Arrange
        var json = JsonSerializer.Serialize("Invalid episode");
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_OK_When_Repository_Creates_Episode()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServiceError_When_Repository_Throw_Exception()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        _mockEpisodeRepository.Setup(repo => repo.CreateEpisode(It.IsAny<Episode>())).Throws<Exception>();

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
