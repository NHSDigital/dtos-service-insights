using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IParticipantScreeningProfileRepository
{
    Task<bool> CreateParticipantProfile(ParticipantScreeningProfile profile);

    Task<ProfilesDataPage> GetParticipantProfile(int page, int pageSize, DateTime? startDate, DateTime? endDate, int skip);
}
