namespace NHS.ServiceInsights.Model;

public class Participant
{
    public string nhs_number { get; set; }
    public string? superseded_nhs_number { get; set; }
    public string? next_test_due_date { get; set; }
    public string? gp_practice_code { get; set; }
    public string? subject_status_code { get; set; }
    public string? is_higher_risk { get; set; }
    public string? higher_risk_next_test_due_date { get; set; }
    public string? hr_recall_due_date { get; set; }
    public string? removal_reason { get; set; }
    public string? removal_date { get; set; }
    public string? bso_organisation_code { get; set; }
    public string? early_recall_date { get; set; }
    public string? latest_invitation_date { get; set; }
    public string? preferred_language { get; set; }
    public string? higher_risk_referral_reason_code { get; set; }
    public string? date_irradiated { get; set; }
    public string? is_higher_risk_active { get; set; }
    public string? gene_code { get; set; }
    public string? ntdd_calculation_method { get; set; }
    public string? reason_for_ceasing_code { get; set; }
}

