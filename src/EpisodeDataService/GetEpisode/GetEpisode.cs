using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
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
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
    {
        string episodeId;

        try
        {
            episodeId = req.Query["episodeId"];
            _logger.LogInformation("Getting Episode ID: {episodeId}", episodeId);
        }
        catch
        {
            _logger.LogError("Could not read episode ID.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            Episode episode = _episodesRepository.GetEpisode(episodeId);
            if (episode == null)
            {
                _logger.LogInformation("Episode not found.");
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            _logger.LogInformation("Episode found successfully.");

            string jsonResponse = JsonSerializer.Serialize(episode);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(jsonResponse);

            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get episode from database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

}