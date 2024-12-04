namespace NHS.ServiceInsights.Model;
public class ParticipantDto
{
    public long NhsNumber { get; set; }
    public string ScreeningName { get; set; }
    public DateOnly? NextTestDueDate { get; set; }
    public string? NextTestDueDateCalculationMethod { get; set; }
    public string? ParticipantScreeningStatus { get; set; }
    public string? ScreeningCeasedReason { get; set; }
    public short? IsHigherRisk { get; set; }
    public short? IsHigherRiskActive { get; set; }
    public DateOnly? HigherRiskNextTestDueDate { get; set; }
    public string? HigherRiskReferralReasonCode { get; set; }
    public DateOnly? DateIrradiated { get; set; }
    public string? GeneCode { get; set; }
}
