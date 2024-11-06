using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IParticipantScreeningEpisodeRepository
{
    Task<bool> CreateParticipantEpisode(ParticipantScreeningEpisode episode);
    public Task<ProfilesDataPage> GetParticipantScreeningEpisode (int page, int pageSize, DateTime? startDate, DateTime? endDate, int skip);
}
