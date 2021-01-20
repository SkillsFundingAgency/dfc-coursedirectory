using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Settings;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Settings
{
    public class ReferenceDataServiceSettings : IReferenceDataServiceSettings
    {
        public ReferenceDataServiceSettings()
        {
            CacheKeyPrefix = "ReferenceData_";
            MinutesToCache = 23 * 60;
        }

        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int MinutesToCache { get; set; }
        public string CacheKeyPrefix { get; set; }
    }
}
