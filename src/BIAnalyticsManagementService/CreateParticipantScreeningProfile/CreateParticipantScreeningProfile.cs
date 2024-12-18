using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsManagementService;

public class CreateParticipantScreeningProfile
{
    private readonly ILogger<CreateParticipantScreeningProfile> _logger;
    private readonly IHttpRequestService _httpRequestService;
    public CreateParticipantScreeningProfile(ILogger<CreateParticipantScreeningProfile> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function(nameof(CreateParticipantScreeningProfile))]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Create Participant Screening Profile function start");

        string serializedEvent = JsonSerializer.Serialize(eventGridEvent);
        _logger.LogInformation(serializedEvent);

        ParticipantDto participant;

        try
        {
            participant = JsonSerializer.Deserialize<ParticipantDto>(eventGridEvent.Data.ToString());
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to deserialize event data to Participant object.");
            return;
        }

        try
        {
            await SendToCreateParticipantScreeningProfileAsync(participant);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create participant screening profile.");
        }
    }

    private async Task<DemographicsData> GetDemographicsDataAsync(long NhsNumber)
    {
        var baseDemographicsServiceUrl = Environment.GetEnvironmentVariable("DemographicsServiceUrl");
        var demographicsServiceUrl = $"{baseDemographicsServiceUrl}?nhs_number={NhsNumber}";
        _logger.LogInformation("Requesting demographic service URL: {Url}", demographicsServiceUrl);

        DemographicsData demographicsData;

        var demographicsResponse = await _httpRequestService.SendGet(demographicsServiceUrl);
        demographicsResponse.EnsureSuccessStatusCode();

        var demographicsJson = await demographicsResponse.Content.ReadAsStringAsync();
        _logger.LogInformation("Demographics data retrieved");
        demographicsData = JsonSerializer.Deserialize<DemographicsData>(demographicsJson);

        return demographicsData;
    }

    private async Task<ScreeningLkp> GetScreeningDataAsync(long ScreeningId)
    {
        var baseScreeningDataServiceUrl = Environment.GetEnvironmentVariable("GetScreeningDataUrl");
        var getScreeningDataUrl = $"{baseScreeningDataServiceUrl}?screening_id={ScreeningId}";
        _logger.LogInformation("Requesting screening data from {Url}", getScreeningDataUrl);

        ScreeningLkp screeningLkp;

        var response = await _httpRequestService.SendGet(getScreeningDataUrl);
        response.EnsureSuccessStatusCode();

        var screeningDataJson = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Screening data retrieved successfully.");

        screeningLkp = JsonSerializer.Deserialize<ScreeningLkp>(screeningDataJson);

        return screeningLkp;
    }

    private async Task SendToCreateParticipantScreeningProfileAsync(ParticipantDto participant)
    {
        DemographicsData demographicsData = await GetDemographicsDataAsync(participant.NhsNumber);
        ScreeningLkp screeningLkp = await GetScreeningDataAsync(participant.ScreeningId);

        var screeningProfile = new ParticipantScreeningProfile
        {
            NhsNumber = participant.NhsNumber,
            ScreeningName = screeningLkp.ScreeningName,
            PrimaryCareProvider = demographicsData.PrimaryCareProvider,
            PreferredLanguage = demographicsData.PreferredLanguage,
            ReasonForRemoval = String.Empty,
            ReasonForRemovalDt = new DateOnly(),
            NextTestDueDate = participant.NextTestDueDate,
            NextTestDueDateCalcMethod = participant.NextTestDueDateCalculationMethod,
            ParticipantScreeningStatus = participant.ParticipantScreeningStatus,
            ScreeningCeasedReason = participant.ScreeningCeasedReason,
            IsHigherRisk = participant.IsHigherRisk,
            IsHigherRiskActive = participant.IsHigherRiskActive,
            HigherRiskNextTestDueDate = participant.HigherRiskNextTestDueDate,
            HigherRiskReferralReasonCode = participant.HigherRiskReferralReasonCode,
            HrReasonCodeDescription = String.Empty,
            DateIrradiated = participant.DateIrradiated,
            GeneCode = participant.GeneCode,
            GeneCodeDescription = String.Empty,
            RecordInsertDatetime = DateTime.Now
        };

        var screeningProfileUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl");

        string serializedParticipantScreeningProfile = JsonSerializer.Serialize(screeningProfile);

        _logger.LogInformation("Sending ParticipantScreeningProfile Profile to {Url}: {Request}", screeningProfileUrl, serializedParticipantScreeningProfile);

        await _httpRequestService.SendPost(screeningProfileUrl, serializedParticipantScreeningProfile);
    }
}
