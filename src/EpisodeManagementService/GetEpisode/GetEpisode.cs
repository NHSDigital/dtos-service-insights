using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.EpisodeManagementService
{
    public class GetEpisode
    {
        private readonly ILogger<GetEpisode> _logger;
        private readonly IHttpRequestService _httpRequestService;

        public GetEpisode(ILogger<GetEpisode> logger, IHttpRequestService httpRequestService)
        {
            _logger = logger;
            _httpRequestService = httpRequestService;
        }

        [Function("GetEpisode")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Request to retrieve episode information has been processed.");

            string episodeId = req.Query["EpisodeId"];

            if (string.IsNullOrEmpty(episodeId))
            {
                _logger.LogError("Please enter a valid Episode ID.");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                return badRequestResponse;
            }

            var baseUrl = Environment.GetEnvironmentVariable("GetEpisodeUrl");
            var url = $"{baseUrl}?EpisodeId={episodeId}";
            _logger.LogInformation("Requesting URL: {Url}", url);

            try
            {
                var response = await _httpRequestService.SendGet(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to retrieve episode with Episode ID {episodeId}. Status Code: {response.StatusCode}");
                    var errorResponse = req.CreateResponse(response.StatusCode);
                    return errorResponse;
                }

                var episodeJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Episode data retrieved");

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                successResponse.Headers.Add("Content-Type", "application/json");
                await successResponse.WriteStringAsync(episodeJson);

                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to call the GetEpisode Data Service. \nUrl:{url}\nException: {ex}", url, ex);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
