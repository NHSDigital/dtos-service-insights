
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class ParticipantScreeningEpisodeRepository : IParticipantScreeningEpisodeRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public ParticipantScreeningEpisodeRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool CreateParticipantEpisode(ParticipantScreeningEpisode episode)
    {
        _dbContext.ParticipantScreeningEpisodes.Add(episode);
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
