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
    private readonly IOrganisationLkpRepository _organisationLkpRepository;

    public CreateEpisode(ILogger<CreateEpisode> logger, IEpisodeRepository episodeRepository, IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IOrganisationLkpRepository organisationLkpRepository)
    {
        _logger = logger;
        _episodesRepository = episodeRepository;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _organisationLkpRepository = organisationLkpRepository;
    }

    [Function("CreateEpisode")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        EpisodeDto episodeDto;

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
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

            var episode = new Episode
            {
                EpisodeId = episodeDto.EpisodeId,
                EpisodeTypeId = _episodeTypeLkpRepository.GetEpisodeTypeId(episodeDto.EpisodeType),
                NhsNumber = episodeDto.NhsNumber,
                EpisodeOpenDate = episodeDto.EpisodeOpenDate,
                AppointmentMadeFlag = episodeDto.AppointmentMadeFlag,
                FirstOfferedAppointmentDate = episodeDto.FirstOfferedAppointmentDate,
                ActualScreeningDate = episodeDto.ActualScreeningDate,
                EarlyRecallDate = episodeDto.EarlyRecallDate,
                CallRecallStatusAuthorisedBy = episodeDto.CallRecallStatusAuthorisedBy,
                EndCodeId = _endCodeLkpRepository.GetEndCodeId(episodeDto.EndCode),
                EndCodeLastUpdated = episodeDto.EndCodeLastUpdated,
                OrganisationId = _organisationLkpRepository.GetOrganisationId(episodeDto.OrganisationCode),
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
            _logger.LogError("Failed to create episode in database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
