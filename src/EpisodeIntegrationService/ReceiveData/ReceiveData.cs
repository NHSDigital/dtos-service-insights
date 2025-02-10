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
    private readonly string[] episodesExpectedHeaders = ["nhs_number", "episode_id", "episode_type", "change_db_date_time", "episode_date", "appointment_made", "date_of_foa", "date_of_as", "early_recall_date", "call_recall_status_authorised_by", "end_code", "end_code_last_updated", "bso_organisation_code", "bso_batch_id", "reason_closed_code", "end_point", "final_action_code"];
    private readonly string[] subjectsExpectedHeaders = ["change_db_date_time", "nhs_number", "superseded_nhs_number", "gp_practice_code", "bso_organisation_code", "next_test_due_date", "subject_status_code", "early_recall_date", "latest_invitation_date", "removal_reason", "removal_date", "reason_for_ceasing_code", "is_higher_risk", "higher_risk_next_test_due_date", "hr_recall_due_date", "higher_risk_referral_reason_code", "date_irradiated", "is_higher_risk_active", "gene_code", "ntdd_calculation_method", "preferred_language"];

    private int participantSuccessCount = 0;
    private int participantFailureCount = 0;
    private int participantRowIndex = 0;
    private int episodeSuccessCount = 0;
    private int episodeFailureCount = 0;
    private int episodeRowIndex = 0;
    private const string RowProcessedSuccessfullyMessage = "Row No.{rowIndex} processed successfully";
    private const string RowProcessedUnsuccessfullyMessage = "Row No.{rowIndex} processed unsuccessfully";

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

            if (!IsValidFileExtension(name))
            {
                return;
            }

            var (episodeUrl, participantUrl) = GetConfigurationUrls();
            if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
            {
                _logger.LogError("One or both URLs are not configured");
                return;
            }

            await ProcessFile(myBlob, name, episodeUrl, participantUrl, processingStart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReceiveData: {Message} \n StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
        }
    }
    private bool IsValidFileExtension(string name)
    {
        if (!name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Invalid file extension. Only .csv files are supported.");
            return false;
        }
        return true;
    }
    private async Task ProcessFile(Stream myBlob, string name, string episodeUrl, string participantUrl, DateTime processingStart)
    {
        if (name.StartsWith("bss_episodes"))
        {
            await ProcessEpisodes(myBlob, name, episodeUrl, processingStart);
        }
        else if (name.StartsWith("bss_subjects"))
        {
            await ProcessSubjects(myBlob, name, participantUrl, processingStart);
        }
        else
        {
            _logger.LogError("fileName is invalid. file name: {Name}", name);
        }
    }

    private async Task<bool> ProcessEpisodes(Stream myBlob, string name, string episodeUrl, DateTime processingStart)
    {
        if (!CheckCsvFileHeaders(myBlob, FileType.Episodes))
        {
            _logger.LogError("Episodes CSV file headers are invalid. file name: {Name}", name);
            return false;
        }

        myBlob.Position = 0;

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
                await ProcessEpisodeDataAsync(episodesEnumerator, episodeUrl);
            }
        }

        DateTime processingEnd = DateTime.UtcNow;

        _logger.LogInformation("===============================================================================\n"
                                + "Episode Data: File processed successfully.\n"
                                + "Start Time: {processingStart}, End Time: {processingEnd}.\n"
                                + "Rows Processed: {episodeRowIndex}, Success: {episodeSuccessCount}, Failures: {episodeFailureCount}"
                                , processingStart, processingEnd, episodeRowIndex, episodeSuccessCount, episodeFailureCount);

        return true;
    }

    private async Task<bool> ProcessSubjects(Stream myBlob, string name, string participantUrl, DateTime processingStart)
    {
        if (!CheckCsvFileHeaders(myBlob, FileType.Subjects))
        {
            _logger.LogError("Subjects CSV file headers are invalid. file name: {Name}", name);
            return false;
        }

        myBlob.Position = 0;

        using (var reader = new StreamReader(myBlob))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var participantsEnumerator = csv.GetRecords<BssSubject>();
            if (name.EndsWith("_historic.csv"))
            {
                var participantReferenceData = GetParticipantReferenceData();
                await ProcessHistoricalParticipantDataAsync(participantsEnumerator, participantReferenceData);
            }
            else
            {
                await ProcessParticipantDataAsync(participantsEnumerator, participantUrl);
            }
        }

        DateTime processingEnd = DateTime.UtcNow;

        _logger.LogInformation("==================================================================\n"
                                + "Participant Data: File processed successfully.\n"
                                + "Start Time: {processingStart}, End Time: {processingEnd}.\n"
                                + "Rows Processed: {participantRowIndex}, Success: {participantSuccessCount}, Failures: {participantFailureCount}"
                                , processingStart, processingEnd, participantRowIndex, participantSuccessCount, participantFailureCount);

        return true;
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

        using var reader = new StreamReader(requestBody, leaveOpen: true);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
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

    private async Task ProcessEpisodeDataAsync(IEnumerable<BssEpisode> episodes, string episodeUrl)
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
            episodeFailureCount++;
            episodeRowIndex++;
            _logger.LogError("Row No.{rowIndex} processed unsuccessfully",episodeRowIndex);
            await ProcessEpisodeDataAsync(episodes, episodeUrl);
        }
    }

    private async Task ProcessHistoricalEpisodeDataAsync(IEnumerable<BssEpisode> episodes, EpisodeReferenceData referenceData, OrganisationReferenceData organisationReferenceData)
    {
        try
        {
            _logger.LogInformation("Processing historical episode data.");
            foreach (var episode in episodes)
            {
                var modifiedEpisode = MapHistoricalEpisodeToEpisodeDto(episode, referenceData, organisationReferenceData);
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
                _logger.LogInformation(RowProcessedSuccessfullyMessage, episodeRowIndex);
            }
        }
        catch (Exception ex)
        {
            episodeFailureCount++;
            episodeRowIndex++;
            _logger.LogError(ex, RowProcessedUnsuccessfullyMessage, episodeRowIndex);
            await ProcessHistoricalEpisodeDataAsync(episodes, referenceData, organisationReferenceData);
        }
    }

    private static InitialEpisodeDto MapEpisodeToEpisodeDto(BssEpisode episode)
    {
        Utils.CheckForNullOrEmptyStrings(episode.episode_type, episode.episode_date);
        Utils.ValidateDataValue(episode.appointment_made);
        return new InitialEpisodeDto
        {
            EpisodeId = long.Parse(episode.episode_id),
            EpisodeType = episode.episode_type,
            ScreeningName = "Breast Screening",
            NhsNumber = long.Parse(episode.nhs_number),
            SrcSysProcessedDateTime = DateTime.Parse(episode.change_db_date_time, CultureInfo.InvariantCulture, DateTimeStyles.None),
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

    private static FinalizedEpisodeDto MapHistoricalEpisodeToEpisodeDto(BssEpisode episode, EpisodeReferenceData referenceData, OrganisationReferenceData organisationReferenceData)
    {
        Utils.CheckForNullOrEmptyStrings(episode.episode_type, episode.episode_date);
        Utils.ValidateDataValue(episode.appointment_made);
        var finalizedEpisodeDto = new FinalizedEpisodeDto
        {
            EpisodeId = long.Parse(episode.episode_id),
            NhsNumber = long.Parse(episode.nhs_number),
            ScreeningId = 1, // Hardcoded to 1 for now because we only have one screening type (Breast Screening)
            EpisodeType = episode.episode_type,
            EpisodeTypeDescription = string.IsNullOrEmpty(episode.episode_type) ? "" : referenceData.EpisodeTypeDescriptions[episode.episode_type],
            EpisodeOpenDate = Utils.ParseNullableDate(episode.episode_date),
            AppointmentMadeFlag = Utils.ParseBooleanStringToShort(episode.appointment_made),
            FirstOfferedAppointmentDate = Utils.ParseNullableDate(episode.date_of_foa),
            ActualScreeningDate = Utils.ParseNullableDate(episode.date_of_as),
            EarlyRecallDate = Utils.ParseNullableDate(episode.early_recall_date),
            CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
            EndCode = episode.end_code,
            EndCodeDescription = string.IsNullOrEmpty(episode.end_code) ? "" : referenceData.EndCodeDescriptions[episode.end_code],
            EndCodeLastUpdated = Utils.ParseNullableDateTime(episode.end_code_last_updated, "yyyy-MM-dd HH:mm:ssz"),
            FinalActionCode = episode.final_action_code,
            FinalActionCodeDescription = string.IsNullOrEmpty(episode.final_action_code) ? "" : referenceData.FinalActionCodeDescriptions[episode.final_action_code],
            ReasonClosedCode = episode.reason_closed_code,
            ReasonClosedCodeDescription = string.IsNullOrEmpty(episode.reason_closed_code) ? "" : referenceData.ReasonClosedCodeDescriptions[episode.reason_closed_code],
            EndPoint = episode.end_point,
            OrganisationId = string.IsNullOrEmpty(episode.bso_organisation_code) ? null : organisationReferenceData.OrganisationCodeToIdLookup[episode.bso_organisation_code],
            BatchId = episode.bso_batch_id,
            SrcSysProcessedDatetime = DateTime.Parse(episode.change_db_date_time, CultureInfo.InvariantCulture, DateTimeStyles.None)
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
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Failed to retrieve episode reference data from {url}", ex);
        }

    }

    private async Task ProcessParticipantDataAsync(IEnumerable<BssSubject> subjects, string participantUrl)
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
            participantFailureCount++;
            participantRowIndex++;
            _logger.LogError("Row No.{rowIndex} processed unsuccessfully",participantRowIndex);
            await ProcessParticipantDataAsync(subjects, participantUrl);
        }
    }

    private async Task ProcessHistoricalParticipantDataAsync(IEnumerable<BssSubject> subjects, ParticipantReferenceData participantReferenceData)
    {

        try
        {
            _logger.LogInformation("Processing historical participant data.");

            foreach (var subject in subjects)
            {
                var modifiedParticipant = MapHistoricalParticipantToParticipantDto(subject, participantReferenceData);
                EventGridEvent eventGridEvent = new EventGridEvent(
                    subject: "Participant Created",
                    eventType: "CreateParticipantScreeningProfile",
                    dataVersion: "1.0",
                    data: modifiedParticipant
                );

                _logger.LogInformation("Sending event to Event Grid: {EventGridEvent}", JsonSerializer.Serialize(eventGridEvent));
                await _eventGridPublisherClient.SendEventAsync(eventGridEvent);
                participantSuccessCount++;
                participantRowIndex++;
                _logger.LogInformation(RowProcessedSuccessfullyMessage, participantRowIndex);
            }
        }
        catch (Exception ex)
        {
            participantFailureCount++;
            participantRowIndex++;
            _logger.LogError(ex, RowProcessedUnsuccessfullyMessage, participantRowIndex);
            await ProcessHistoricalParticipantDataAsync(subjects, participantReferenceData);
        }
    }

    private static InitialParticipantDto MapParticipantToParticipantDto(BssSubject subject)
    {
        return new InitialParticipantDto
        {
            NhsNumber = long.Parse(subject.nhs_number),
            ScreeningName = "Breast Screening",
            NextTestDueDate = Utils.ParseNullableDate(subject.next_test_due_date),
            NextTestDueDateCalculationMethod = subject.ntdd_calculation_method,
            ParticipantScreeningStatus = subject.subject_status_code,
            ScreeningCeasedReason = subject.reason_for_ceasing_code,
            IsHigherRisk = Utils.ParseBooleanStringToShort(subject.is_higher_risk),
            IsHigherRiskActive = Utils.ParseBooleanStringToShort(subject.is_higher_risk_active),
            SrcSysProcessedDateTime = DateTime.Parse(subject.change_db_date_time, CultureInfo.InvariantCulture, DateTimeStyles.None),
            HigherRiskNextTestDueDate = Utils.ParseNullableDate(subject.higher_risk_next_test_due_date),
            HigherRiskReferralReasonCode = subject.higher_risk_referral_reason_code,
            DateIrradiated = Utils.ParseNullableDate(subject.date_irradiated),
            GeneCode = subject.gene_code
        };
    }

    private static FinalizedParticipantDto MapHistoricalParticipantToParticipantDto(BssSubject subject, ParticipantReferenceData participantReferenceData)
    {
        return new FinalizedParticipantDto
        {
            NhsNumber = long.Parse(subject.nhs_number),
            ScreeningId = 1, // Hardcoded to 1 for now because we only have one screening type (Breast Screening)
            ReasonForRemoval = subject.removal_reason,
            ReasonForRemovalDt = Utils.ParseNullableDate(subject.removal_date),
            NextTestDueDate = Utils.ParseNullableDate(subject.next_test_due_date),
            NextTestDueDateCalculationMethod = subject.ntdd_calculation_method,
            ParticipantScreeningStatus = subject.subject_status_code,
            ScreeningCeasedReason = subject.reason_for_ceasing_code,
            IsHigherRisk = Utils.ParseBooleanStringToShort(subject.is_higher_risk),
            IsHigherRiskActive = Utils.ParseBooleanStringToShort(subject.is_higher_risk_active),
            SrcSysProcessedDatetime = DateTime.Parse(subject.change_db_date_time, CultureInfo.InvariantCulture, DateTimeStyles.None),
            HigherRiskNextTestDueDate = Utils.ParseNullableDate(subject.higher_risk_next_test_due_date),
            HigherRiskReferralReasonCode = subject.higher_risk_referral_reason_code,
            HigherRiskReasonCodeDescription = string.IsNullOrEmpty(subject.higher_risk_referral_reason_code) ? "" : participantReferenceData.HigherRiskReferralReasonCodeDescriptions[subject.higher_risk_referral_reason_code],
            DateIrradiated = Utils.ParseNullableDate(subject.date_irradiated),
            GeneCode = subject.gene_code,
            GeneDescription = string.IsNullOrEmpty(subject.gene_code) ? "" : participantReferenceData.GeneCodeDescriptions[subject.gene_code]
        };
    }

    private async Task<OrganisationReferenceData> GetOrganisationIdAsync()
    {
        var url = Environment.GetEnvironmentVariable("GetAllOrganisationReferenceDataUrl");
        var response = await _httpRequestService.SendGet(url);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<OrganisationReferenceData>(await response.Content.ReadAsStreamAsync());

    }

    private static ParticipantReferenceData GetParticipantReferenceData()
    {
        var geneCodeDescriptions = new Dictionary<string, string>
        {
            {"BRCA1", "BRCA1"},
            {"BRCA2", "BRCA2"},
            {"CDH1", "CDH1 (E-Cadherin)"},
            {"OTHER", "Other"},
            {"PALB2", "PALB2"},
            {"PTEN", "PTEN"},
            {"STK11", "STK11"}
        };

        var higherRiskReferralReasonCodeDescriptions = new Dictionary<string, string>
        {
            {"AT_HETEROZYGOTES", "A-T heterozygotes"},
            {"AT_HOMOZYGOTES", "A-T homozygotes"},
            {"BRCA_RISK", "BRCA1/BRCA2/PALB2 (8% 10-year risk)"},
            {"BRCA_TESTED", "BRCA1/BRCA2/PALB2 Tested"},
            {"HR_GENE_UNTESTED", "HR Gene untested"},
            {"OTHER_GENE_MUTATIONS", "Other gene mutations"},
            {"RADIOTHERAPY_BELOW_30", "Radiotherapy below aged 30"},
            {"RADIOTHERAPY_LOWER", "Radiotherapy to breast tissue aged 10-19"},
            {"RADIOTHERAPY_UPPER", "Radiotherapy to breast tissue aged 20-35"},
            {"RISK_EQUIVALENT", "Risk equivalent, not tested"},
            {"TP53", "TP53 (Li-Fraumeni) syndrome"}
        };

        var participantReferenceData = new ParticipantReferenceData
        {
            GeneCodeDescriptions = geneCodeDescriptions,
            HigherRiskReferralReasonCodeDescriptions = higherRiskReferralReasonCodeDescriptions,
        };

        return participantReferenceData;
    }
}

