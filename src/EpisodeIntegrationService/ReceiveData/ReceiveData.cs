using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using CsvHelper;
using System.Globalization;
using Azure.Messaging.EventGrid;

namespace NHS.ServiceInsights.EpisodeIntegrationService;

public class ReceiveData
{
    private readonly ILogger<ReceiveData> _logger;
    private readonly IHttpRequestService _httpRequestService;
    private readonly EventGridPublisherClient _eventGridPublisherClient;
    private readonly string[] episodesExpectedHeaders = new[] { "nhs_number", "episode_id", "episode_type", "change_db_date_time", "episode_date", "appointment_made", "date_of_foa", "date_of_as", "early_recall_date", "call_recall_status_authorised_by", "end_code", "end_code_last_updated", "bso_organisation_code", "bso_batch_id", "reason_closed_code", "end_point", "final_action_code" };
    private readonly string[] subjectsExpectedHeaders = new[] { "change_db_date_time", "nhs_number", "superseded_nhs_number", "gp_practice_code", "bso_organisation_code", "next_test_due_date", "subject_status_code", "early_recall_date", "latest_invitation_date", "removal_reason", "removal_date", "reason_for_ceasing_code", "is_higher_risk", "higher_risk_next_test_due_date", "hr_recall_due_date", "higher_risk_referral_reason_code", "date_irradiated", "is_higher_risk_active", "gene_code", "ntdd_calculation_method", "preferred_language" };

    private int participantSuccessCount = 0;
    private int participantFailureCount = 0;
    private int participantRowIndex = 0;

    private int episodeSuccessCount = 0;
    private int episodeFailureCount = 0;
    private int episodeRowIndex = 0;

