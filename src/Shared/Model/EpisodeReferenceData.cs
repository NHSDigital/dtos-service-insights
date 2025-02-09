namespace NHS.ServiceInsights.Model;

public class EpisodeReferenceData
{
    public IDictionary<string, string> EndCodeDescriptions { get; set; }
    public IDictionary<string, string> EpisodeTypeDescriptions { get; set; }
    public IDictionary<string, string> FinalActionCodeDescriptions { get; set; }
    public IDictionary<string, string> ReasonClosedCodeDescriptions { get; set; }
}

