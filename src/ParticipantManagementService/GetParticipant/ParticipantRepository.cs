using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.ParticipantManagementService;

public static class ParticipantRepository
{
    private static readonly List<InitialParticipantDto> Participants = new List<InitialParticipantDto>
    {

        new InitialParticipantDto { NhsNumber = 1111111112, ScreeningName = "Breast Screening", ScreeningId = 1, NextTestDueDate = new DateOnly(2019, 08, 01), NextTestDueDateCalculationMethod = "ROUTINE", ParticipantScreeningStatus = "NORMAL",
                        ScreeningCeasedReason = "PERSONAL_WELFARE",  IsHigherRisk = 1, IsHigherRiskActive = 1, HigherRiskNextTestDueDate = new DateOnly(2020, 02, 01), HigherRiskReferralReasonCode = "", DateIrradiated = new DateOnly(2019, 12, 01),
                        GeneCode = "BRCA1"},

        new InitialParticipantDto { NhsNumber = 1111111110, ScreeningName = "Breast Screening", ScreeningId = 2, NextTestDueDate = new DateOnly(2019, 08, 01), NextTestDueDateCalculationMethod = "ROUTINE", ParticipantScreeningStatus = "NORMAL",
                        ScreeningCeasedReason = "PERSONAL_WELFARE",  IsHigherRisk = 1, IsHigherRiskActive = 0, HigherRiskNextTestDueDate = null, HigherRiskReferralReasonCode = "OTHER_GENE_MUTATIONS", DateIrradiated = new DateOnly(2019, 08, 01),
                        GeneCode = "STK11"},
    };

    public static InitialParticipantDto GetParticipantByNhsNumber(long NhsNumber)
    {
        return Participants.Find(p => p.NhsNumber == NhsNumber);
    }
}
