using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.EpisodeDataService;

public class RetrieveEpisodeReferenceData
{
    private readonly ILogger<RetrieveEpisodeReferenceData> _logger;

    private readonly IEndCodeLkpRepository _endCodeLkpRepository;
    private readonly IEpisodeTypeLkpRepository _episodeTypeLkpRepository;
    private readonly IFinalActionCodeLkpRepository _finalActionCodeLkpRepository;
    private readonly IReasonClosedCodeLkpRepository _reasonClosedCodeLkpRepository;

    public RetrieveEpisodeReferenceData(
        ILogger<RetrieveEpisodeReferenceData> logger,
        IEndCodeLkpRepository endCodeLkpRepository,
        IEpisodeTypeLkpRepository episodeTypeLkpRepository,
        IFinalActionCodeLkpRepository finalActionCodeLkpRepository,
        IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository)
    {
        _logger = logger;
        _endCodeLkpRepository = endCodeLkpRepository;
        _episodeTypeLkpRepository = episodeTypeLkpRepository;
        _finalActionCodeLkpRepository = finalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
    }

    [Function("RetrieveEpisodeReferenceData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Retrieving Episode Reference Data... ");

        try
        {
            var endCodes = await _endCodeLkpRepository.GetAllEndCodesAsync();
            var episodeTypes = await _episodeTypeLkpRepository.GetAllEpisodeTypesAsync();
            var finalActionCodes = await _finalActionCodeLkpRepository.GetAllFinalActionCodesAsync();
            var reasonClosedCodes = await _reasonClosedCodeLkpRepository.GetAllReasonClosedCodesAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await JsonSerializer.SerializeAsync(response.Body, new EpisodeReferenceData
            {
                EndCodeDescriptions = endCodes.ToDictionary(ec => ec.EndCode, ec => ec.EndCodeDescription),
                EpisodeTypeDescriptions = episodeTypes.ToDictionary(et => et.EpisodeType, et => et.EpisodeDescription),
                FinalActionCodeDescriptions = finalActionCodes.ToDictionary(fac => fac.FinalActionCode, fac => fac.FinalActionCodeDescription),
                ReasonClosedCodeDescriptions = reasonClosedCodes.ToDictionary(rcc => rcc.ReasonClosedCode, rcc => rcc.ReasonClosedCodeDescription)
            });
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve data from the db.\nException: {Message}", ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
