using Data;
using Data.Models;

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
}
