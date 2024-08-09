namespace NHS.ServiceInsights.EpisodeManagementService;

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Common;

public class CreateUpdateEpisode
{
    private readonly ILogger<CreateUpdateEpisode> _logger;
    private readonly ICallFunction _callFunction;

    public CreateUpdateEpisode(ILogger<CreateUpdateEpisode> logger, ICallFunction callFunction)
    {
        _logger = logger;
        _callFunction = callFunction;
    }

    [Function("CreateUpdateEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        Episode episode;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
                episode = JsonSerializer.Deserialize<Episode>(postData);
            }

            var json = JsonSerializer.Serialize(episode);
            var response = await _callFunction.SendPost(Environment.GetEnvironmentVariable("CreateEpisodeUrl"), json);

            _logger.LogInformation(episode.EpisodeId);
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch
        {
            _logger.LogError("Could not read episode data.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
