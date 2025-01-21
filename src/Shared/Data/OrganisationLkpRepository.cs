using Microsoft.EntityFrameworkCore;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class OrganisationLkpRepository : IOrganisationLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public OrganisationLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<OrganisationLkp?> GetOrganisationLkp(string organisationCode)
    {
        var organisationLkp = await _dbContext.OrganisationLkps.FirstOrDefaultAsync(ol => ol.OrganisationCode == organisationCode);
        return organisationLkp;
    }

    public async Task<OrganisationLkp?> GetOrganisationAsync(long organisationId)
    {
        return await _dbContext.OrganisationLkps.FindAsync(organisationId);
    }

    public async Task<IEnumerable<OrganisationLkp>> GetAllOrganisationsAsync()
    {
        var organisationLkps = await _dbContext.OrganisationLkps.ToListAsync();
        return organisationLkps;
    }
}
