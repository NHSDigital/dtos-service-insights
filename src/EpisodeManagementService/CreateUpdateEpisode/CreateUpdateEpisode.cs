using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.EpisodeManagementService;

public class CreateUpdateEpisode
{
    private readonly ILogger<CreateUpdateEpisode> _logger;
    private readonly IHttpRequestService _httpRequestService;

    public CreateUpdateEpisode(ILogger<CreateUpdateEpisode> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("CreateUpdateEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        EpisodeDto episode;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = await reader.ReadToEndAsync();
                episode = JsonSerializer.Deserialize<EpisodeDto>(postData);
                _logger.LogInformation("PostData: {postData}", postData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not read episode data.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            // Check if episode exists
            var getEpisodeUrl = $"{Environment.GetEnvironmentVariable("GetEpisodeUrl")}?EpisodeId={episode.EpisodeId}";
            var getEpisodeResponse = await _httpRequestService.SendGet(getEpisodeUrl);
            if (getEpisodeResponse.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Episode {episodeId} already exists and will be updated.", episode.EpisodeId);
                await _httpRequestService.SendPut(Environment.GetEnvironmentVariable("UpdateEpisodeUrl"), JsonSerializer.Serialize(episode));
                _logger.LogInformation("UpdateEpisode function called successfully.");
                return req.CreateResponse(HttpStatusCode.OK);
            }
            else if (getEpisodeResponse.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Episode {episodeId} does not exist and will be created.", episode.EpisodeId);
                await _httpRequestService.SendPost(Environment.GetEnvironmentVariable("CreateEpisodeUrl"), JsonSerializer.Serialize(episode));
                _logger.LogInformation("CreateEpisode function called successfully.");
                return req.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                _logger.LogError("Error occurred while checking episode existence. Status code: {statusCode}", getEpisodeResponse.StatusCode);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing episode.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
