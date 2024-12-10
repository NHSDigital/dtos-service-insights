using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
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
        EpisodeDto episodeDto;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = await reader.ReadToEndAsync();
                episodeDto = JsonSerializer.Deserialize<EpisodeDto>(postData);
                _logger.LogInformation("PostData: {postData}", postData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not read episode data.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {

            var episodeTypeId = !string.IsNullOrWhiteSpace(episodeDto.EpisodeType) ?
                await _episodeTypeLkpRepository.GetEpisodeTypeIdAsync(episodeDto.EpisodeType) : null;

            if (episodeTypeId == null && !string.IsNullOrWhiteSpace(episodeDto.EpisodeType))
            {
                _logger.LogError("Episode type '{episodeType}' not found in lookup table.", episodeDto.EpisodeType);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var endCodeId = !string.IsNullOrWhiteSpace(episodeDto.EndCode) ?
                await _endCodeLkpRepository.GetEndCodeIdAsync(episodeDto.EndCode) : null;

            if (endCodeId == null && !string.IsNullOrWhiteSpace(episodeDto.EndCode))
            {
                _logger.LogError("End code '{endCode}' not found in lookup table.", episodeDto.EndCode);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var reasonClosedCodeId = !string.IsNullOrWhiteSpace(episodeDto.ReasonClosedCode) ?
                await _reasonClosedCodeLkpRepository.GetReasonClosedCodeIdAsync(episodeDto.ReasonClosedCode) : null;

            if (reasonClosedCodeId == null && !string.IsNullOrWhiteSpace(episodeDto.ReasonClosedCode))
            {
                _logger.LogError("Reason closed code '{reasonClosedCode}' not found in lookup table.", episodeDto.ReasonClosedCode);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var finalActionCodeId = !string.IsNullOrWhiteSpace(episodeDto.FinalActionCode) ?
                await _finalActionCodeLkpRepository.GetFinalActionCodeIdAsync(episodeDto.FinalActionCode) : null;

            if (finalActionCodeId == null && !string.IsNullOrWhiteSpace(episodeDto.FinalActionCode))
            {
                _logger.LogError("Final action code '{finalActionCode}' not found in lookup table.", episodeDto.FinalActionCode);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }


            var episode = new Episode
            {
                EpisodeId = episodeDto.EpisodeId,
                EpisodeIdSystem = null,
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

            _logger.LogInformation("Calling CreateEpisode method...");
            _episodesRepository.CreateEpisode(episode);
            _logger.LogInformation("Episode created successfully.");

            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            string json = JsonSerializer.Serialize(episode, options);
            BinaryData binaryData = new BinaryData(json);

            EventGridEvent eventGridEvent = new EventGridEvent(
                subject: "Episode Created",
                eventType: "CreateParticipantScreeningEpisode",
                dataVersion: "1.0",
                data: binaryData
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
}
