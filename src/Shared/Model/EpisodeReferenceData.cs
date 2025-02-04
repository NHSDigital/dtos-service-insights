namespace NHS.ServiceInsights.Model;

public class EpisodeReferenceData
{
    public IDictionary<string, string> EndCodeToIdLookup { get; set; }
    public IDictionary<string, string> EpisodeTypeToIdLookup { get; set; }
    public IDictionary<string, string> FinalActionCodeToIdLookup { get; set; }
    public IDictionary<string, string> ReasonClosedCodeToIdLookup { get; set; }
}

