using Microsoft.EntityFrameworkCore;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class FinalActionCodeLkpRepository : IFinalActionCodeLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public FinalActionCodeLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long?> GetFinalActionCodeIdAsync(string finalActionCode)
    {
        var finalActionCodeLkp = await _dbContext.FinalActionCodeLkps.FirstOrDefaultAsync(ec => ec.FinalActionCode == finalActionCode);
        return finalActionCodeLkp?.FinalActionCodeId;
    }

    public async Task<FinalActionCodeLkp?> GetFinalActionCodeLkp(string finalActionCode)
    {
        var finalActionCodeLkp = await _dbContext.FinalActionCodeLkps.FirstOrDefaultAsync(ec => ec.FinalActionCode == finalActionCode);
        return finalActionCodeLkp;
    }

    public async Task<IEnumerable<FinalActionCodeLkp>> GetAllFinalActionCodesAsync()
    {
        var finalActionCodeLkps = await _dbContext.FinalActionCodeLkps.ToListAsync();
        return finalActionCodeLkps;
    }
}
