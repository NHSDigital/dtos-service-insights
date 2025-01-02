using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.ParticipantManagementService;
using NHS.ServiceInsights.TestUtils;

namespace NHS.ServiceInsights.ParticipantManagementServiceTests;

[TestClass]
public class UpdateParticipantTests
{
    private readonly Mock<ILogger<UpdateParticipant>> _mockLogger = new();
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();
    private Mock<HttpRequestData> _mockHttpRequestData;
    private readonly SetupRequest _setupRequest = new();
    private readonly UpdateParticipant _function;

    public UpdateParticipantTests()
    {
        _function = new UpdateParticipant(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenValidParticipantNotReceived()
    {
        // Arrange
        _mockHttpRequestData = _setupRequest.Setup("");

        // Act
        var result = await _function.Run(_mockHttpRequestData.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_ShouldReturnOK_WhenValidParticipantReceived()
    {
        // Arrange
        var participant = new InitialParticipantDto
        {
            NhsNumber = 999999999
        };
        var json = JsonSerializer.Serialize(participant);
        _mockHttpRequestData = _setupRequest.Setup(json);

        // Act
        var result = await _function.Run(_mockHttpRequestData.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }
}
