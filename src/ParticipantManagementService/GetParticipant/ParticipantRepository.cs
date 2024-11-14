using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.ParticipantManagementService;

public static class ParticipantRepository
{
    private static readonly List<Participant> Participants = new List<Participant>
    {

        new Participant { nhs_number = "1111111112", next_test_due_date = "null", gp_practice_code = "39", subject_status_code = "NORMAL",
                        is_higher_risk = "false", higher_risk_next_test_due_date = "null", removal_reason = "null", removal_date = "null",
                        bso_organisation_code = "null", early_recall_date = "null", latest_invitation_date = "null", preferred_language = "null",
                        higher_risk_referral_reason_code = "null", date_irradiated = "null", is_higher_risk_active = "false", gene_code = "null",
                        ntdd_calculation_method = "null" },

        new Participant { nhs_number = "1111111110", next_test_due_date = "null", gp_practice_code = "null", subject_status_code = "NORMAL",
                        is_higher_risk = "false", higher_risk_next_test_due_date = "null", removal_reason = "MENTAL_HOSPITAL", removal_date = "2017-07-28",
                        bso_organisation_code = "null", early_recall_date = "null", latest_invitation_date = "null", preferred_language = "null",
                        higher_risk_referral_reason_code = "null", date_irradiated = "null", is_higher_risk_active = "false", gene_code = "null",
                        ntdd_calculation_method = "null" },
    };

    public static Participant GetParticipantByNhsNumber(string NhsNumber)
    {
        return Participants.Find(p => p.nhs_number == NhsNumber);
    }
}
