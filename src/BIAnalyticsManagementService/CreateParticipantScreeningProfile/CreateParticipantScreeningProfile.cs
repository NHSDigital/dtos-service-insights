using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
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

    [Function("CreateParticipantScreeningProfile")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Create Participant Screening Profile function start");

        string nhsNumber = req.Query["nhs_number"];

        if (string.IsNullOrEmpty(nhsNumber))
        {
            _logger.LogError("nhsNumber is null or empty.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var baseParticipantUrl = Environment.GetEnvironmentVariable("GetParticipantUrl");
        var participantUrl = $"{baseParticipantUrl}?nhs_number={nhsNumber}";
        _logger.LogInformation("Requesting participant URL: {Url}", participantUrl);

        ParticipantDto participant;

        try
        {
            var participantResponse = await _httpRequestService.SendGet(participantUrl);

            if (!participantResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve participant data with NHS number {NhsNumber}. Status Code: {StatusCode}", nhsNumber, participantResponse.StatusCode);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var participantJson = await participantResponse.Content.ReadAsStringAsync();
            participant = JsonSerializer.Deserialize<ParticipantDto>(participantJson);
            _logger.LogInformation("Participant data retrieved and deserialised");
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialise or retrieve participant from {participantUrl}.", participantUrl);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        try
        {
            await SendToCreateParticipantScreeningProfileAsync(participant);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create participant screening profile.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private async Task<DemographicsData> GetDemographicsDataAsync(long nhsNumber)
    {
        var baseDemographicsServiceUrl = Environment.GetEnvironmentVariable("DemographicsServiceUrl");
        var demographicsServiceUrl = $"{baseDemographicsServiceUrl}?nhs_number={nhsNumber}";
        _logger.LogInformation("Requesting demographic service URL: {Url}", demographicsServiceUrl);

        DemographicsData demographicsData;

        var demographicsResponse = await _httpRequestService.SendGet(demographicsServiceUrl);
        demographicsResponse.EnsureSuccessStatusCode();

        var demographicsJson = await demographicsResponse.Content.ReadAsStringAsync();
        _logger.LogInformation("Demographics data retrieved");
        demographicsData = JsonSerializer.Deserialize<DemographicsData>(demographicsJson);

        return demographicsData;
    }

    private async Task SendToCreateParticipantScreeningProfileAsync(ParticipantDto participant)
    {
        DemographicsData demographicsData = await GetDemographicsDataAsync(participant.NhsNumber);

        var screeningProfile = new ParticipantScreeningProfile
        {
            NhsNumber = participant.NhsNumber,
            ScreeningName = String.Empty,
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
