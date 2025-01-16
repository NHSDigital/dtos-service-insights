using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.EpisodeIntegrationService;

    public class ReferenceData
    {
        public List<EpisodeTypeLkp> EpisodeTypes { get; set; }
        public List<EndCodeLkp> EndCodes { get; set; }
        public List<ReasonClosedCodeLkp> ReasonClosedCodes { get; set; }
        public List<FinalActionCodeLkp> FinalActionCodes { get; set; }
    }


