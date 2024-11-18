using NHS.ServiceInsights.Model;
using Microsoft.EntityFrameworkCore;

namespace NHS.ServiceInsights.Data;

public class ParticipantScreeningEpisodeRepository : IParticipantScreeningEpisodeRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public ParticipantScreeningEpisodeRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CreateParticipantEpisode(ParticipantScreeningEpisode episode)
    {
        _dbContext.ParticipantScreeningEpisodes.Add(episode);

        if (1 == await _dbContext.SaveChangesAsync())
        {
            return true;
        }

        return false;
    }

    public async Task<EpisodesDataPage> GetParticipantScreeningEpisode (int page, int pageSize, DateTime? startDate, DateTime? endDate, int skip)
    {
        var query = _dbContext.ParticipantScreeningEpisodes
            .Where(x => (!startDate.HasValue || x.RecordInsertDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordInsertDatetime <= endDate))
            .OrderBy(x => x.RecordInsertDatetime)
            .Skip(skip)
            .Take(pageSize);

        var data = await query.ToListAsync();

        int count = await _dbContext.ParticipantScreeningEpisodes
            .Where(x => (!startDate.HasValue || x.RecordInsertDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordInsertDatetime <= endDate))
            .CountAsync();

        int totalPages = (int)Math.Ceiling((double)count/(double)pageSize);
        int totalRemainingPages = totalPages - page;

        var episodesPage = new EpisodesDataPage()
        {
            episodes = data,
            TotalResults = count,
            TotalPages = totalPages,
            TotalRemainingPages = totalRemainingPages
        };

        return episodesPage;
    }
}
