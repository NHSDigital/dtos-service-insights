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
    private readonly IHttpRequestService _httpRequestService;

    public CreateUpdateEpisode(ILogger<CreateUpdateEpisode> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
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
        }
        catch
        {
            _logger.LogError("Could not read episode data.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            var json = JsonSerializer.Serialize(episode);
            await _httpRequestService.SendPost(Environment.GetEnvironmentVariable("CreateEpisodeUrl"), json);

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
