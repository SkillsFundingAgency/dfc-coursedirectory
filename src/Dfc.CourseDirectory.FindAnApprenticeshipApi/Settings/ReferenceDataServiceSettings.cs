using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.Providerportal.FindAnApprenticeship.Settings
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
