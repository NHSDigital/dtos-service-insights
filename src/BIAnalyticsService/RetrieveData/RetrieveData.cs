using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.ParticipantManagementService;

namespace NHS.ServiceInsights.BIAnalyticsService;

public class RetrieveData
{
    private readonly ILogger<RetrieveData> _logger;
    private readonly IHttpRequestService _httpRequestService;
    public RetrieveData(ILogger<RetrieveData> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("RetrieveData")]
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
    var url = $"{baseUrl}EpisodeId={episodeId}";
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

        // Retrieve participant data
        string nhsNumber = "1111111112";

        if (string.IsNullOrEmpty(nhsNumber))
        {
            _logger.LogError("Please enter a valid NHS Number.");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequestResponse;
        }

        var participant = ParticipantRepository.GetParticipantByNhsNumber(nhsNumber);

        if (participant == null)
        {
            _logger.LogError($"Participant with NHS Number {nhsNumber} not found.");
            return req.CreateResponse(HttpStatusCode.NotFound);
        }
        var retrievedData = new RetrievedData ()
        {
            episode = JsonSerializer.Deserialize<Episode>(episodeJson),
            participant = participant
        };
        string serializedRetrievedData = JsonSerializer.Serialize(retrievedData, new JsonSerializerOptions { WriteIndented = true });

        var transformUrl = Environment.GetEnvironmentVariable("TransformUrl");
        //  _logger.LogInformation($"Sending participant to {transformUrl}: {serializedRetrievedData}");//
        await _httpRequestService.SendPost(transformUrl, serializedRetrievedData);

    _logger.LogInformation("Requesting URL: {transformUrl}", transformUrl);
        var Response = req.CreateResponse(HttpStatusCode.OK);
        Response.Headers.Add("Content-Type", "application/json");
        return Response;
    }
    catch (Exception ex)
    {
        _logger.LogError("Failed to call the GetEpisode Data Service. \nUrl:{url}\nException: {ex}", url, ex);
        return req.CreateResponse(HttpStatusCode.InternalServerError);
    }
}
}
public class RetrievedData
{
    public Episode episode;
    public Participant participant;
}
