using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using Azure.Messaging.EventGrid;

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


    public UpdateEpisode(ILogger<UpdateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IFinalActionCodeLkpRepository finalActionCodeLkpRepository, IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository, EventGridPublisherClient eventGridPublisherClient)
    {
        _logger = logger;
        _episodeRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _finalActionCodeLkpRepository = finalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
        _eventGridPublisherClient = eventGridPublisherClient;
    }

    [Function("UpdateEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put")] HttpRequestData req)
    {
        EpisodeDto episodeDto;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = await reader.ReadToEndAsync();
                episodeDto = JsonSerializer.Deserialize<EpisodeDto>(postData);
                _logger.LogInformation("Request to update episode {episodeId} received.", episodeDto.EpisodeId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not read episode data.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            var existingEpisode = await _episodeRepository.GetEpisodeAsync(episodeDto.EpisodeId);

            if (existingEpisode != null)
            {
                var episodeTypeId = await _episodeTypeLkpRepository.GetEpisodeTypeIdAsync(episodeDto.EpisodeType);
                var endCodeId = await _endCodeLkpRepository.GetEndCodeIdAsync(episodeDto.EndCode);
                var reasonClosedCodeId = await _reasonClosedCodeLkpRepository.GetReasonClosedCodeIdAsync(episodeDto.ReasonClosedCode);
                var finalActionCodeId = await _finalActionCodeLkpRepository.GetFinalActionCodeIdAsync(episodeDto.FinalActionCode);

                existingEpisode.EpisodeIdSystem = null;
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
                existingEpisode.RecordUpdateDatetime = DateTime.UtcNow;

                try
                {
                    await _episodeRepository.UpdateEpisode(existingEpisode);
                    _logger.LogInformation("Episode {episodeId} updated successfully.", episodeDto.EpisodeId);

                    EventGridEvent eventGridEvent = new EventGridEvent(
                        subject: "Episode Updated",
                        eventType: "CreateParticipantScreeningEpisode",
                        dataVersion: "1.0",
                        data: existingEpisode
                    );

                    await _eventGridPublisherClient.SendEventAsync(eventGridEvent);

                    return req.CreateResponse(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating episode {episodeId}.", episodeDto.EpisodeId);
                    return req.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                _logger.LogError("Episode {episodeId} not found.", episodeDto.EpisodeId);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating episode {episodeId}.", episodeDto.EpisodeId);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
