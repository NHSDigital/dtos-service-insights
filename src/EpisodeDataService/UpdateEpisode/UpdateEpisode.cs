using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Common;
using Azure.Messaging.EventGrid;
using System.Text.Json.Serialization;

namespace NHS.ServiceInsights.EpisodeDataService;

public class UpdateEpisode
{
    private readonly ILogger<UpdateEpisode> _logger;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IEndCodeLkpRepository _endCodeLkpRepository;
    private readonly IEpisodeTypeLkpRepository _episodeTypeLkpRepository;
    private readonly IFinalActionCodeLkpRepository _finalActionCodeLkpRepository;
    private readonly IReasonClosedCodeLkpRepository _reasonClosedCodeLkpRepository;
    private readonly IOrganisationLkpRepository _organisationLkpRepository;
    private readonly EventGridPublisherClient _eventGridPublisherClient;
     private readonly IHttpRequestService _httpRequestService;

    public UpdateEpisode(ILogger<UpdateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IFinalActionCodeLkpRepository finalActionCodeLkpRepository, IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository, IOrganisationLkpRepository organisationLkpRepository ,EventGridPublisherClient eventGridPublisherClient)
    {
        _logger = logger;
        _episodeRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _finalActionCodeLkpRepository = finalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
        _organisationLkpRepository= organisationLkpRepository;
        _eventGridPublisherClient = eventGridPublisherClient;
    }

