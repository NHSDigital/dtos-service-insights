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
    private readonly IOrganisationLkpRepository _organisationLkpRepository;
   // private readonly GetReference
 // private readonly ReferenceDataService _referenceDataService;

    public CreateEpisode(ILogger<CreateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IFinalActionCodeLkpRepository finalActionCodeLkpRepository, IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository,  IOrganisationLkpRepository organisationLkpRepository, EventGridPublisherClient eventGridPublisherClient)
    {
        _logger = logger;
        _episodesRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _finalActionCodeLkpRepository = finalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
        _organisationLkpRepository = organisationLkpRepository;
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

           // OrganisationLkp? organisationLkp = await GetCodeObject<OrganisationLkp?>(episodeDto.OrganisationCode, "Organisation code", code => _referenceDataService.GetOrganisationIdByCode(code) );

            EndCodeLkp? endCodeLkp = await GetCodeObject<EndCodeLkp?>(episodeDto.EndCode, "End code", _endCodeLkpRepository.GetEndCodeLkp);
            ReasonClosedCodeLkp? reasonClosedCodeLkp = await GetCodeObject<ReasonClosedCodeLkp?>(episodeDto.ReasonClosedCode, "Reason closed code", _reasonClosedCodeLkpRepository.GetReasonClosedLkp);
            FinalActionCodeLkp? finalActionCodeLkp = await GetCodeObject<FinalActionCodeLkp?>(episodeDto.FinalActionCode, "Final action code", _finalActionCodeLkpRepository.GetFinalActionCodeLkp);

          //  OrganisationReferenceData? organisationReferenceData = await GetCodeObject<OrganisationReferenceData?>(episodeDto.OrganisationCode, "Organisation code", _organisationLkpRepository.GetOrganisationAsync);

            var episode = await MapEpisodeDtoToEpisode(episodeDto, episodeTypeLkp?.EpisodeTypeId, endCodeLkp?.EndCodeId, reasonClosedCodeLkp?.ReasonClosedCodeId, finalActionCodeLkp?.FinalActionCodeId, ;

            _logger.LogInformation("Calling CreateEpisode method...");
            _episodesRepository.CreateEpisode(episode);
            _logger.LogInformation("Episode created successfully.");

            // Prepare finalized episode DTO
            var finalizedEpisodeDto = MapToFinalizedEpisodeDto(episode, episodeTypeLkp, endCodeLkp, reasonClosedCodeLkp, finalActionCodeLkp);

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

    //private async Task<Episode> MapEpisodeDtoToEpisode(InitialEpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId, long? organisationID)
    private async Task<Episode> MapEpisodeDtoToEpisode(InitialEpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId, OrganisationReferenceData organisationReferenceData)
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
            OrganisationId=string.IsNullOrEmpty(episodeDto.OrganisationCode) ? null : organisationReferenceData.OrganisationCodeToIdLookup[episodeDto.OrganisationCode],

            BatchId = episodeDto.BatchId,
            RecordInsertDatetime = DateTime.UtcNow,
            RecordUpdateDatetime = DateTime.UtcNow
        };
    }

    private FinalizedEpisodeDto MapToFinalizedEpisodeDto(Episode episode, EpisodeTypeLkp? episodeTypeLkp, EndCodeLkp? endCodeLkp, ReasonClosedCodeLkp? reasonClosedCodeLkp, FinalActionCodeLkp? finalActionCodeLkp, OrganisationLkp? organisationLkp)
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
            OrganisationId = organisationLkp?.OrganisationId
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

