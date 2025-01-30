using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.ParticipantManagementService;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;

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
    public void Run_ShouldReturnBadRequest_WhenNhsNumberIsNotProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "NhsNumber", null
            },
            {
                "ScreeningId", "1"
            }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Request parameters invalid"),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void Run_ShouldReturnNotFound_WhenParticipantIsNotFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "NhsNumber", "9999999999"
            },
            {
                "ScreeningId", "1"
            }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Participant does not exist"),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void Run_ShouldReturnOk_WhenParticipantIsFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "NhsNumber", "1111111110"
            },
            {
                "ScreeningId", "1"
            }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
