using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;

namespace NHS.ServiceInsights.EpisodeDataService;

public class UpdateEpisode
{
    private readonly ILogger<UpdateEpisode> _logger;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IEndCodeLkpRepository _endCodeLkpRepository;
    private readonly IEpisodeTypeLkpRepository _episodeTypeLkpRepository;
    private readonly IFinalActionCodeLkpRepository _finalActionCodeLkpRepository;
    private readonly IReasonClosedCodeLkpRepository _reasonClosedCodeLkpRepository;

    public UpdateEpisode(ILogger<UpdateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IFinalActionCodeLkpRepository finalActionCodeLkpRepository, IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository)
    {
        _logger = logger;
        _episodeRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _finalActionCodeLkpRepository = finalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
    }

    [Function("UpdateEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put")] HttpRequestData req)
    {
        EpisodeDto episodeDto;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
                episodeDto = JsonSerializer.Deserialize<EpisodeDto>(postData);
                _logger.LogInformation("Request to update episode {episodeId} received.", episodeDto.EpisodeId);
            }
        }
        catch
        {
            _logger.LogError("Could not read episode data.");
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
                existingEpisode.ScreeningId = episodeDto.ScreeningId;
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
                existingEpisode.EndPoint = null;
                existingEpisode.OrganisationId = null;
                existingEpisode.BatchId = episodeDto.BatchId;
                existingEpisode.RecordUpdateDatetime = DateTime.UtcNow;

                try
                {
                    await _episodeRepository.UpdateEpisode(existingEpisode);
                    _logger.LogInformation("Episode {episodeId} updated successfully.", episodeDto.EpisodeId);
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
