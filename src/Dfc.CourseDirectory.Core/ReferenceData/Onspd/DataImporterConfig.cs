using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.ReferenceData.Onspd
{
    public class DataImporterConfig
    {
        public string ProviderQueryPort { get; set; }
        public string geoportal_url { get; set; }
        public string arcgisurl { get; set; }
        public string checkForSecureUri { get; set; }

    }
}
