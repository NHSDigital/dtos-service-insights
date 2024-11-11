using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class OrganisationLkp
{
    public long OrganisationId { get; set; }

    public string? ScreeningName { get; set; }

    public string? OrganisationCode { get; set; }

    public string? OrganisationName { get; set; }

    public string? OrganisationType { get; set; }

    public string? IsActive { get; set; }
}
