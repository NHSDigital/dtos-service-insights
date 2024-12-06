using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.EpisodeDataService
{
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
            try
            {
                var episodeDto = await DeserializeEpisodeDto(req);
                _logger.LogInformation("PostData: {postData}", episodeDto);

                var episodeTypeId = await GetCodeId(episodeDto.EpisodeType, "Episode type", _episodeTypeLkpRepository.GetEpisodeTypeIdAsync, req);
                var endCodeId = await GetCodeId(episodeDto.EndCode, "End code", _endCodeLkpRepository.GetEndCodeIdAsync, req);
                var reasonClosedCodeId = await GetCodeId(episodeDto.ReasonClosedCode, "Reason closed code", _reasonClosedCodeLkpRepository.GetReasonClosedCodeIdAsync, req);
                var finalActionCodeId = await GetCodeId(episodeDto.FinalActionCode, "Final action code", _finalActionCodeLkpRepository.GetFinalActionCodeIdAsync, req);

                var episode = await MapEpisodeDtoToEpisode(episodeDto, episodeTypeId, endCodeId, reasonClosedCodeId, finalActionCodeId);
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

        private async Task<EpisodeDto> DeserializeEpisodeDto(HttpRequestData req)
        {

            try
            {
                using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
                {
                    var postData = await reader.ReadToEndAsync();
                    return JsonSerializer.Deserialize<EpisodeDto>(postData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not read episode data.");
                throw;
            }
        }

        private async Task<Episode> MapEpisodeDtoToEpisode(EpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId)
        {
            return new Episode
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
        }

        private async Task<long?> GetCodeId(string code, string codeName, Func<string, Task<long?>> getCodeIdMethod, HttpRequestData req)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var codeId = await getCodeIdMethod(code);
            if (codeId == null)
            {
                _logger.LogError("{codeName} '{code}' not found in lookup table.", codeName, code);
                throw new Exception($"{codeName} '{code}' not found in lookup table.");
            }
            return codeId;
        }
    }
}
