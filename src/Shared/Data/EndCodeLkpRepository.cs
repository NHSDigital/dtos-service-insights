using Microsoft.EntityFrameworkCore;

namespace NHS.ServiceInsights.Data;

public class EndCodeLkpRepository : IEndCodeLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public EndCodeLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long?> GetEndCodeIdAsync(string endCode)
    {
        var endCodeLkp = await _dbContext.EndCodeLkps.FirstOrDefaultAsync(ec => ec.EndCode == endCode);
        return endCodeLkp?.EndCodeId;
    }
}