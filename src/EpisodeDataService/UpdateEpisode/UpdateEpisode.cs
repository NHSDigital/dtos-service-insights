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

    public UpdateEpisode(ILogger<UpdateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IFinalActionCodeLkpRepository finalActionCodeLkpRepository, IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository, EventGridPublisherClient eventGridPublisherClient, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _episodeRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _finalActionCodeLkpRepository = finalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
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

            var checkParticipantExistsUrl = $"{Environment.GetEnvironmentVariable("CheckParticipantExistsUrl")}?NhsNumber={episodeDto.NhsNumber}&ScreeningId={ScreeningId}";
            var checkParticipantExistsResult = await _httpRequestService.SendGet(checkParticipantExistsUrl);
            // If the participant does not exist then flag as an exception
            var exceptionFlag = !checkParticipantExistsResult.IsSuccessStatusCode;

            EpisodeTypeLkp? episodeTypeLkp = await GetCodeObject<EpisodeTypeLkp?>(episodeDto.EpisodeType, "Episode type", _episodeTypeLkpRepository.GetEpisodeTypeLkp);
            EndCodeLkp? endCodeLkp = await GetCodeObject<EndCodeLkp?>(episodeDto.EndCode, "End code", _endCodeLkpRepository.GetEndCodeLkp);
            ReasonClosedCodeLkp? reasonClosedCodeLkp = await GetCodeObject<ReasonClosedCodeLkp?>(episodeDto.ReasonClosedCode, "Reason closed code", _reasonClosedCodeLkpRepository.GetReasonClosedLkp);
            FinalActionCodeLkp? finalActionCodeLkp = await GetCodeObject<FinalActionCodeLkp?>(episodeDto.FinalActionCode, "Final action code", _finalActionCodeLkpRepository.GetFinalActionCodeLkp);

            existingEpisode = await MapEpisodeDtoToEpisode(existingEpisode, episodeDto, episodeTypeLkp?.EpisodeTypeId, endCodeLkp?.EndCodeId, reasonClosedCodeLkp?.ReasonClosedCodeId, finalActionCodeLkp?.FinalActionCodeId, exceptionFlag);

            await _episodeRepository.UpdateEpisode(existingEpisode);
            _logger.LogInformation("Episode {episodeId} updated successfully.", episodeDto.EpisodeId);

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
                eventType: "CreateParticipantScreeningEpisode",
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

    private async static Task<Episode> MapEpisodeDtoToEpisode(Episode existingEpisode, InitialEpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId, bool exceptionFlag)
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
        existingEpisode.OrganisationId = 111111; // Need to get OrganisationId from Reference Management Data Store
        existingEpisode.BatchId = episodeDto.BatchId;
        existingEpisode.ExceptionFlag = exceptionFlag ? (short)1 : (short)0;
        existingEpisode.RecordUpdateDatetime = DateTime.UtcNow;
        return existingEpisode;
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
}
