namespace NHS.ServiceInsights.Data
{
    public interface IOrganisationLkpRepository
    {
        long GetOrganisationId(string organisationCode);
    }
}
