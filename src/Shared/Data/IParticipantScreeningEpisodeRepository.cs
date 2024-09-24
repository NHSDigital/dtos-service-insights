
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IParticipantScreeningEpisodeRepository
{
    bool CreateParticipantEpisode(ParticipantScreeningEpisode episode);
}
