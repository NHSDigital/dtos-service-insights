using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace NHS.ServiceInsights.EpisodeManagementService
{
    public class GetEpisode
    {
        private readonly ILogger<GetEpisode> _logger;
        private readonly HttpClient _httpClient;

        public GetEpisode(ILogger<GetEpisode> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [Function("GetEpisode")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Request to retrieve episode information has been processed.");

            string episodeId = req.Query["EpisodeId"];

            if (string.IsNullOrEmpty(episodeId))
            {
                _logger.LogError("Please enter a valid Episode ID.");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            // Call the Get Episode function in the Episode Data Service
            var response = await _httpClient.GetAsync($"http://localhost:7070/api/GetEpisode?EpisodeId={episodeId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve episode with Episode ID {episodeId}. Status Code: {response.StatusCode}");
                return req.CreateResponse(response.StatusCode);
            }

            var episodeJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Episode data retrieved: {episodeJson}");

            var newResponse = req.CreateResponse(HttpStatusCode.OK);
            newResponse.Headers.Add("Content-Type", "application/json");
            await newResponse.WriteStringAsync(episodeJson);

            return newResponse;
        }
    }
}
