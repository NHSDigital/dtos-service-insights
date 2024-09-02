using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class Episode
{
    public long EpisodeId { get; set; }
    public string? Episode_Type { get; set; }
    public string? Bso_Organisation_Code { get; set; }
    public string? Episode_Date { get; set; }
    public string? End_Code { get; set; }
    public string? Date_Of_Foa { get; set; }
    public string? Date_Of_As { get; set; }
    public string? Appointment_Made { get; set; }
    public string? Call_Recall_Status_Authorised_By { get; set; }
    public string? Early_Recall_Date { get; set; }
    public string? End_Code_Last_Updated { get; set; }
    public string? Bso_Batch_Id { get; set; }
}
