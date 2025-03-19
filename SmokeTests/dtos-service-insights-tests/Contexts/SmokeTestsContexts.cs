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
    public List<string>? NhsNumbers { get; set; }
}
