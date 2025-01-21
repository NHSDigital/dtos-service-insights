using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IOrganisationLkpRepository
{
    Task<OrganisationLkp?> GetOrganisationLkp(string organisationCode);
    Task<OrganisationLkp?> GetOrganisationAsync(long organisationId);
    Task<IEnumerable<OrganisationLkp>> GetAllOrganisationsAsync();
}
