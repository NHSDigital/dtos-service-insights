using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.EpisodeDataService;

public class CreateEpisode
{
    private readonly ILogger<CreateEpisode> _logger;
    private readonly IEpisodeRepository _episodesRepository;
    private readonly IEndCodeLkpRepository _endCodeLkpRepository;
    private readonly IEpisodeTypeLkpRepository _episodeTypeLkpRepository;
    private readonly IFinalActionCodeLkpRepository _finalActionCodeLkpRepository;
    private readonly IReasonClosedCodeLkpRepository _reasonClosedCodeLkpRepository;

    public CreateEpisode(ILogger<CreateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IFinalActionCodeLkpRepository finalActionCodeLkpRepository, IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository)
    {
        _logger = logger;
        _episodesRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _finalActionCodeLkpRepository = finalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
    }

    [Function("CreateEpisode")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
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
        catch
        {
            _logger.LogError("Could not read episode data.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {

            var episodeTypeId = await _episodeTypeLkpRepository.GetEpisodeTypeIdAsync(episodeDto.EpisodeType);
            var endCodeId = await _endCodeLkpRepository.GetEndCodeIdAsync(episodeDto.EndCode);
            var reasonClosedCodeId = await _reasonClosedCodeLkpRepository.GetReasonClosedCodeIdAsync(episodeDto.ReasonClosedCode);
            var finalActionCodeId = await _finalActionCodeLkpRepository.GetFinalActionCodeIdAsync(episodeDto.FinalActionCode);

            var episode = new Episode
            {
                EpisodeId = episodeDto.EpisodeId,
                EpisodeIdSystem = null,
                ScreeningId = episodeDto.ScreeningId,
                NhsNumber = episodeDto.NhsNumber,
                EpisodeTypeId = episodeTypeId,
                EpisodeOpenDate = episodeDto.EpisodeOpenDate,
                FirstOfferedAppointmentDate = episodeDto.FirstOfferedAppointmentDate,
                ActualScreeningDate = episodeDto.ActualScreeningDate,
                EarlyRecallDate = episodeDto.EarlyRecallDate,
                CallRecallStatusAuthorisedBy = episodeDto.CallRecallStatusAuthorisedBy,
                EndCodeId = endCodeId,
                EndCodeLastUpdated = episodeDto.EndCodeLastUpdated,
                ReasonClosedCodeId = reasonClosedCodeId,
                FinalActionCodeId = finalActionCodeId,
                EndPoint = null,
                OrganisationId = null,
                BatchId = episodeDto.BatchId,
                RecordInsertDatetime = DateTime.UtcNow,
                RecordUpdateDatetime = DateTime.UtcNow
            };

            short? appointmentMadeFlagValue = episodeDto.AppointmentMadeFlag?.ToUpper() == "TRUE" ? 1 : (episodeDto.AppointmentMadeFlag != null ? 0 : null);
            episode.AppointmentMadeFlag = appointmentMadeFlagValue;

            _logger.LogInformation("Calling CreateEpisode method...");
            _episodesRepository.CreateEpisode(episode);
            _logger.LogInformation("Episode created successfully.");
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create episode in database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
