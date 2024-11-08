using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IOrganisationLkpRepository
{
    Task<OrganisationLkp?> GetOrganisationAsync(string organisationId);
}