    [Function("UpdateEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put")] HttpRequestData req)
    {

        InitialEpisodeDto episodeDto;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = await reader.ReadToEndAsync();
                episodeDto = JsonSerializer.Deserialize<InitialEpisodeDto>(postData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not read episode data");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            _logger.LogInformation("Request to update episode {episodeId} received.", episodeDto.EpisodeId);

            var existingEpisode = await _episodeRepository.GetEpisodeAsync(episodeDto.EpisodeId);
            if (existingEpisode == null)
            {
                _logger.LogError("Episode {episodeId} not found.", episodeDto.EpisodeId);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            EpisodeTypeLkp? episodeTypeLkp = await GetCodeObject<EpisodeTypeLkp?>(episodeDto.EpisodeType, "Episode type", _episodeTypeLkpRepository.GetEpisodeTypeLkp);
            EndCodeLkp? endCodeLkp = await GetCodeObject<EndCodeLkp?>(episodeDto.EndCode, "End code", _endCodeLkpRepository.GetEndCodeLkp);
            ReasonClosedCodeLkp? reasonClosedCodeLkp = await GetCodeObject<ReasonClosedCodeLkp?>(episodeDto.ReasonClosedCode, "Reason closed code", _reasonClosedCodeLkpRepository.GetReasonClosedLkp);
            FinalActionCodeLkp? finalActionCodeLkp = await GetCodeObject<FinalActionCodeLkp?>(episodeDto.FinalActionCode, "Final action code", _finalActionCodeLkpRepository.GetFinalActionCodeLkp);

            var organisationId = await GetOrganisationIdByCodeAsync(episodeDto.OrganisationCode);

            existingEpisode = await MapEpisodeDtoToEpisode(existingEpisode, episodeDto, episodeTypeLkp?.EpisodeTypeId, endCodeLkp?.EndCodeId, reasonClosedCodeLkp?.ReasonClosedCodeId, finalActionCodeLkp?.FinalActionCodeId, organisationId);

            bool shouldUpdate = episodeDto.SrcSysProcessedDateTime > existingEpisode.SrcSysProcessedDatetime;

            if (shouldUpdate)
            {
                await _episodeRepository.UpdateEpisode(existingEpisode);
                _logger.LogInformation("Episode {episodeId} updated successfully.", episodeDto.EpisodeId);
            }

            else
            {
                _logger.LogInformation("Incoming data is not newer. Skipping update for episode {episodeId}.", episodeDto.EpisodeId);
            }

            // Prepare finalized episode DTO
            var finalizedEpisodeDto = MapToFinalizedEpisodeDto(existingEpisode, episodeTypeLkp, endCodeLkp, reasonClosedCodeLkp, finalActionCodeLkp,organisationId);

            EventGridEvent eventGridEvent = new EventGridEvent(
                subject: "EpisodeUpdate",
                eventType: "NSP.EpisodeUpdateReceived",
                dataVersion: "1.0",
                data: finalizedEpisodeDto
            );

            var result = await _eventGridPublisherClient.SendEventAsync(eventGridEvent);

            if (result.Status != (int)HttpStatusCode.OK)
            {
                _logger.LogError("Failed to send event to event grid");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating episode.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    private async static Task<Episode> MapEpisodeDtoToEpisode(Episode existingEpisode, InitialEpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId, long? organisationId)
    {
        existingEpisode.ScreeningId = 1; // Need to get ScreeningId from ScreeningName
        existingEpisode.NhsNumber = episodeDto.NhsNumber;
        existingEpisode.EpisodeTypeId = episodeTypeId;
        existingEpisode.EpisodeOpenDate = episodeDto.EpisodeOpenDate;
        existingEpisode.AppointmentMadeFlag = episodeDto.AppointmentMadeFlag;
        existingEpisode.FirstOfferedAppointmentDate = episodeDto.FirstOfferedAppointmentDate;
        existingEpisode.ActualScreeningDate = episodeDto.ActualScreeningDate;
        existingEpisode.EarlyRecallDate = episodeDto.EarlyRecallDate;
        existingEpisode.CallRecallStatusAuthorisedBy = episodeDto.CallRecallStatusAuthorisedBy;
        existingEpisode.EndCodeId = endCodeId;
        existingEpisode.EndCodeLastUpdated = episodeDto.EndCodeLastUpdated;
        existingEpisode.ReasonClosedCodeId = reasonClosedCodeId;
        existingEpisode.FinalActionCodeId = finalActionCodeId;
        existingEpisode.EndPoint = episodeDto.EndPoint;
        existingEpisode.OrganisationId=organisationId;
        existingEpisode.BatchId = episodeDto.BatchId;
        existingEpisode.RecordUpdateDatetime = DateTime.UtcNow;
        return existingEpisode;
    }

    private FinalizedEpisodeDto MapToFinalizedEpisodeDto(Episode episode, EpisodeTypeLkp? episodeTypeLkp, EndCodeLkp? endCodeLkp, ReasonClosedCodeLkp? reasonClosedCodeLkp, FinalActionCodeLkp? finalActionCodeLkp, long? organisationId)
    {
        return new FinalizedEpisodeDto
        {
            EpisodeId = episode.EpisodeId,
            EpisodeType = episodeTypeLkp?.EpisodeType,
            EpisodeTypeDescription = episodeTypeLkp?.EpisodeDescription,
            EndCode = endCodeLkp?.EndCode,
            EndCodeDescription = endCodeLkp?.EndCodeDescription,
            ReasonClosedCode = reasonClosedCodeLkp?.ReasonClosedCode,
            ReasonClosedCodeDescription = reasonClosedCodeLkp?.ReasonClosedCodeDescription,
            FinalActionCode = finalActionCodeLkp?.FinalActionCode,
            FinalActionCodeDescription = finalActionCodeLkp?.FinalActionCodeDescription,
            OrganisationId = organisationId
        };
    }

    private async Task<T?> GetCodeObject<T>(string code, string codeName, Func<string, Task<T?>> getObjectMethod) where T : class?
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var codeObject = await getObjectMethod(code);
        if (codeObject == null)
        {
            _logger.LogError("{codeName} '{code}' not found in lookup table.", codeName, code);
            throw new InvalidOperationException($"{codeName} '{code}' not found in lookup table.");
        }
        return codeObject;
    }

    private async Task<long?> GetOrganisationIdByCodeAsync(string organisationCode)
    {
        var url = $"{Environment.GetEnvironmentVariable("GetOrganisationIdByCodeUrl")}?organisation_code={organisationCode}";
        var response = await _httpRequestService.SendGet(url);

        response.EnsureSuccessStatusCode();

        var organisationId = await response.Content.ReadAsStringAsync();
        return long.Parse(organisationId);
    }

}

