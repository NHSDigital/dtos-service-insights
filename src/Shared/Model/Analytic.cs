using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class Analytic
{
    public long Id { get; set; }

    public string? EpisodeId { get; set; }

    public string? EpisodeType { get; set; }

    public string? EpisodeDate { get; set; }

    public string? AppointmentMade { get; set; }

    public string? DateOfFoa { get; set; }

    public string? DateOfAs { get; set; }

    public string? EarlyRecallDate { get; set; }

    public string? CallRecallStatusAuthorisedBy { get; set; }

    public string? EndCode { get; set; }

    public string? EndCodeLastUpdated { get; set; }

    public string? BsoOrganisationCode { get; set; }

    public string? BsoBatchId { get; set; }

    public string? NhsNumber { get; set; }

    public string? GpPracticeId { get; set; }

    public string? BsoOrganisationId { get; set; }

    public string? NextTestDueDate { get; set; }

    public string? SubjectStatusCode { get; set; }

    public string? LatestInvitationDate { get; set; }

    public string? RemovalReason { get; set; }

    public string? RemovalDate { get; set; }

    public string? CeasedReason { get; set; }

    public string? ReasonForCeasedCode { get; set; }

    public string? ReasonDeducted { get; set; }

    public string? IsHigherRisk { get; set; }

    public string? HigherRiskNextTestDueDate { get; set; }

    public string? HigherRiskReferralReasonCode { get; set; }

    public string? DateIrradiated { get; set; }

    public string? IsHigherRiskActive { get; set; }

    public string? GeneCode { get; set; }

    public string? NtddCalculationMethod { get; set; }

    public string? PreferredLanguage { get; set; }
}
