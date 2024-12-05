using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IScreeningLkpRepository
{
    Task<ScreeningLkp?> GetScreeningAsync(long screeningId);
}
