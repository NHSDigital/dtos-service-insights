using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Common;
using Azure.Messaging.EventGrid;

namespace NHS.ServiceInsights.EpisodeDataService;
public class CreateEpisode
{
    private readonly ILogger<CreateEpisode> _logger;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IEndCodeLkpRepository _endCodeLkpRepository;
    private readonly IEpisodeTypeLkpRepository _episodeTypeLkpRepository;
    private readonly IFinalActionCodeLkpRepository _finalActionCodeLkpRepository;
    private readonly IReasonClosedCodeLkpRepository _reasonClosedCodeLkpRepository;
    private readonly Func<string, IEventGridPublisherClient> _eventGridPublisherClientFactory;
    private readonly IHttpRequestService _httpRequestService;
    private const long ScreeningId = 1;

    public CreateEpisode(ILogger<CreateEpisode> logger, IEpisodeRepository episodeRepository, IEpisodeLkpRepository episodeLkpRepository, Func<string, IEventGridPublisherClient> eventGridPublisherClientFactory, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _episodeRepository = episodeRepository;
        _endCodeLkpRepository = episodeLkpRepository.EndCodeLkpRepository;
        _episodeTypeLkpRepository = episodeLkpRepository.EpisodeTypeLkpRepository;
        _finalActionCodeLkpRepository = episodeLkpRepository.FinalActionCodeLkpRepository;
        _reasonClosedCodeLkpRepository = episodeLkpRepository.ReasonClosedCodeLkpRepository;
        _eventGridPublisherClientFactory = eventGridPublisherClientFactory;
        _httpRequestService = httpRequestService;
    }

