
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ServiceInsightsDbContext _dbContext;

    public AnalyticsRepository(ServiceInsightsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool SaveData(Analytic datum)
    {
        try
        {
            _dbContext.Analytics.Add(datum);
            _dbContext.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public bool SaveData(List<Analytic> datum)
    {
        try
        {
            _dbContext.Analytics.AddRange(datum);
            _dbContext.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
