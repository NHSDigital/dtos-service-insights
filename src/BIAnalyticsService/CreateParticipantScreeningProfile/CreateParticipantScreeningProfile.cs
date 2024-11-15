using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using Grpc.Net.Client.Balancer;
using System.Globalization;

namespace NHS.ServiceInsights.BIAnalyticsService;

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

        Participant participant;

        try
        {
            var participantResponse = await _httpRequestService.SendGet(participantUrl);

            if (!participantResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve participant data with NHS number {nhsNumber}. Status Code: {participantResponse.StatusCode}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var participantJson = await participantResponse.Content.ReadAsStringAsync();
            participant = JsonSerializer.Deserialize<Participant>(participantJson);
            _logger.LogInformation("Participant data retrieved and deserialised");
        }

        catch (Exception ex)
        {
            _logger.LogError("Failed to deserialise or retrieve participant from {participantUrl}. \nException: {ex}", participantUrl, ex);
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

    private async Task<DemographicsData> GetDemographicsDataAsync(string nhsNumber)
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

    private async Task SendToCreateParticipantScreeningProfileAsync(Participant participant)
    {
        DemographicsData demographicsData = await GetDemographicsDataAsync(participant.nhs_number);

        var screeningProfile = new ParticipantScreeningProfile
        {
            NhsNumber = long.TryParse(participant.nhs_number, out long num) ? num : 0,
            ScreeningName = String.Empty,
            PrimaryCareProvider = demographicsData.PrimaryCareProvider,
            PreferredLanguage = demographicsData.PreferredLanguage,
            ReasonForRemoval = participant.removal_reason,
            ReasonForRemovalDt = new DateOnly(),
            NextTestDueDate = DateOnly.ParseExact(participant.next_test_due_date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            NextTestDueDateCalcMethod = participant.ntdd_calculation_method,
            ParticipantScreeningStatus = participant.subject_status_code,
            ScreeningCeasedReason = String.Empty,
            IsHigherRisk = (participant.is_higher_risk == "True") ? (short)1 : (short)0,
            IsHigherRiskActive = (participant.is_higher_risk_active == "True") ? (short)1 : (short)0,
            HigherRiskNextTestDueDate = DateOnly.ParseExact(participant.higher_risk_next_test_due_date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            HigherRiskReferralReasonCode = participant.higher_risk_referral_reason_code,
            HrReasonCodeDescription = String.Empty,
            DateIrradiated = DateOnly.ParseExact(participant.date_irradiated, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            GeneCode = participant.gene_code,
            GeneCodeDescription = String.Empty,
            RecordInsertDatetime = DateTime.Now
        };

        var screeningProfileUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl");

        string serializedParticipantScreeningProfile = JsonSerializer.Serialize(screeningProfile);

        _logger.LogInformation($"Sending ParticipantScreeningProfile Profile to {screeningProfileUrl}: {serializedParticipantScreeningProfile}");

        await _httpRequestService.SendPost(screeningProfileUrl, serializedParticipantScreeningProfile);
    }
}