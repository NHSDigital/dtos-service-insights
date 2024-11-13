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
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create episode in database.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
