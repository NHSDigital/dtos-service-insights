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
        try
        {
            var episodeDto = await DeserializeEpisodeDto(req);
            _logger.LogInformation("Request to update episode {episodeId} received.", episodeDto.EpisodeId);

            var existingEpisode = await _episodeRepository.GetEpisodeAsync(episodeDto.EpisodeId);
            if (existingEpisode == null)
            {
                _logger.LogError("Episode {episodeId} not found.", episodeDto.EpisodeId);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var episodeTypeId = await GetCodeId(episodeDto.EpisodeType, "Episode type", _episodeTypeLkpRepository.GetEpisodeTypeIdAsync);
            var endCodeId = await GetCodeId(episodeDto.EndCode, "End code", _endCodeLkpRepository.GetEndCodeIdAsync);
            var reasonClosedCodeId = await GetCodeId(episodeDto.ReasonClosedCode, "Reason closed code", _reasonClosedCodeLkpRepository.GetReasonClosedCodeIdAsync);
            var finalActionCodeId = await GetCodeId(episodeDto.FinalActionCode, "Final action code", _finalActionCodeLkpRepository.GetFinalActionCodeIdAsync);

            existingEpisode = await MapEpisodeDtoToEpisode(existingEpisode, episodeDto, episodeTypeId, endCodeId, reasonClosedCodeId, finalActionCodeId);

            await _episodeRepository.UpdateEpisode(existingEpisode);
            _logger.LogInformation("Episode {episodeId} updated successfully.", episodeDto.EpisodeId);
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating episode.");
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
            var errorMessage = $"Could not read episode data.: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new InvalidOperationException(errorMessage, ex);
        }
    }

    private async static Task<Episode> MapEpisodeDtoToEpisode(Episode existingEpisode, EpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId)
    {
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
        return existingEpisode;
    }

    private async Task<long?> GetCodeId(string code, string codeName, Func<string, Task<long?>> getCodeIdMethod)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var codeId = await getCodeIdMethod(code);
        if (codeId == null)
        {
            _logger.LogError("{codeName} '{code}' not found in lookup table.", codeName, code);
            throw new InvalidOperationException($"{codeName} '{code}' not found in lookup table.");
        }
        return codeId;
    }
}