    [Function("CreateEpisode")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        InitialEpisodeDto episodeDto;

        try
        {
            // Read and deserialize the request body
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = await reader.ReadToEndAsync();
                episodeDto = JsonSerializer.Deserialize<InitialEpisodeDto>(postData);
                // Log the payload received
                _logger.LogInformation("Received payload: {Payload}", postData);
            }
        }
        catch (Exception ex)
        {
            // Log error and return BadRequest if deserialization fails
            _logger.LogError(ex, "Could not read episode data");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            // Check if the participant exists
            var checkParticipantExistsUrl = $"{Environment.GetEnvironmentVariable("CheckParticipantExistsUrl")}?NhsNumber={episodeDto.NhsNumber}&ScreeningId={ScreeningId}";
            var checkParticipantExistsResult = await _httpRequestService.SendGet(checkParticipantExistsUrl);
            // If the participant does not exist then flag as an exception
            var exceptionFlag = !checkParticipantExistsResult.IsSuccessStatusCode;

            // Retrieve lookup data
            EpisodeTypeLkp? episodeTypeLkp = await GetCodeObject<EpisodeTypeLkp?>(episodeDto.EpisodeType, "Episode type", _episodeTypeLkpRepository.GetEpisodeTypeLkp);
            EndCodeLkp? endCodeLkp = await GetCodeObject<EndCodeLkp?>(episodeDto.EndCode, "End code", _endCodeLkpRepository.GetEndCodeLkp);
            ReasonClosedCodeLkp? reasonClosedCodeLkp = await GetCodeObject<ReasonClosedCodeLkp?>(episodeDto.ReasonClosedCode, "Reason closed code", _reasonClosedCodeLkpRepository.GetReasonClosedLkp);
            FinalActionCodeLkp? finalActionCodeLkp = await GetCodeObject<FinalActionCodeLkp?>(episodeDto.FinalActionCode, "Final action code", _finalActionCodeLkpRepository.GetFinalActionCodeLkp);

            // Map DTO to Episode
            var episode = await MapEpisodeDtoToEpisode(episodeDto, episodeTypeLkp?.EpisodeTypeId, endCodeLkp?.EndCodeId, reasonClosedCodeLkp?.ReasonClosedCodeId, finalActionCodeLkp?.FinalActionCodeId, exceptionFlag);

            try
            {
                // Write to database
                _logger.LogInformation("Calling CreateEpisode method...");
                _episodeRepository.CreateEpisode(episode);
                _logger.LogInformation("Episode {episodeId} created successfully in database.", episodeDto.EpisodeId);

            }
            catch (Exception ex)
            {
                // Log error and return InternalServerError if database write fails
                _logger.LogError(ex, "Failed to create episode in database.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            // Cast the episode object to FinalizedEpisodeDto
            var finalizedEpisodeDto = (FinalizedEpisodeDto)episode;

            // Set additional properties on the finalizedEpisodeDto
            finalizedEpisodeDto.EpisodeType = episodeTypeLkp?.EpisodeType;
            finalizedEpisodeDto.EpisodeTypeDescription = episodeTypeLkp?.EpisodeDescription;
            finalizedEpisodeDto.EndCode = endCodeLkp?.EndCode;
            finalizedEpisodeDto.EndCodeDescription = endCodeLkp?.EndCodeDescription;
            finalizedEpisodeDto.ReasonClosedCode = reasonClosedCodeLkp?.ReasonClosedCode;
            finalizedEpisodeDto.ReasonClosedCodeDescription = reasonClosedCodeLkp?.ReasonClosedCodeDescription;
            finalizedEpisodeDto.FinalActionCode = finalActionCodeLkp?.FinalActionCode;
            finalizedEpisodeDto.FinalActionCodeDescription = finalActionCodeLkp?.FinalActionCodeDescription;

            // Create an EventGridEvent with the finalized episode data
            EventGridEvent eventGridEvent = new EventGridEvent(
                subject: "Episode Created",
                eventType: "CreateParticipantScreeningEpisode",
                dataVersion: "1.0",
                data: finalizedEpisodeDto
            );

            // Send the event to Event Grid
            var episodePublisher = _eventGridPublisherClientFactory("episode");
            try
            {
                await episodePublisher.SendEventAsync(eventGridEvent);
                _logger.LogInformation("Sending Episode event to Event Grid: {EventGridEvent}", JsonSerializer.Serialize(eventGridEvent));
            }
            catch (Exception ex)
            {
                // Log error and return InternalServerError if event grid publishing fails
                _logger.LogError(ex, "Failed to send event to event grid");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            // Return OK response if everything is successful
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            // Log error and return InternalServerError if any other error occurs
            _logger.LogError(ex, "Error creating episode.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    private async Task<Episode> MapEpisodeDtoToEpisode(InitialEpisodeDto episodeDto, long? episodeTypeId, long? endCodeId, long? reasonClosedCodeId, long? finalActionCodeId, bool exceptionFlag)
    {
        var organisationId = await GetOrganisationId(episodeDto.OrganisationCode);
        return new Episode
        {
            EpisodeId = episodeDto.EpisodeId,
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
            OrganisationId = organisationId,
            BatchId = episodeDto.BatchId,
            ExceptionFlag = exceptionFlag ? (short)1 : (short)0,
            SrcSysProcessedDatetime = episodeDto.SrcSysProcessedDateTime,
            RecordInsertDatetime = DateTime.UtcNow,
            RecordUpdateDatetime = DateTime.UtcNow
        };
    }

    private async Task<T?> GetCodeObject<T>(string code, string codeName, Func<string, Task<T?>> getObjectMethod) where T : class?
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var codeObject = await getObjectMethod(code);
        if (codeObject == null)
        {
            _logger.LogError("{codeName} '{code}' not found in lookup table.", codeName, code);
            throw new InvalidOperationException($"{codeName} '{code}' not found in lookup table.");
        }
        return codeObject;
    }

    private async Task<long?> GetOrganisationId(string organisationCode)
    {
        if (string.IsNullOrWhiteSpace(organisationCode))
        {
            _logger.LogInformation("Organisation code is null");
            return null;
        }

        var url = $"{Environment.GetEnvironmentVariable("GetOrganisationIdByCodeUrl")}?organisation_code={organisationCode}";
        var response = await _httpRequestService.SendGet(url);
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<long>(await response.Content.ReadAsStreamAsync());
    }
}
