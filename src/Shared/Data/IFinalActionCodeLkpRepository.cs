namespace NHS.ServiceInsights.Data;

public interface IFinalActionCodeLkpRepository
{
    Task<long> GetFinalActionCodeIdAsync(string finalActionCode);
}
