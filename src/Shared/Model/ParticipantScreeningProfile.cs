using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class ParticipantScreeningProfile
{
    public long Id { get; set; }

    public string NhsNumber { get; set; } = null!;

    public string? ScreeningName { get; set; }

    public string? PrimaryCareProvider { get; set; }

    public string? PreferredLanguage { get; set; }

    public string? ReasonForRemoval { get; set; }

    public string? ReasonForRemovalDt { get; set; }

    public string? NextTestDueDate { get; set; }

    public string? NextTestDueDateCalculationMethod { get; set; }

    public string? ParticipantScreeningStatus { get; set; }

    public string? ScreeningCeasedReason { get; set; }

    public string? IsHigherRisk { get; set; }

    public string? IsHigherRiskActive { get; set; }

    public string? HigherRiskNextTestDueDate { get; set; }

    public string? HigherRiskReferralReasonCode { get; set; }

    public string? HrReasonCodeDescription { get; set; }

    public string? DateIrradiated { get; set; }

    public string? GeneCode { get; set; }

    public string? GeneCodeDescription { get; set; }

    public DateTime? RecordInsertDatetime { get; set; }
}
