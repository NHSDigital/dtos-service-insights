using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using System.Text.Json.Serialization;

namespace NHS.ServiceInsights.BIAnalyticsManagementService;

public class CreateParticipantScreeningEpisode
{
    private readonly ILogger<CreateParticipantScreeningEpisode> _logger;
    private readonly IHttpRequestService _httpRequestService;
    public CreateParticipantScreeningEpisode(ILogger<CreateParticipantScreeningEpisode> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function(nameof(CreateParticipantScreeningEpisode))]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Create Participant Screening Episode function start");

        string serializedEvent = JsonSerializer.Serialize(eventGridEvent);
        _logger.LogInformation(serializedEvent);

        Episode episode;

        try
        {
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            episode = JsonSerializer.Deserialize<Episode>(eventGridEvent.Data.ToString(), options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to deserialize event data to Episode object.");
            return;
        }

        try
        {
            await SendToCreateParticipantScreeningEpisodeAsync(episode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create participant screening episode.");
        }
    }

    private async Task SendToCreateParticipantScreeningEpisodeAsync(Episode episode)
    {
        var screeningEpisode = new ParticipantScreeningEpisode
        {
            EpisodeId = episode.EpisodeId,
            ScreeningName = episode.ScreeningId.ToString(),
            NhsNumber = episode.NhsNumber,
            EpisodeType = episode.EpisodeTypeId.ToString(),
            EpisodeTypeDescription = String.Empty,
            EpisodeOpenDate = episode.EpisodeOpenDate,
            AppointmentMadeFlag = episode.AppointmentMadeFlag,
            FirstOfferedAppointmentDate = episode.FirstOfferedAppointmentDate,
            ActualScreeningDate = episode.ActualScreeningDate,
            EarlyRecallDate = episode.EarlyRecallDate,
            CallRecallStatusAuthorisedBy = episode.CallRecallStatusAuthorisedBy,
            EndCode = episode.EndCodeId.ToString(),
            EndCodeDescription = String.Empty,
            EndCodeLastUpdated = episode.EndCodeLastUpdated,
            OrganisationCode = episode.OrganisationId.ToString(),
            OrganisationName = String.Empty,
            BatchId = episode.BatchId,
            RecordInsertDatetime = DateTime.Now
        };

        var screeningEpisodeUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl");

        string serializedParticipantScreeningEpisode = JsonSerializer.Serialize(screeningEpisode);

        _logger.LogInformation("Sending ParticipantScreeningEpisode to {Url}: {Request}", screeningEpisodeUrl, serializedParticipantScreeningEpisode);


        await _httpRequestService.SendPost(screeningEpisodeUrl, serializedParticipantScreeningEpisode);
    }
}
