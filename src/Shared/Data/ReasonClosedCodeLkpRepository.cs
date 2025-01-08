using Microsoft.EntityFrameworkCore;
using NHS.ServiceInsights.Model;

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

    public async Task<ReasonClosedCodeLkp?> GetReasonClosedLkp(string reasonClosedCode)
    {
        var reasonClosedCodeLkp = await _dbContext.ReasonClosedCodeLkps.FirstOrDefaultAsync(ec => ec.ReasonClosedCode == reasonClosedCode);
        return reasonClosedCodeLkp;
    }

    public async Task<IEnumerable<ReasonClosedCodeLkp>> GetAllReasonClosedCodesAsync()
    {
        var reasonClosedCodeLkps = await _dbContext.ReasonClosedCodeLkps.ToListAsync();
        return reasonClosedCodeLkps;
    }

}
