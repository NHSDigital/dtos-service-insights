using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.TestUtils;
using updateParticipant;

namespace NHS.ServiceInsights.ParticipantManagementServiceTests;

[TestClass]
public class UpdateParticipantTests
{
    private readonly Mock<ILogger<UpdateParticipant>> _logger = new();
    private readonly Mock<IHttpRequestService> _httpRequestService = new();
    private Mock<HttpRequestData> _httpRequestData;
    private readonly SetupRequest _setupRequest = new();
    private UpdateParticipant _function;

    public UpdateParticipantTests()
    {
        _function = new UpdateParticipant(_logger.Object, _httpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenValidParticipantNotReceived()
    {
        // Arrange
        _httpRequestData = _setupRequest.Setup("");

        // Act
        var result = await _function.Run(_httpRequestData.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_ShouldReturnOK_WhenValidParticipantReceived()
    {
        // Arrange
        var participant = new Participant
        {
            NhsNumber = "123"
        };
        var json = JsonSerializer.Serialize(participant);
        _httpRequestData = _setupRequest.Setup(json);

        // Act
        var result = await _function.Run(_httpRequestData.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }
}
