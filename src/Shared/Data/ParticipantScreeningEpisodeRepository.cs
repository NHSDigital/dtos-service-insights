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
            .Where(x => (!startDate.HasValue || x.RecordUpdateDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordUpdateDatetime <= endDate) && x.ExceptionFlag == 0)
            .OrderBy(x => x.RecordUpdateDatetime)
            .Skip(skip)
            .Take(pageSize);

        var data = await query.ToListAsync();

        int count = await _dbContext.ParticipantScreeningEpisodes
            .Where(x => (!startDate.HasValue || x.RecordUpdateDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordUpdateDatetime <= endDate) && x.ExceptionFlag == 0)
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
