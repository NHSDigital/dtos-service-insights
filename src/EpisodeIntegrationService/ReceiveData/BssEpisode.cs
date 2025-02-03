namespace NHS.ServiceInsights.EpisodeIntegrationService;
public class BssEpisode
{
    public long episode_id { get; set; }
    public long nhs_number { get; set; }
    public required string episode_type { get; set; }
    public DateTime change_db_date_time { get; set; }
    public required string episode_date { get; set; }
    public string? appointment_made { get; set; }
    public string? date_of_foa { get; set; }
    public string? date_of_as { get; set; }
    public string? early_recall_date { get; set; }
    public string? call_recall_status_authorised_by { get; set; }
    public string? end_code { get; set; }
    public string? end_code_last_updated { get; set; }
    public string? bso_organisation_code { get; set; }
    public string? bso_batch_id { get; set; }
    public string? reason_closed_code { get; set; }
    public string? end_point { get; set; }
    public string? final_action_code { get; set; }
}
