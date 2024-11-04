using Microsoft.EntityFrameworkCore;

namespace NHS.ServiceInsights.Data;

public class EpisodeTypeLkpRepository : IEpisodeTypeLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public EpisodeTypeLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> GetEpisodeTypeIdAsync(string episodeType)
    {
        var episodeTypeLkp = await _dbContext.EpisodeTypeLkps.FirstOrDefaultAsync(et => et.EpisodeType == episodeType);
        return episodeTypeLkp?.EpisodeTypeId ?? 0;
    }
}
