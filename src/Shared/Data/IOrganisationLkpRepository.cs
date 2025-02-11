using Microsoft.VisualBasic;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IOrganisationLkpRepository
{
    Task<long?> GetOrganisationByCodeAsync(string organisationCode);
    Task<OrganisationLkp?> GetOrganisationAsync(long organisationId);
    Task<IEnumerable<OrganisationLkp>> GetAllOrganisationsAsync();
}
