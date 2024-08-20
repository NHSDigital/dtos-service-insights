using Moq;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.EpisodeIntegrationService;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.TestUtils;
using System.Text.Json;

namespace NHS.ServiceInsights.EpisodeIntegrationServiceTests;

[TestClass]
public class ProcessDataTests
{
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();
    private readonly Mock<ILogger<ProcessData>> _mockLogger = new();
    private ProcessData _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private readonly SetupRequest _setupRequest = new();

    public ProcessDataTests()
    {
        Environment.SetEnvironmentVariable("EpisodeManagementUrl", "EpisodeManagementUrl");
        Environment.SetEnvironmentVariable("ParticipantManagementUrl", "ParticipantManagementUrl");

        _function = new ProcessData(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task ProcessData_ShouldSendJsonToDownstreamFunctions()
    {
        // Arrange
        DataPayLoad _DataPayLoad = new DataPayLoad
        {
            Episodes = new List<Episode> {
                new Episode{ episode_id = "245395"
            },
                new Episode{ episode_id = "245396"
            }},
            Participants = new List<Participant> {
                new Participant{ nhs_number = "1111111112"
            },
                new Participant{ nhs_number = "1111111110"
            }}
        };

        var json = JsonSerializer.Serialize(_DataPayLoad);
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(2));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(2));
    }
}
