
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class EpisodeRepository : IEpisodeRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public EpisodeRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void CreateEpisode(Episode episode)
    {
        _dbContext.Episodes.Add(episode);
        _dbContext.SaveChanges();
    }

    public Episode GetEpisode(string episodeId)
    {

        return _dbContext.Episodes.Find(episodeId);

    }
    public void UpdateEpisode(Episode episode)
    {
        _dbContext.Episodes.Update(episode);
        _dbContext.SaveChanges();
    }

}
