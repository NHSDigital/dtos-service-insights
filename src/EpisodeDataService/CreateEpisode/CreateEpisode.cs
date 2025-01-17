using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;
using Azure.Messaging.EventGrid;
using System.Text.Json.Serialization;

namespace NHS.ServiceInsights.EpisodeDataService;

public class CreateEpisode
{
    private readonly ILogger<CreateEpisode> _logger;
    private readonly IEpisodeRepository _episodesRepository;
    private readonly IEndCodeLkpRepository _endCodeLkpRepository;
    private readonly IEpisodeTypeLkpRepository _episodeTypeLkpRepository;
    private readonly IFinalActionCodeLkpRepository _finalActionCodeLkpRepository;
    private readonly IReasonClosedCodeLkpRepository _reasonClosedCodeLkpRepository;
    private readonly EventGridPublisherClient _eventGridPublisherClient;

    public CreateEpisode(ILogger<CreateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IFinalActionCodeLkpRepository finalActionCodeLkpRepository, IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository, EventGridPublisherClient eventGridPublisherClient)
    {
        _logger = logger;
        _episodesRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _finalActionCodeLkpRepository = finalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
        _eventGridPublisherClient = eventGridPublisherClient;
    }

    [Function("CreateEpisode")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
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
            _logger.LogError(ex, "Episode could not be read");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            EpisodeTypeLkp? episodeTypeLkp = await GetCodeObject<EpisodeTypeLkp?>(episodeDto.EpisodeType, "Episode type", _episodeTypeLkpRepository.GetEpisodeTypeLkp);
            EndCodeLkp? endCodeLkp = await GetCodeObject<EndCodeLkp?>(episodeDto.EndCode, "End code", _endCodeLkpRepository.GetEndCodeLkp);
            ReasonClosedCodeLkp? reasonClosedCodeLkp = await GetCodeObject<ReasonClosedCodeLkp?>(episodeDto.ReasonClosedCode, "Reason closed code", _reasonClosedCodeLkpRepository.GetReasonClosedLkp);
            FinalActionCodeLkp? finalActionCodeLkp = await GetCodeObject<FinalActionCodeLkp?>(episodeDto.FinalActionCode, "Final action code", _finalActionCodeLkpRepository.GetFinalActionCodeLkp);

            var episode = await MapEpisodeDtoToEpisode(episodeDto, episodeTypeLkp?.EpisodeTypeId, endCodeLkp?.EndCodeId, reasonClosedCodeLkp?.ReasonClosedCodeId, finalActionCodeLkp?.FinalActionCodeId);
            _logger.LogInformation("Calling CreateEpisode method...");
            _episodesRepository.CreateEpisode(episode);
            _logger.LogInformation("Episode created successfully.");

            var finalizedEpisodeDto = (FinalizedEpisodeDto)episode;

            finalizedEpisodeDto.EpisodeType = episodeTypeLkp?.EpisodeType;
            finalizedEpisodeDto.EpisodeTypeDescription = episodeTypeLkp?.EpisodeDescription;
            finalizedEpisodeDto.EndCode = endCodeLkp?.EndCode;
            finalizedEpisodeDto.EndCodeDescription = endCodeLkp?.EndCodeDescription;
            finalizedEpisodeDto.ReasonClosedCode = reasonClosedCodeLkp?.ReasonClosedCode;
            finalizedEpisodeDto.ReasonClosedCodeDescription = reasonClosedCodeLkp?.ReasonClosedCodeDescription;
            finalizedEpisodeDto.FinalActionCode = finalActionCodeLkp?.FinalActionCode;
            finalizedEpisodeDto.FinalActionCodeDescription = finalActionCodeLkp?.FinalActionCodeDescription;

            EventGridEvent eventGridEvent = new EventGridEvent(
                subject: "Episode Created",
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
            _logger.LogError(ex, "Failed to create episode in database.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    private async Task<Episode> MapEpisodeDtoToEpisode(InitialEpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId)
    {
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
            OrganisationId = 111111, // Need to get OrganisationId from Reference Management Data Store
            BatchId = episodeDto.BatchId,
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
}

