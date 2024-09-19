using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

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
        _logger.LogInformation("Request to retrieve data has been processed.");

        string episodeId = req.Query["EpisodeId"];

        if (string.IsNullOrEmpty(episodeId))
        {
            _logger.LogError("Please enter a valid Episode ID.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var baseUrl = Environment.GetEnvironmentVariable("GetEpisodeUrl");
        var url = $"{baseUrl}?EpisodeId={episodeId}";
        _logger.LogInformation("Requesting episode URL: {Url}", url);

        string episodeJson = string.Empty;
        Episode episode;

        try
        {
            var response = await _httpRequestService.SendGet(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve episode with Episode ID {episodeId}. Status Code: {response.StatusCode}");
                return req.CreateResponse(response.StatusCode);
            }

            episodeJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Episode data retrieved");
            episode = JsonSerializer.Deserialize<Episode>(episodeJson);
        }

        catch (Exception ex)
        {
            _logger.LogError("Failed to call the GetEpisode Data Service. \nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        string nhsNumber = "1111111112";

        var baseParticipantUrl = Environment.GetEnvironmentVariable("GetParticipantUrl");
        var participantUrl = $"{baseParticipantUrl}?nhs_number={nhsNumber}";
        _logger.LogInformation("Requesting participant URL: {Url}",participantUrl);

        try
        {
            var participantResponse = await _httpRequestService.SendGet(participantUrl);

            if (!participantResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve participant data with NHS number {nhsNumber}. Status Code: {participantResponse.StatusCode}");
                return req.CreateResponse(participantResponse.StatusCode);
            }

            var participantJson = await participantResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Participant data retrieved");

            var participant = JsonSerializer.Deserialize<Participant>(participantJson);
            if (participant == null)
            {
                _logger.LogError($"Participant with NHS Number {nhsNumber} not found.");
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        catch (Exception ex)
        {
            _logger.LogError("Failed to call the Participant Management Service. \nUrl:{participantUrl}\nException: {ex}", participantUrl, ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
public class RetrievedData
{
    public Episode episode { get; set; }
    public Participant participant { get; set; }
}
