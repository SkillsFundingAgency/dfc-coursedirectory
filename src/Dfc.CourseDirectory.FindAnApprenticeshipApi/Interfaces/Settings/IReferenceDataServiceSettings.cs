using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Settings
{
    public interface IReferenceDataServiceSettings
    {
        string ApiUrl { get; set; }
        string ApiKey { get; set; }
        int MinutesToCache { get; set; }
    }
}