    public ReceiveData(ILogger<ReceiveData> logger, IHttpRequestService httpRequestService, EventGridPublisherClient eventGridPublisherClient)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
        _eventGridPublisherClient = eventGridPublisherClient;

    }

    [Function("ReceiveData")]
    public async Task Run([BlobTrigger("inbound/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, string name)
    {

        try
        {
            DateTime processingStart = DateTime.UtcNow;

            _logger.LogInformation("C# HTTP trigger function ReceiveData received a request.");

            if (!name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Invalid file extension. Only .csv files are supported.");
                return;
            }

            var (episodeUrl, participantUrl) = GetConfigurationUrls();
            if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
            {
                _logger.LogError("One or both URLs are not configured");
                return;
            }

            if (name.StartsWith("bss_episodes") || name.EndsWith("_historic.csv"))
            {
                if (!CheckCsvFileHeaders(myBlob, FileType.Episodes))
                {
                    _logger.LogError("Episodes CSV file headers are invalid.");
                    return;
                }

                using (var reader = new StreamReader(myBlob))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var episodesEnumerator = csv.GetRecords<BssEpisode>();
                    if (name.EndsWith("_historic.csv"))
                    {
                        var referenceData = await RetrieveReferenceDataAsync();
                        var organisationReferenceData = await GetOrganisationIdAsync();
                        await ProcessHistoricalEpisodeDataAsync(episodesEnumerator, referenceData, organisationReferenceData);

                    }
                    else
                    {
                        await ProcessEpisodeDataAsync(name, episodesEnumerator, episodeUrl);
                    }
                }

                DateTime processingEnd = DateTime.UtcNow;

                _logger.LogInformation("===============================================================================\n"
                                +"Episode Data: File {name} processed successfully.\n"
                                +"Start Time: {processingStart}, End Time: {processingEnd}.\n"
                                +"Rows Processed: {episodeRowIndex}, Success: {episodesuccessCount}, Failures: {episodefailureCount}"
                                ,name,processingStart,processingEnd,episodeRowIndex,episodeSuccessCount, episodeFailureCount );

            }

            else if (name.StartsWith("bss_subjects"))
            {
                if (!CheckCsvFileHeaders(myBlob, FileType.Subjects))
                {
                    _logger.LogError("Subjects CSV file headers are invalid.");
                    return;
                }

                using (var reader = new StreamReader(myBlob))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var participantsEnumerator = csv.GetRecords<BssSubject>();

                    await ProcessParticipantDataAsync(name,participantsEnumerator, participantUrl);
                }

                DateTime processingEnd = DateTime.UtcNow;

                _logger.LogInformation("==================================================================\n"
                                +"Participant Data: File {name} processed successfully.\n"
                                +"Start Time: {processingStart}, End Time: {processingEnd}.\n"
                                +"Rows Processed: {participantRowIndex}, Success: {participantSuccessCount}, Failures: {participantFailureCount}"
                                ,name,processingStart,processingEnd,participantRowIndex,participantSuccessCount, participantFailureCount );
            }
            else
            {
                _logger.LogError("fileName is invalid. file name: {Name}", name);
                return;
            }

        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReceiveData: {Message} \n StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
        }
    }

    private static (string episodeUrl, string participantUrl) GetConfigurationUrls()
    {
        return (Environment.GetEnvironmentVariable("EpisodeManagementUrl"), Environment.GetEnvironmentVariable("ParticipantManagementUrl"));
    }

    private bool CheckCsvFileHeaders(Stream requestBody, FileType fileType)
    {
        string[] expectedHeaders = { "" };
        if (fileType == FileType.Episodes)
        {
            expectedHeaders = episodesExpectedHeaders;
        }
        else if (fileType == FileType.Subjects)
        {
            expectedHeaders = subjectsExpectedHeaders;
        }

        using (var reader = new StreamReader(requestBody, leaveOpen: true))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();

            var actualHeaders = csv.Context.Reader.HeaderRecord;

            if (!actualHeaders.SequenceEqual(expectedHeaders))
            {
                return false;
            }

            requestBody.Position = 0;
            return true;
        }
    }

    private async Task ProcessEpisodeDataAsync(string name,IEnumerable<BssEpisode> episodes, string episodeUrl)
    {

        try
        {
            _logger.LogInformation("Processing episode data.");
            foreach (var episode in episodes)
            {
                var modifiedEpisode = MapEpisodeToEpisodeDto(episode);
                string serializedEpisode = JsonSerializer.Serialize(modifiedEpisode);

                _logger.LogInformation("Sending Episode to {Url}: {Request}", episodeUrl, serializedEpisode);

                await _httpRequestService.SendPost(episodeUrl, serializedEpisode);

                episodeSuccessCount++;
                episodeRowIndex++;
                _logger.LogInformation("Row No.{rowIndex} processed successfully",episodeRowIndex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessEpisodeDataAsync: {Message}", ex.Message);

            episodeFailureCount++;
            episodeRowIndex++;
            _logger.LogInformation("Row No.{rowIndex} processed unsuccessfully",episodeRowIndex);
            await ProcessEpisodeDataAsync(name,episodes, episodeUrl);
        }
    }


    private async Task ProcessHistoricalEpisodeDataAsync(IEnumerable<BssEpisode> episodes, EpisodeReferenceData referenceData, OrganisationReferenceData organisationReferenceData)
    {
        _logger.LogInformation("Processing historical episode data.");

        foreach (var episode in episodes)
        {
            try
            {
                var modifiedEpisode = await MapHistoricalEpisodeToEpisodeDto(episode, referenceData, organisationReferenceData);
                EventGridEvent eventGridEvent = new EventGridEvent(
                    subject: "Episode Created",
                    eventType: "CreateParticipantScreeningEpisode",
                    dataVersion: "1.0",
                    data: modifiedEpisode
                );
                _logger.LogInformation("Sending event to Event Grid: {EventGridEvent}", JsonSerializer.Serialize(eventGridEvent));
                await _eventGridPublisherClient.SendEventAsync(eventGridEvent);
                episodeSuccessCount++;
                episodeRowIndex++;
                _logger.LogInformation("Row No.{rowIndex} processed successfully",episodeRowIndex);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessHistoricalEpisodeDataAsync: {Message}", ex.Message);

                episodeFailureCount++;
                episodeRowIndex++;
                _logger.LogInformation("Row No.{rowIndex} processed unsuccessfully",episodeRowIndex);
            }
        }
    }


    private InitialEpisodeDto MapEpisodeToEpisodeDto(BssEpisode episode)
    {
        return new InitialEpisodeDto
        {
            EpisodeId = episode.episode_id,
            EpisodeType = episode.episode_type,
            ScreeningName = "Breast Screening",
            NhsNumber = episode.nhs_number,
            SrcSysProcessedDateTime = episode.change_db_date_time,
            EpisodeOpenDate = Utils.ParseNullableDate(episode.episode_date),
            AppointmentMadeFlag = Utils.ParseBooleanStringToShort(episode.appointment_made),
            FirstOfferedAppointmentDate = Utils.ParseNullableDate(episode.date_of_foa),
            ActualScreeningDate = Utils.ParseNullableDate(episode.date_of_as),
            EarlyRecallDate = Utils.ParseNullableDate(episode.early_recall_date),
            CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
            EndCode = episode.end_code,
            EndCodeLastUpdated = Utils.ParseNullableDateTime(episode.end_code_last_updated, "yyyy-MM-dd HH:mm:ssz"),
            OrganisationCode = episode.bso_organisation_code,
            BatchId = episode.bso_batch_id,
            EndPoint = episode.end_point,
            ReasonClosedCode = episode.reason_closed_code,
            FinalActionCode = episode.final_action_code
        };
    }
    private async Task<FinalizedEpisodeDto> MapHistoricalEpisodeToEpisodeDto(BssEpisode episode, EpisodeReferenceData referenceData, OrganisationReferenceData organisationReferenceData)
    {
        var finalizedEpisodeDto = new FinalizedEpisodeDto
        {
            EpisodeId = episode.episode_id,
            NhsNumber = episode.nhs_number,
            ScreeningId = 1, // Hardcoded to 1 for now because we only have one screening type (Breast Screening)
            EpisodeType = episode.episode_type,
            EpisodeTypeDescription = string.IsNullOrEmpty(episode.episode_type) ? "" : referenceData.EpisodeTypeToIdLookup[episode.episode_type],
            EpisodeOpenDate = Utils.ParseNullableDate(episode.episode_date),
            AppointmentMadeFlag = Utils.ParseBooleanStringToShort(episode.appointment_made),
            FirstOfferedAppointmentDate = Utils.ParseNullableDate(episode.date_of_foa),
            ActualScreeningDate = Utils.ParseNullableDate(episode.date_of_as),
            EarlyRecallDate = Utils.ParseNullableDate(episode.early_recall_date),
            CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
            EndCode = episode.end_code,
            EndCodeDescription = string.IsNullOrEmpty(episode.end_code) ? "" : referenceData.EndCodeToIdLookup[episode.end_code],
            EndCodeLastUpdated = Utils.ParseNullableDateTime(episode.end_code_last_updated, "yyyy-MM-dd HH:mm:ssz"),
            FinalActionCode = episode.final_action_code,
            FinalActionCodeDescription = string.IsNullOrEmpty(episode.final_action_code) ? "" : referenceData.FinalActionCodeToIdLookup[episode.final_action_code],
            ReasonClosedCode = episode.reason_closed_code,
            ReasonClosedCodeDescription = string.IsNullOrEmpty(episode.reason_closed_code) ? "" : referenceData.ReasonClosedCodeToIdLookup[episode.reason_closed_code],
            EndPoint = episode.end_point,
            OrganisationId = string.IsNullOrEmpty(episode.bso_organisation_code) ? null : organisationReferenceData.OrganisationCodeToIdLookup[episode.bso_organisation_code],
            BatchId = episode.bso_batch_id,
            SrcSysProcessedDatetime = episode.change_db_date_time
        };

        return finalizedEpisodeDto;
    }


    private async Task<EpisodeReferenceData> RetrieveReferenceDataAsync()
    {
        var url = Environment.GetEnvironmentVariable("GetEpisodeReferenceDataServiceUrl");

        try
        {
            var response = await _httpRequestService.SendGet(url);

            response.EnsureSuccessStatusCode();

            var referenceDataJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EpisodeReferenceData>(referenceDataJson);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve episode reference data from url: {Url}", url);
            throw;
        }
    }


    private async Task ProcessParticipantDataAsync(string name,IEnumerable<BssSubject> subjects, string participantUrl)
    {

        try
        {
            _logger.LogInformation("Processing participant data.");

            foreach (var subject in subjects)
            {
                var modifiedParticipant = MapParticipantToParticipantDto(subject);
                string serializedParticipant = JsonSerializer.Serialize(modifiedParticipant);

                _logger.LogInformation("Sending participant to {Url}: {Request}", participantUrl, serializedParticipant);

                await _httpRequestService.SendPost(participantUrl, serializedParticipant);

                participantSuccessCount++;
                participantRowIndex++;
                _logger.LogInformation("Row No.{rowIndex} processed successfully",participantRowIndex);
            }
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessParticipantDataAsync: {Message}", ex.Message);

            participantFailureCount++;
            participantRowIndex++;
            _logger.LogInformation("Row No.{rowIndex} processed unsuccessfully",participantRowIndex);
            await ProcessParticipantDataAsync(name,subjects, participantUrl);
        }
    }
    private InitialParticipantDto MapParticipantToParticipantDto(BssSubject subject)
    {
        return new InitialParticipantDto
        {
            NhsNumber = subject.nhs_number,
            ScreeningName = "Breast Screening",
            NextTestDueDate = Utils.ParseNullableDate(subject.next_test_due_date),
            NextTestDueDateCalculationMethod = subject.ntdd_calculation_method,
            ParticipantScreeningStatus = subject.subject_status_code,
            ScreeningCeasedReason = subject.reason_for_ceasing_code,
            IsHigherRisk = Utils.ParseBooleanStringToShort(subject.is_higher_risk),
            IsHigherRiskActive = Utils.ParseBooleanStringToShort(subject.is_higher_risk_active),
            SrcSysProcessedDateTime = subject.change_db_date_time,
            HigherRiskNextTestDueDate = Utils.ParseNullableDate(subject.higher_risk_next_test_due_date),
            HigherRiskReferralReasonCode = subject.higher_risk_referral_reason_code,
            DateIrradiated = Utils.ParseNullableDate(subject.date_irradiated),
            GeneCode = subject.gene_code
        };
    }

    private async Task<OrganisationReferenceData> GetOrganisationIdAsync()
    {
        var url = Environment.GetEnvironmentVariable("GetAllOrganisationReferenceDataUrl");
        var response = await _httpRequestService.SendGet(url);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<OrganisationReferenceData>(await response.Content.ReadAsStreamAsync());

    }

}

