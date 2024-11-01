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
    private readonly IOrganisationLkpRepository _organisationLkpRepository;

    public UpdateEpisode(ILogger<UpdateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IOrganisationLkpRepository organisationLkpRepository)
    {
        _logger = logger;
        _episodeRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _organisationLkpRepository = organisationLkpRepository;
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
                existingEpisode.ScreeningId = episodeDto.ScreeningId;
                existingEpisode.NhsNumber = episodeDto.NhsNumber;
                existingEpisode.EpisodeTypeId = _episodeTypeLkpRepository.GetEpisodeTypeId(episodeDto.EpisodeType);
                existingEpisode.EpisodeOpenDate = episodeDto.EpisodeOpenDate;
                existingEpisode.AppointmentMadeFlag = episodeDto.AppointmentMadeFlag;
                existingEpisode.FirstOfferedAppointmentDate = episodeDto.FirstOfferedAppointmentDate;
                existingEpisode.ActualScreeningDate = episodeDto.ActualScreeningDate;
                existingEpisode.EarlyRecallDate = episodeDto.EarlyRecallDate;
                existingEpisode.CallRecallStatusAuthorisedBy = episodeDto.CallRecallStatusAuthorisedBy;
                existingEpisode.EndCodeId = _endCodeLkpRepository.GetEndCodeId(episodeDto.EndCode);
                existingEpisode.EndCodeLastUpdated = episodeDto.EndCodeLastUpdated;
                existingEpisode.OrganisationId = _organisationLkpRepository.GetOrganisationId(episodeDto.OrganisationCode);
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
