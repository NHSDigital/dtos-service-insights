namespace NHS.ServiceInsights.Model;

public class EpisodeReferenceData
{
    public IDictionary<string, string> EndCodes { get; set; }
    public IDictionary<string, string> EpisodeTypes { get; set; }
    public IDictionary<string, string> FinalActionCodes { get; set; }
    public IDictionary<string, string> ReasonClosedCodes { get; set; }
}

