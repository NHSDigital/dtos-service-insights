using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.ParticipantManagementService;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;

namespace NHS.ServiceInsights.ParticipantManagementServiceTests;

[TestClass]
public class GetParticipantTests
{
    private readonly Mock<ILogger<GetParticipant>> _mockLogger = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly GetParticipant _function;

    public GetParticipantTests()
    {
        _function = new GetParticipant(_mockLogger.Object);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenNhsNumberIsNotProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "nhs_number", null
            }
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
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Please enter a valid NHS Number."),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnNotFound_WhenParticipantIsNotFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "nhs_number", "9999999999"
            }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == $"Participant with NHS Number 9999999999 not found."),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnOk_WhenParticipantIsFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "nhs_number", "1111111110"
            }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Participant participant;
        using (StreamReader reader = new StreamReader(response.Body, Encoding.UTF8))
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = reader.ReadToEnd();
            participant = JsonSerializer.Deserialize<Participant>(responseBody);
        }
        Assert.AreEqual("1111111110", participant.nhs_number);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenNhsNumberIsInvalidFormat()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "nhs_number", "invalidFormat"
            }
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
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Invalid NHS Number format."),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
