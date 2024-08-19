using Moq;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.EpisodeIntegrationService;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.TestUtils;
using System.Text.Json;

namespace NHS.EpisodeIntegrationServiceTests
{

    [TestClass]
    public class ProcessDataTests
    {
        private Mock<IHttpRequestService> httpRequestServiceMock = new();
        private Mock<ILogger<ProcessData>> loggerMock = new();
        private ProcessData _function;
        private Mock<HttpRequestData> _MockRequest = new();
        private SetupRequest _SetupRequest = new();

        [TestInitialize]
        public void TestInitialize()
        {
            Environment.SetEnvironmentVariable("EpisodeManagementUrl", "EpisodeManagementUrl");
            Environment.SetEnvironmentVariable("ParticipantManagementUrl", "ParticipantManagementUrl");

            _function = new ProcessData(loggerMock.Object, httpRequestServiceMock.Object);
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

            _MockRequest = _SetupRequest.Setup(json);

            // Act
            var result = await _function.Run(_MockRequest.Object);

            // Assert
            httpRequestServiceMock.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(2));

            httpRequestServiceMock.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(2));

        }
    }
}
