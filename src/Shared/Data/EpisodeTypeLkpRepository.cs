namespace NHS.ServiceInsights.Data;

public class EpisodeTypeLkpRepository : IEpisodeTypeLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public EpisodeTypeLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public long GetEpisodeTypeId(string episodeType)
    {
        return _dbContext.EpisodeTypeLkps.FirstOrDefault(et => et.EpisodeType == episodeType)?.EpisodeTypeId ?? 0;
    }
}
