namespace NHS.ServiceInsights.Data;

public interface IReasonClosedCodeLkpRepository
{
    Task<long> GetReasonClosedCodeIdAsync(string reasonClosedCode);
}
