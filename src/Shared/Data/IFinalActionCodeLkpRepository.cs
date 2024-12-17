using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IFinalActionCodeLkpRepository
{
    Task<long?> GetFinalActionCodeIdAsync(string finalActionCode);

    Task<FinalActionCodeLkp?> GetFinalActionCodeLkp(string finalActionCode);
}
