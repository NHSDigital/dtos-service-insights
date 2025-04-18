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

        // Retrieve and validate required environment variables
        var screeningProfileUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl");
        var demographicsServiceUrl = Environment.GetEnvironmentVariable("DemographicsServiceUrl");
        var screeningDataServiceUrl = Environment.GetEnvironmentVariable("GetScreeningDataUrl");

        if (string.IsNullOrEmpty(demographicsServiceUrl))
        {
            throw new InvalidOperationException("Environment variable 'DemographicsServiceUrl' is missing.");
        }

        if (string.IsNullOrEmpty(screeningDataServiceUrl))
        {
            throw new InvalidOperationException("Environment variable 'GetScreeningDataUrl' is missing.");
        }

        if (string.IsNullOrEmpty(screeningProfileUrl))
        {
            throw new InvalidOperationException("Environment variable 'CreateParticipantScreeningProfileUrl' is missing.");
        }

    }

    [Function(nameof(CreateParticipantScreeningProfile))]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Create Participant Screening Profile function start");

        string serializedEvent = JsonSerializer.Serialize(eventGridEvent);
        _logger.LogInformation(serializedEvent);

        FinalizedParticipantDto participant;

        try
        {
            participant = JsonSerializer.Deserialize<FinalizedParticipantDto>(eventGridEvent.Data.ToString());
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to deserialize event data to Participant object.");
            return;
        }

        try
        {
            DateTime historicDataCutOffDate = new DateTime(2025, 03, 01, 0, 0, 0, DateTimeKind.Utc);

            bool isHistoric = participant.SrcSysProcessedDatetime < historicDataCutOffDate;

            if (isHistoric)
            {
                _logger.LogInformation("Data is historic.");
            }

            else
            {
                _logger.LogInformation("Data is not historic.");
            }

            await SendToCreateParticipantScreeningProfileAsync(participant, isHistoric);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create participant screening profile.");
        }
    }

    private async Task<DemographicsData> GetDemographicsDataAsync(long nhsNumber)
    {
        var baseDemographicsServiceUrl = Environment.GetEnvironmentVariable("DemographicsServiceUrl");
        var demographicsServiceUrl = $"{baseDemographicsServiceUrl}?nhs_number={nhsNumber}";
        _logger.LogInformation("Requesting demographic service URL: {Url}", demographicsServiceUrl);

        try
        {
            var demographicsResponse = await _httpRequestService.SendGet(demographicsServiceUrl);
            demographicsResponse.EnsureSuccessStatusCode();

            var demographicsJson = await demographicsResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Demographics data retrieved successfully: {DemographicsJson}", demographicsJson);

            return JsonSerializer.Deserialize<DemographicsData>(demographicsJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve demographics data. NHS Number: {NhsNumber}, Service: {demographicsServiceUrl}, Timestamp: {Timestamp}",
                nhsNumber, demographicsServiceUrl, DateTime.UtcNow);

            throw new HttpRequestException($"Failed to retrieve demographics data from {baseDemographicsServiceUrl}", ex);
        }
    }

    private async Task<ScreeningLkp> GetScreeningDataAsync(long screeningId)
    {
        var baseScreeningDataServiceUrl = Environment.GetEnvironmentVariable("GetScreeningDataUrl");
        var getScreeningDataUrl = $"{baseScreeningDataServiceUrl}?screening_id={screeningId}";
        _logger.LogInformation("Requesting screening data from {Url}", getScreeningDataUrl);

        try
        {
            var response = await _httpRequestService.SendGet(getScreeningDataUrl);
            response.EnsureSuccessStatusCode();

            var screeningDataJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Screening data retrieved successfully: {ScreeningDataJson}", screeningDataJson);

            return JsonSerializer.Deserialize<ScreeningLkp>(screeningDataJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve screening data. Screening ID: {ScreeningId}, Service: {getScreeningDataUrl}, Timestamp: {Timestamp}",
                screeningId, getScreeningDataUrl, DateTime.UtcNow);

            throw new HttpRequestException($"Failed to retrieve screening data from {baseScreeningDataServiceUrl}", ex);
        }
    }

    private async Task SendToCreateParticipantScreeningProfileAsync(FinalizedParticipantDto participant, bool isHistoric)
    {
        DemographicsData demographicsData = await GetDemographicsDataAsync(participant.NhsNumber);
        ScreeningLkp screeningLkp = await GetScreeningDataAsync(participant.ScreeningId);

        var screeningProfile = new ParticipantScreeningProfile
        {
            NhsNumber = participant.NhsNumber,
            ScreeningName = screeningLkp.ScreeningName,
            PrimaryCareProvider = demographicsData.PrimaryCareProvider,
            PreferredLanguage = demographicsData.PreferredLanguage,
            ReasonForRemoval = participant.ReasonForRemoval,
            ReasonForRemovalDt = participant.ReasonForRemovalDt,
            NextTestDueDate = participant.NextTestDueDate,
            NextTestDueDateCalcMethod = participant.NextTestDueDateCalculationMethod,
            ParticipantScreeningStatus = participant.ParticipantScreeningStatus,
            ScreeningCeasedReason = participant.ScreeningCeasedReason,
            IsHigherRisk = participant.IsHigherRisk,
            IsHigherRiskActive = participant.IsHigherRiskActive,
            HigherRiskNextTestDueDate = participant.HigherRiskNextTestDueDate,
            HigherRiskReferralReasonCode = participant.HigherRiskReferralReasonCode,
            HrReasonCodeDescription = participant.HigherRiskReasonCodeDescription,
            DateIrradiated = participant.DateIrradiated,
            GeneCode = participant.GeneCode,
            GeneCodeDescription = participant.GeneDescription,
            SrcSysProcessedDatetime = participant.SrcSysProcessedDatetime,
            RecordInsertDatetime = isHistoric ? participant.SrcSysProcessedDatetime.AddDays(1) : DateTime.UtcNow,
            RecordUpdateDatetime = isHistoric ? participant.SrcSysProcessedDatetime.AddDays(1) : DateTime.UtcNow,
        };

        var screeningProfileUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl");

        string serializedParticipantScreeningProfile = JsonSerializer.Serialize(screeningProfile);

        _logger.LogInformation("Sending ParticipantScreeningProfile Profile to {Url}: {Request}", screeningProfileUrl, serializedParticipantScreeningProfile);

        await _httpRequestService.SendPost(screeningProfileUrl, serializedParticipantScreeningProfile);
    }
}
