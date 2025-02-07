using Microsoft.VisualBasic;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IOrganisationLkpRepository
{
    //Task<OrganisationLkp?> GetOrganisationByCodeAsync(string organisationCode);
    Task<long?> GetOrganisationByCodeAsync(string organisationCode);
    Task<OrganisationLkp?> GetOrganisationAsync(long organisationId);
    Task<IEnumerable<OrganisationLkp>> GetAllOrganisationsAsync();
}
