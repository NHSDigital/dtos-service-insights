using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IEndCodeLkpRepository
{
    Task<long?> GetEndCodeIdAsync(string endCode);

    Task<EndCodeLkp?> GetEndCodeLkp(string endCode);

    Task<IEnumerable<EndCodeLkp>> GetAllEndCodesAsync();
}
