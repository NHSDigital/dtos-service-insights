namespace NHS.ServiceInsights.MeshIntegrationService;

using System.ComponentModel.DataAnnotations;

public class RetrieveMeshFileConfig
{
    public string MeshApiBaseUrl { get; set; }
    [Required]
    public string BSSMailBox { get; set; }
    [Required]
    public string MeshPassword { get; set; }
    [Required]
    public string MeshSharedKey { get; set; }
    public string MeshKeyPassphrase { get; set; }
    public string MeshKeyName { get; set; }

}
