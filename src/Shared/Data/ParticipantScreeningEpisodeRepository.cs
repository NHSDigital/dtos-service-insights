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

    public async Task<ProfilesDataPage> GetParticipantScreeningEpisode (int page, int pageSize, DateTime? startDate, DateTime? endDate, int skip)
    {
       var query = _dbContext.ParticipantScreeningEpisodes
            .Where(x => (!startDate.HasValue || x.RecordInsertDatetime >= startDate) &&  // Apply start date filter if provided
                        (!endDate.HasValue || x.RecordInsertDatetime <= endDate))        // Apply end date filter if provided
            .OrderBy(x => x.RecordInsertDatetime)                                        // Order by date for consistent paging
            .Skip(skip)                                                  // Skip the records from previous pages
            .Take(pageSize);                                             // Take only the records for the current page

        // Execute the query and retrieve the results
        var data = await query.ToListAsync();

        // Check if there's more data available for the next page
        bool hasMoreData = await _dbContext.ParticipantScreeningEpisodes
            .Where(x => (!startDate.HasValue || x.RecordInsertDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordInsertDatetime <= endDate))
            .CountAsync() > skip + pageSize;

        var profilesPage = new ProfilesDataPage()
        {
            episodes = data,
            page = page,
            pageSize = pageSize,
            hasMoreData = hasMoreData
        };

        return profilesPage;
    }
}
