
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IParticipantScreeningProfileRepository
{
    bool CreateParticipantProfile(ParticipantScreeningProfile profile);
}
