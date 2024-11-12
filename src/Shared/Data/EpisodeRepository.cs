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

    public async Task<Episode?> GetEpisodeAsync(long episodeId)
    {

        return await _dbContext.Episodes.FindAsync(episodeId);
    }

    public async Task UpdateEpisode(Episode episode)
    {
        _dbContext.Episodes.Update(episode);
        await _dbContext.SaveChangesAsync();
    }
}
