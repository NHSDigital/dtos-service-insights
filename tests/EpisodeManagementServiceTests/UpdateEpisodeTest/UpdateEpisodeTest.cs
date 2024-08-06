namespace NHS.UpdateEpisodeTests;

using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
// using Common;
// using Model;
// using NHS.UpdateEpisodeTests;
using UpdateEpisode;
using NHS.CohortManager.Tests.TestUtils;

[TestClass]
public class UpdateEpisodeTests
{
  private readonly Mock<ILogger<UpdateEpisode>> _logger = new();
  // private readonly Mock<ICallFunction> _callFunction = new();
  private Mock<HttpRequestData> _request;
  private readonly SetupRequest _setupRequest = new();

  public UpdateEpisodeTests()
  {
    // Environment.SetEnvironmentVariable("PMSAddParticipant", "PMSAddParticipant");
    // Environment.SetEnvironmentVariable("PMSRemoveParticipant", "PMSRemoveParticipant");
    // Environment.SetEnvironmentVariable("PMSUpdateParticipant", "PMSUpdateParticipant");
    // Environment.SetEnvironmentVariable("StaticValidationURL", "StaticValidationURL");
  }

  [TestMethod]
  public async Task Run_Should_Log_EpisodeId()
  {
    // Arrange

    var episode = new Episode
    {
      EpisodeId = "1234567890"
    };


    var json = JsonSerializer.Serialize(episode);
    _request = _setupRequest.Setup(json);

    var sut = new UpdateEpisode(_logger.Object);

    // Act
    var result = await sut.Run(_request.Object);

    // Assert
    _logger.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("1234567890"))), Times.Once);

  }


}
