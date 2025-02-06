using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using Azure.Messaging.EventGrid;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.EpisodeDataService;

public class UpdateEpisode
{
    private readonly ILogger<UpdateEpisode> _logger;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IEndCodeLkpRepository _endCodeLkpRepository;
    private readonly IEpisodeTypeLkpRepository _episodeTypeLkpRepository;
    private readonly IFinalActionCodeLkpRepository _finalActionCodeLkpRepository;
    private readonly IReasonClosedCodeLkpRepository _reasonClosedCodeLkpRepository;
    private readonly EventGridPublisherClient _eventGridPublisherClient;
    private readonly IHttpRequestService _httpRequestService;
    private const long ScreeningId = 1;

    public UpdateEpisode(ILogger<UpdateEpisode> logger, IEpisodeRepository episodeRepository, IEpisodeLkpRepository episodeLkpRepository, EventGridPublisherClient eventGridPublisherClient, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _episodeRepository = episodeRepository;
        _endCodeLkpRepository = episodeLkpRepository.EndCodeLkpRepository;
        _episodeTypeLkpRepository = episodeLkpRepository.EpisodeTypeLkpRepository;
        _finalActionCodeLkpRepository = episodeLkpRepository.FinalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = episodeLkpRepository.ReasonClosedCodeLkpRepository;
        _eventGridPublisherClient = eventGridPublisherClient;
        _httpRequestService = httpRequestService;
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

            bool shouldUpdate = episodeDto.SrcSysProcessedDateTime > existingEpisode.SrcSysProcessedDatetime;

            var checkParticipantExistsUrl = $"{Environment.GetEnvironmentVariable("CheckParticipantExistsUrl")}?NhsNumber={episodeDto.NhsNumber}&ScreeningId={ScreeningId}";
            var checkParticipantExistsResult = await _httpRequestService.SendGet(checkParticipantExistsUrl);
            // If the participant does not exist then flag as an exception
            var exceptionFlag = !checkParticipantExistsResult.IsSuccessStatusCode;

            EpisodeTypeLkp? episodeTypeLkp = await GetCodeObject<EpisodeTypeLkp?>(episodeDto.EpisodeType, "Episode type", _episodeTypeLkpRepository.GetEpisodeTypeLkp);
            EndCodeLkp? endCodeLkp = await GetCodeObject<EndCodeLkp?>(episodeDto.EndCode, "End code", _endCodeLkpRepository.GetEndCodeLkp);
            ReasonClosedCodeLkp? reasonClosedCodeLkp = await GetCodeObject<ReasonClosedCodeLkp?>(episodeDto.ReasonClosedCode, "Reason closed code", _reasonClosedCodeLkpRepository.GetReasonClosedLkp);
            FinalActionCodeLkp? finalActionCodeLkp = await GetCodeObject<FinalActionCodeLkp?>(episodeDto.FinalActionCode, "Final action code", _finalActionCodeLkpRepository.GetFinalActionCodeLkp);

            existingEpisode = await MapEpisodeDtoToEpisode( episodeDto, episodeTypeLkp?.EpisodeTypeId, endCodeLkp?.EndCodeId, reasonClosedCodeLkp?.ReasonClosedCodeId, finalActionCodeLkp?.FinalActionCodeId, exceptionFlag);

            if (shouldUpdate)
            {
                await _episodeRepository.UpdateEpisode(existingEpisode);
                _logger.LogInformation("Episode {episodeId} updated successfully.", episodeDto.EpisodeId);
            }
            else
            {
                _logger.LogInformation("Incoming data is not newer. Skipping update for episode {episodeId}.", episodeDto.EpisodeId);
            }

            var finalizedEpisodeDto = (FinalizedEpisodeDto)existingEpisode;

            finalizedEpisodeDto.EpisodeType = episodeTypeLkp?.EpisodeType;
            finalizedEpisodeDto.EpisodeTypeDescription = episodeTypeLkp?.EpisodeDescription;
            finalizedEpisodeDto.EndCode = endCodeLkp?.EndCode;
            finalizedEpisodeDto.EndCodeDescription = endCodeLkp?.EndCodeDescription;
            finalizedEpisodeDto.ReasonClosedCode = reasonClosedCodeLkp?.ReasonClosedCode;
            finalizedEpisodeDto.ReasonClosedCodeDescription = reasonClosedCodeLkp?.ReasonClosedCodeDescription;
            finalizedEpisodeDto.FinalActionCode = finalActionCodeLkp?.FinalActionCode;
            finalizedEpisodeDto.FinalActionCodeDescription = finalActionCodeLkp?.FinalActionCodeDescription;

            EventGridEvent eventGridEvent = new EventGridEvent(
                subject: "Episode Updated",
                eventType: "UpdateParticipantScreeningEpisode",
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

    private async Task<Episode> MapEpisodeDtoToEpisode(InitialEpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId, bool exceptionFlag)
    {
        var organisationId = await GetOrganisationId(episodeDto.OrganisationCode);
        return new Episode
        {
            EpisodeId = episodeDto.EpisodeId,
            ScreeningId = 1, // Need to get ScreeningId from ScreeningName
            NhsNumber = episodeDto.NhsNumber,
            EpisodeTypeId = episodeTypeId,
            EpisodeOpenDate = episodeDto.EpisodeOpenDate,
            AppointmentMadeFlag = episodeDto.AppointmentMadeFlag,
            FirstOfferedAppointmentDate = episodeDto.FirstOfferedAppointmentDate,
            ActualScreeningDate = episodeDto.ActualScreeningDate,
            EarlyRecallDate = episodeDto.EarlyRecallDate,
            CallRecallStatusAuthorisedBy = episodeDto.CallRecallStatusAuthorisedBy,
            EndCodeId = endCodeId,
            EndCodeLastUpdated = episodeDto.EndCodeLastUpdated,
            ReasonClosedCodeId = reasonClosedCodeId,
            FinalActionCodeId = finalActionCodeId,
            EndPoint = episodeDto.EndPoint,
            OrganisationId = organisationId,
            BatchId = episodeDto.BatchId,
            ExceptionFlag = exceptionFlag ? (short)1 : (short)0,
            SrcSysProcessedDatetime = episodeDto.SrcSysProcessedDateTime,
            RecordInsertDatetime = DateTime.UtcNow,
            RecordUpdateDatetime = DateTime.UtcNow
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

    private async Task<long> GetOrganisationId(string organisationCode)
    {
        var getOrganisationUrl = $"{Environment.GetEnvironmentVariable("GetOrganisationIdByCodeUrl")}?organisation_code={organisationCode}";
        var getOrganisationResponse = await _httpRequestService.SendGet(getOrganisationUrl);
        if (!getOrganisationResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve Organisation ID for organisation code '{organisationCode}'", organisationCode);
            throw new Exception($"Failed to retrieve Organisation ID for organisation code '{organisationCode}'");
        }
        var getOrganisationJson = await getOrganisationResponse.Content.ReadAsStringAsync();
        var organisationLkp = JsonSerializer.Deserialize<OrganisationLkp>(getOrganisationJson);
        return organisationLkp.OrganisationId;
    }
}
