using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class OrganisationLkpRepository : IOrganisationLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public OrganisationLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrganisationLkp?> GetOrganisationAsync(long organisationId)
    {
        return await _dbContext.OrganisationLkps.FindAsync(organisationId);
    }
}
