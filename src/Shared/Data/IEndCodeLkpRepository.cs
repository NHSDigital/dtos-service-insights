namespace NHS.ServiceInsights.Data;

public interface IEndCodeLkpRepository
{
    Task<long> GetEndCodeIdAsync(string endCode);
}
