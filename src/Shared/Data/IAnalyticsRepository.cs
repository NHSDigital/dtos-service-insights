
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IAnalyticsRepository
{
    bool SaveData(AnalyticsDatum datum);
    bool SaveData(List<AnalyticsDatum> datum);
}
