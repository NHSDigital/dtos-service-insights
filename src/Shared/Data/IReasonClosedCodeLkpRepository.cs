using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IReasonClosedCodeLkpRepository
{
    Task<long?> GetReasonClosedCodeIdAsync(string reasonClosedCode);
    Task<ReasonClosedCodeLkp?> GetReasonClosedLkp(string reasonClosedCode);
    Task<IEnumerable<ReasonClosedCodeLkp>> GetAllReasonClosedCodesAsync();
}
