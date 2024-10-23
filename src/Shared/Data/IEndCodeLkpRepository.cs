namespace NHS.ServiceInsights.Data;

public interface IEndCodeLkpRepository
{
    long GetEndCodeId(string endCode);
}
