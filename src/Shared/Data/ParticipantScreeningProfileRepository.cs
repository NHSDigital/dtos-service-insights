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
            .Where(x => (!startDate.HasValue || x.RecordUpdateDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordUpdateDatetime <= endDate))
            .OrderBy(x => x.RecordUpdateDatetime)
            .Skip(skip)
            .Take(pageSize);

        var data = await query.ToListAsync();

        int count = await _dbContext.ParticipantScreeningProfiles
            .Where(x => (!startDate.HasValue || x.RecordUpdateDatetime >= startDate) &&
                        (!endDate.HasValue || x.RecordUpdateDatetime <= endDate))
            .CountAsync();

        int totalPages = (int)Math.Ceiling((double)count/(double)pageSize);
        int totalRemainingPages = totalPages - page;
        if (totalRemainingPages < 0) totalRemainingPages = 0;

        var profilesPage = new ProfilesDataPage()
        {
            Profiles = data,
            TotalResults = count,
            TotalPages = totalPages,
            TotalRemainingPages = totalRemainingPages
        };

        return profilesPage;
    }
}
