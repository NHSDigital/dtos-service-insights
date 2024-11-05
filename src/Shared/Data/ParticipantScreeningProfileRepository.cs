using Microsoft.EntityFrameworkCore;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class ParticipantScreeningProfileRepository : IParticipantScreeningProfileRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public ParticipantScreeningProfileRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CreateParticipantProfile(ParticipantScreeningProfile profile)
    {
        _dbContext.ParticipantScreeningProfiles.Add(profile);

        if (1 == await _dbContext.SaveChangesAsync())
        {
            return true;
        }

        return false;
    }

    public async Task<ProfilesDataPage> GetParticipantProfile(int page, int pageSize, DateTime? startDate, DateTime? endDate, int skip)
    {
        var query = _dbContext.ParticipantScreeningProfiles
            .Where(x => (!startDate.HasValue || x.RecordInsertDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordInsertDatetime <= endDate))
            .OrderBy(x => x.RecordInsertDatetime)
            .Skip(skip)
            .Take(pageSize);

        var data = await query.ToListAsync();

        bool hasMoreData = await _dbContext.ParticipantScreeningProfiles
            .Where(x => (!startDate.HasValue || x.RecordInsertDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordInsertDatetime <= endDate))
            .CountAsync() > skip + pageSize;

        var profilesPage = new ProfilesDataPage()
        {
            profiles = data,
            page = page,
            pageSize = pageSize,
            hasMoreData = hasMoreData
        };

        return profilesPage;
    }
}
