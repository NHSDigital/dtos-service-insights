using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class ParticipantScreeningProfileRepository : IParticipantScreeningProfileRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public ParticipantScreeningProfileRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CreateParticipantProfile(ParticipantScreeningProfile profile)
    {
        _dbContext.ParticipantScreeningProfiles.Add(profile);

        if (1 == await _dbContext.SaveChangesAsync())
        {
            return true;
        }

        return false;
    }
}
