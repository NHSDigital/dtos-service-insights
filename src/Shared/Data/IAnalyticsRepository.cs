
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IAnalyticsRepository
{
    bool SaveData(Analytic datum);
    bool SaveData(List<Analytic> datum);
}
