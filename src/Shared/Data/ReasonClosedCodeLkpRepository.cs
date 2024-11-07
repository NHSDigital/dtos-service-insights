using Microsoft.EntityFrameworkCore;

namespace NHS.ServiceInsights.Data;

public class ReasonClosedCodeLkpRepository : IReasonClosedCodeLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public ReasonClosedCodeLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long?> GetReasonClosedCodeIdAsync(string reasonClosedCode)
    {
        var reasonClosedCodeLkp = await _dbContext.ReasonClosedCodeLkps.FirstOrDefaultAsync(ec => ec.ReasonClosedCode == reasonClosedCode);
        return reasonClosedCodeLkp?.ReasonClosedCodeId;
    }
}
