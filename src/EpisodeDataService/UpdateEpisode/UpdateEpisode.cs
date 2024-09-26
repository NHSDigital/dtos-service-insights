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

    public UpdateEpisode(ILogger<UpdateEpisode> logger, IEpisodeRepository episodeRepository)
    {
        _logger = logger;
        _episodeRepository = episodeRepository;
    }

    [Function("UpdateEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put")] HttpRequestData req)
    {
        Episode episode;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
                episode = JsonSerializer.Deserialize<Episode>(postData);
                _logger.LogInformation("Request to update episode {episodeId} received.", episode.EpisodeId);
                _logger.LogInformation("PostData: {postData}", postData);
            }
        }
        catch
        {
            _logger.LogError("Could not read episode data.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            var existingEpisode = await _episodeRepository.GetEpisodeAsync(episode.EpisodeId);
            if (existingEpisode != null)
            {
                existingEpisode.ParticipantId = episode.ParticipantId;
                existingEpisode.ScreeningId = episode.ScreeningId;
                existingEpisode.NhsNumber = episode.NhsNumber;
                existingEpisode.EpisodeTypeId = episode.EpisodeTypeId;
                existingEpisode.EpisodeOpenDate = episode.EpisodeOpenDate;
                existingEpisode.AppointmentMadeFlag = episode.AppointmentMadeFlag;
                existingEpisode.FirstOfferedAppointmentDate = episode.FirstOfferedAppointmentDate;
                existingEpisode.ActualScreeningDate = episode.ActualScreeningDate;
                existingEpisode.EarlyRecallDate = episode.EarlyRecallDate;
                existingEpisode.CallRecallStatusAuthorisedBy = episode.CallRecallStatusAuthorisedBy;
                existingEpisode.EndCodeId = episode.EndCodeId;
                existingEpisode.EndCodeLastUpdated = episode.EndCodeLastUpdated;
                existingEpisode.OrganisationId = episode.OrganisationId;
                existingEpisode.BatchId = episode.BatchId;
                existingEpisode.RecordInsertDatetime = episode.RecordInsertDatetime;
                existingEpisode.RecordUpdateDatetime = episode.RecordUpdateDatetime;

                try
                {
                    _episodeRepository.UpdateEpisode(existingEpisode);
                    _logger.LogInformation("Episode {episodeId} updated successfully.", episode.EpisodeId);
                    return req.CreateResponse(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating episode {episodeId}.", episode.EpisodeId);
                    return req.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                _logger.LogInformation("Episode {episodeId} not found.", episode.EpisodeId);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating episode {episodeId}.", episode.EpisodeId);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
