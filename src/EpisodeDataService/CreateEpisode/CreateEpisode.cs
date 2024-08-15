using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.EpisodeDataService;

public class CreateEpisode
{
    private readonly ILogger<CreateEpisode> _logger;
    private readonly IEpisodeRepository _episodesRepository;

    public CreateEpisode(ILogger<CreateEpisode> logger, IEpisodeRepository episodeRepository)
    {
        _logger = logger;
        _episodesRepository = episodeRepository;
    }

    [Function("CreateEpisode")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        Episode episode;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
                _logger.LogInformation("postData:");
                _logger.LogInformation(postData);
                _logger.LogInformation("Request Headers: {Headers}", req.Headers);
                _logger.LogInformation("Request Body: {Body}", req.Body);
                _logger.LogInformation("Deserializing episode...");
                episode = JsonSerializer.Deserialize<Episode>(postData);
                _logger.LogInformation("Episode Object: {Episode}", episode);
                _logger.LogInformation("EpisodeId: {EpisodeId}", episode.EpisodeId);
            }
        }
        catch
        {
            _logger.LogError("Could not read episode data.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            _logger.LogInformation("Calling CreateEpisode method...");
            _episodesRepository.CreateEpisode(episode);
            _logger.LogInformation("Episode created successfully.");
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create episode in database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
