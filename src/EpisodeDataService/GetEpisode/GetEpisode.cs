using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.EpisodeDataService;

public class GetEpisode
{
    private readonly ILogger<GetEpisode> _logger;
    private readonly IEpisodeRepository _episodesRepository;

    public GetEpisode(ILogger<GetEpisode> logger, IEpisodeRepository episodeRepository)
    {
        _logger = logger;
        _episodesRepository = episodeRepository;
    }

    [Function("GetEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
    {
        string episodeIdString = req.Query["episodeId"];

        if (!long.TryParse(episodeIdString, out long episodeId))
        {
            _logger.LogError("Episode ID missing or not valid.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        _logger.LogInformation("Getting Episode ID: {episodeId}", episodeId);

        try
        {
            Episode episode = await _episodesRepository.GetEpisodeAsync(episodeId);
            if (episode == null)
            {
                _logger.LogError("Episode not found.");
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            _logger.LogInformation("Episode found successfully.");

            string jsonResponse = JsonSerializer.Serialize(episode);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(jsonResponse);

            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get episode from database.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
