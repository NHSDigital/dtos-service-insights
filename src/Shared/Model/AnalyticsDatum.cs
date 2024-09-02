using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class AnalyticsDatum
{
    public long EpisodeId { get; set; }
    public string? Episode_Type { get; set; }
    public string? Episode_Date { get; set; }
    public string? Appointment_Made { get; set; }
    public string? Date_Of_Foa { get; set; }
    public string? Date_Of_As { get; set; }
    public string? Early_Recall_Date { get; set; }
    public string? Call_Recall_Status_Authorised_By { get; set; }
    public string? End_Code { get; set; }
    public string? End_Code_Last_Updated { get; set; }
    public string? Bso_Organisation_Code { get; set; }
    public string? Bso_Batch_Id { get; set; }
    public string? Nhs_number { get; set; }
    public string? Gp_practice_id { get; set; }
    public string? Bso_organisation_id { get; set; }
    public string? Next_test_due_date { get; set; }
    public string? Subject_status_code { get; set; }
    public string? Latest_invitation_date { get; set; }
    public string? Removal_reason { get; set; }
    public string? Removal_date { get; set; }
    public string? Ceased_reason { get; set; }
    public string? Reason_for_ceased_code { get; set; }
    public string? Reason_Deducted { get; set; }
    public string? Is_higher_risk { get; set; }
    public string? Higher_risk_next_test_due_date { get; set; }
    public string? Higher_risk_referral_reason_code { get; set; }
    public string? Date_irradiated { get; set; }
    public string? Is_higher_risk_active { get; set; }
    public string? Gene_code { get; set; }
    public string? Ntdd_calculation_method { get; set; }
    public string? Preferred_language { get; set; }
}
