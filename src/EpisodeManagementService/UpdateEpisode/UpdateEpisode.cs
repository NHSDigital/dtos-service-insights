namespace UpdateEpisode;

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;


public class UpdateEpisode
{
  private readonly ILogger<UpdateEpisode> _logger;

  public UpdateEpisode(ILogger<UpdateEpisode> logger)
  {
    _logger = logger;
  }

  [Function("updateEpisode")]
  public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
  {
    Episode episode;
    try
    {
      using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
      {
        var postData = reader.ReadToEnd();
        episode = JsonSerializer.Deserialize<Episode>(postData);
      }

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


