using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.ParticipantManagementService;

public static class ParticipantRepository
{
    private static readonly List<Participant> Participants = new List<Participant>
    {

        new Participant { nhs_number = "1111111112", next_test_due_date = "2019-08-01", gp_practice_code = "39", subject_status_code = "NORMAL",
                        is_higher_risk = "True", higher_risk_next_test_due_date = "2020-08-01", removal_reason = "null", removal_date = "null",
                        bso_organisation_code = "null", early_recall_date = "null", latest_invitation_date = "null", preferred_language = "null",
                        higher_risk_referral_reason_code = "null", date_irradiated = "2019-07-29", is_higher_risk_active = "False", gene_code = "null",
                        ntdd_calculation_method = "null" },

        new Participant { nhs_number = "1111111110", next_test_due_date = "2019-04-01", gp_practice_code = "null", subject_status_code = "NORMAL",
                        is_higher_risk = "False", higher_risk_next_test_due_date = "2020-04-01", removal_reason = "MENTAL_HOSPITAL", removal_date = "2017-07-28",
                        bso_organisation_code = "null", early_recall_date = "null", latest_invitation_date = "null", preferred_language = "null",
                        higher_risk_referral_reason_code = "null", date_irradiated = "2019-08-02", is_higher_risk_active = "False", gene_code = "null",
                        ntdd_calculation_method = "null" },
    };

    public static Participant GetParticipantByNhsNumber(long NhsNumber)
    {
        return Participants.Find(p => p.nhs_number == NhsNumber.ToString());
    }
}
