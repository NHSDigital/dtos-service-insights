using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Net;
using CsvHelper;
using System.Globalization;

namespace NHS.ServiceInsights.EpisodeIntegrationService;
public class ProcessData
{
    private readonly ILogger<ProcessData> _logger;
    private readonly IHttpRequestService _httpRequestService;

    private readonly string[] episodesExpectedHeaders = new[] {"nhs_number","episode_id","episode_type","change_db_date_time","episode_date","appointment_made","date_of_foa","date_of_as","early_recall_date","call_recall_status_authorised_by","end_code","end_code_last_updated","bso_organisation_code","bso_batch_id","reason_closed_code","end_point","final_action_code"};

    private readonly string[] subjectsExpectedHeaders = new[] {"change_db_date_time","nhs_number","superseded_nhs_number","gp_practice_code","bso_organisation_code","next_test_due_date","subject_status_code","early_recall_date","latest_invitation_date","removal_reason","removal_date","reason_for_ceasing_code","is_higher_risk","higher_risk_next_test_due_date","hr_recall_due_date","higher_risk_referral_reason_code","date_irradiated","is_higher_risk_active","gene_code","ntdd_calculation_method","preferred_language"};

    public ProcessData(ILogger<ProcessData> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("ProcessData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("C# HTTP trigger function ProcessData received a request.");

            var (episodeUrl, participantUrl) = GetConfigurationUrls();
            if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
            {
                return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "One or both URLs are not configured");
            }

            string? fileName = req.Query["FileName"];

            if (string.IsNullOrEmpty(fileName))
            {
                return CreateErrorResponse(req, HttpStatusCode.BadRequest, "no file name provided");
            }

            if(fileName.StartsWith("episodes"))
            {
                if(!CheckCsvFileHeaders(req.Body, FileType.Episodes))
                {
                    return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Episodes CSV file headers are invalid.");
                }

                using (var reader = new StreamReader(req.Body))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var episodesEnumerator = csv.GetRecords<BssEpisode>();

                    await ProcessEpisodeDataAsync(episodesEnumerator, episodeUrl);
                }
            }
            else if(fileName.StartsWith("subjects"))
            {
                if(!CheckCsvFileHeaders(req.Body, FileType.Subjects))
                {
                    return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Subjects CSV file headers are invalid.");
                }

                using (var reader = new StreamReader(req.Body))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var participantsEnumerator = csv.GetRecords<Participant>();

                    await ProcessParticipantDataAsync(participantsEnumerator, participantUrl);
                }
            }
            else
            {
                return CreateErrorResponse(req, HttpStatusCode.BadRequest, "fileName is invalid. file name: " + fileName);
            }

            _logger.LogInformation("Data processed successfully.");
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(req, HttpStatusCode.InternalServerError, $"Error in ProcessData: {ex.Message} \n StackTrace: {ex.StackTrace}");
        }
    }

    private (string episodeUrl, string participantUrl) GetConfigurationUrls()
    {
        return (Environment.GetEnvironmentVariable("EpisodeManagementUrl"), Environment.GetEnvironmentVariable("ParticipantManagementUrl"));
    }

    private bool CheckCsvFileHeaders(Stream requestBody, FileType fileType)
    {
        string[] expectedHeaders = {""};
        if(fileType == FileType.Episodes){
            expectedHeaders = episodesExpectedHeaders;
        }
        else if(fileType == FileType.Subjects){
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
    private HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        _logger.LogError(message);
        var response = req.CreateResponse(statusCode);
        response.WriteString(message);
        return response;
    }

    private async Task ProcessEpisodeDataAsync(IEnumerable<BssEpisode> episodes, string episodeUrl)
    {
        try
        {
            _logger.LogInformation("Processing episode data.");
            foreach (var episode in episodes)
            {
                var modifiedEpisode = new Episode
                {
                    EpisodeId = episode.episode_id,
                    EpisodeTypeId = episode.episode_type,
                    EpisodeOpenDate = episode.episode_date,
                    AppointmentMadeFlag = episode.appointment_made,
                    FirstOfferedAppointmentDate = episode.date_of_foa,
                    ActualScreeningDate = episode.date_of_as,
                    EarlyRecallDate = episode.early_recall_date,
                    CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
                    EndCodeId = episode.end_code,
                    EndCodeLastUpdated = episode.end_code_last_updated,
                    OrganisationId = episode.bso_organisation_code,
                    BatchId = episode.bso_batch_id
                };

                string serializedEpisode = JsonSerializer.Serialize(modifiedEpisode, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation($"Sending Episode to {episodeUrl}: {serializedEpisode}");

                await _httpRequestService.SendPost(episodeUrl, serializedEpisode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in ProcessEpisodeDataAsync: " + ex.Message);
            ProcessEpisodeDataAsync(episodes, episodeUrl);
        }
    }

    private async Task ProcessParticipantDataAsync(IEnumerable<Participant> participants, string participantUrl)
    {
        try
        {
            _logger.LogInformation("Processing participant data.");
            foreach (var participant in participants)
            {
                string serializedParticipant = JsonSerializer.Serialize(participant, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation($"Sending participant to {participantUrl}: {serializedParticipant}");

                await _httpRequestService.SendPost(participantUrl, serializedParticipant);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in ProcessParticipantDataAsync: " + ex.Message);
            ProcessParticipantDataAsync(participants, participantUrl);
        }
    }
}

public class BssEpisode
{
    public string episode_id { get; set; } = null!;
    public string nhs_number { get; set; }
    public string? episode_type { get; set; }
    public string? episode_date { get; set; }
    public string? appointment_made { get; set; }
    public string? date_of_foa { get; set; }
    public string? date_of_as { get; set; }
    public string? early_recall_date { get; set; }
    public string? call_recall_status_authorised_by { get; set; }
    public string? end_code { get; set; }
    public string? end_code_last_updated { get; set; }
    public string? bso_organisation_code { get; set; }
    public string? bso_batch_id { get; set; }
    public string? reason_closed_code { get; set; }
    public string? end_point { get; set; }
    public string? final_action_code { get; set; }
}

enum FileType {
    Episodes = 0,
    Subjects = 1,
}
