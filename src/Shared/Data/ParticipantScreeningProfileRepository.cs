using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class ParticipantScreeningProfileRepository : IParticipantScreeningProfileRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public ParticipantScreeningProfileRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool CreateParticipantProfile(ParticipantScreeningProfile profile)
    {
        _dbContext.ParticipantScreeningProfiles.Add(profile);
        try
        {
            if (1 == _dbContext.SaveChanges())
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
