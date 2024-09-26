
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IParticipantScreeningProfileRepository
{
    Task<bool> CreateParticipantProfile(ParticipantScreeningProfile profile);
}
