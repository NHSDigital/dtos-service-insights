using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsService;

public class CreateDataAssets
{
    private readonly ILogger<CreateDataAssets> _logger;
    private readonly IHttpRequestService _httpRequestService;
    public CreateDataAssets(ILogger<CreateDataAssets> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("CreateDataAssets")]
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

        var (screeningEpisodeUrl, screeningProfileUrl) = GetConfigurationUrls();
        if (string.IsNullOrEmpty(screeningEpisodeUrl) || string.IsNullOrEmpty(screeningProfileUrl))
        {
            return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "One or both URLs are not configured");
        }

        // Log out useful debug information
        _logger.LogInformation(screeningEpisodeUrl);
        _logger.LogInformation(screeningProfileUrl);

        // Send to downstream functions
        await SendToCreateParticipantScreeningEpisodeAsync(episode, screeningEpisodeUrl);
        await ProcessEpisodeDataAsync(participant, screeningProfileUrl);

        _logger.LogInformation("Data processed successfully.");

            return req.CreateResponse(HttpStatusCode.OK);
        }

        catch (Exception ex)
        {
            _logger.LogError("Failed to call the Participant Management Service. \nUrl:{participantUrl}\nException: {ex}", participantUrl, ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    private (string screeningEpisodeUrl, string screeningProfileUrl) GetConfigurationUrls()
    {
        return (Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl"), Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl"));
    }

    private async Task SendToCreateParticipantScreeningEpisodeAsync(List<ParticipantScreeningProfile> participant, string screeningProfileUrl)
    {
        if (participant != null && participant.Any())
        {
            _logger.LogInformation("Mapping participant profile data.");
            foreach (var screeningProfile in participant)
            {
                string serializedParticipantScreeningProfile = JsonSerializer.Serialize(screeningProfile, new JsonSerializerOptions { WriteIndented = true });

            // Log the Episode data before sending it
            _logger.LogInformation($"Sending Episode to {screeningProfileUrl}: {serializedParticipantScreeningProfile}");

            await _httpRequestService.SendPost(screeningProfileUrl, serializedParticipantScreeningProfile);
            }
        }
    }
}

public class DataPayLoad
{
    public List<ParticipantScreeningEpisode> ScreeningEpisodes { get; set; } = new List<ParticipantScreeningEpisode>();
    public List<ParticipantScreeningProfile> Participants { get; set; } = new List<ParticipantScreeningProfile>();
}
