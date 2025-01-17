using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class ParticipantScreeningProfile
{
    public long Id { get; set; }

    public long NhsNumber { get; set; }

    public string? ScreeningName { get; set; }

    public string? PrimaryCareProvider { get; set; }

    public string? PreferredLanguage { get; set; }

    public string? ReasonForRemoval { get; set; }

    public DateOnly? ReasonForRemovalDt { get; set; }

    public DateOnly? NextTestDueDate { get; set; }

    public string? NextTestDueDateCalcMethod { get; set; }

    public string? ParticipantScreeningStatus { get; set; }

    public string? ScreeningCeasedReason { get; set; }

    public short? IsHigherRisk { get; set; }

    public short? IsHigherRiskActive { get; set; }

    public DateOnly? HigherRiskNextTestDueDate { get; set; }

    public string? HigherRiskReferralReasonCode { get; set; }

    public string? HrReasonCodeDescription { get; set; }

    public DateOnly? DateIrradiated { get; set; }

    public string? GeneCode { get; set; }

    public string? GeneCodeDescription { get; set; }

    public DateTime? SrcSysProcessedDatetime { get; set; }

    public DateTime? RecordInsertDatetime { get; set; }

    public DateTime? RecordUpdateDatetime { get; set; }

    public bool ExceptionFlag { get; set; }
}
