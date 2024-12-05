using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;
public class ScreeningLkpRepository : IScreeningLkpRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public ScreeningLkpRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ScreeningLkp?> GetScreeningAsync(long screeningId)
    {
        return await _dbContext.ScreeningLkps.FindAsync(screeningId);
    }
}
