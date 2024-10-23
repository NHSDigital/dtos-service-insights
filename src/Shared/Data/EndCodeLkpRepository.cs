namespace NHS.ServiceInsights.Data;

public class EndCodeLkpRepository : IEndCodeLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public EndCodeLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public long GetEndCodeId(string endCode)
    {
        return _dbContext.EndCodeLkps.FirstOrDefault(ec => ec.EndCode == endCode)?.EndCodeId ?? 0;
    }
}
