namespace NHS.ServiceInsights.Data
{
    public class OrganisationLkpRepository : IOrganisationLkpRepository
    {
        private readonly ServiceInsightsDbContext _dbContext;

        public OrganisationLkpRepository(ServiceInsightsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public long GetOrganisationId(string organisationCode)
        {
            return _dbContext.OrganisationLkps.FirstOrDefault(o => o.OrganisationCode == organisationCode)?.OrganisationId ?? 0;
        }
    }
}
