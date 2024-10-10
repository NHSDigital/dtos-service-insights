
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IParticipantScreeningEpisodeRepository
{
    Task<bool> CreateParticipantEpisode(ParticipantScreeningEpisode episode);
}
