using Moq;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using System.Text;

namespace NHS.ServiceInsights.EpisodeManagementService;

[TestClass]
public class ReceiveDataTests
{
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();
    private readonly Mock<ILogger<ReceiveData>> _mockLogger = new();
    private readonly ReceiveData _function;
    public ReceiveDataTests()
    {
        Environment.SetEnvironmentVariable("ProcessDataURL", "ProcessDataURL");

        _function = new ReceiveData(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldLogValidJsonAndCallSendPost()
    {
        // Arrange
        var validJson = "{\"name\":\"John Doe\"}";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));

        // Act
        await _function.Run(stream, "test.json");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Information,
            0,
            It.Is<object>(state => state.ToString().Contains("JSON is valid.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);

        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldLogInvalidJsonAndNotCallSendPost()
    {
        // Arrange
        var invalidJson = "Invalid JSON content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));

        // Act
        await _function.Run(stream, "sample-container/{name}");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Could not validate JSON"),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);

        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}



