using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dtos_service_insights_tests.Helpers;
using dtos_service_insights_tests.Models;

namespace dtos_service_insights_tests.Contexts;

public class SmokeTestsContexts
{
    public string FilePath { get; set; }

    public RecordTypesEnum RecordType { get; set; }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public List<string>? NhsNumbers { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

}
