namespace NHS.ServiceInsights.Model;
public class FinalizedParticipantDto
{
    public long NhsNumber { get; set; }
    public long ScreeningId { get; set; }
    public string? ReasonForRemoval { get; set; }
    public DateOnly? ReasonForRemovalDt { get; set; }
    public DateOnly? NextTestDueDate { get; set; }
    public string? NextTestDueDateCalculationMethod { get; set; }
    public string? ParticipantScreeningStatus { get; set; }
    public string? ScreeningCeasedReason { get; set; }
    public short? IsHigherRisk { get; set; }
    public short? IsHigherRiskActive { get; set; }
    public DateTime SrcSysProcessedDateTime { get; set; }
    public DateOnly? HigherRiskNextTestDueDate { get; set; }
    public string? HigherRiskReferralReasonCode { get; set; }
    public string? HigherRiskReasonCodeDescription { get; set; }
    public DateOnly? DateIrradiated { get; set; }
    public string? GeneCode { get; set; }
    public string? GeneDescription { get; set; }
}
