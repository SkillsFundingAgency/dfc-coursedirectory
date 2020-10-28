
using System;
using Dfc.ProviderPortal.FindACourse.Interfaces;


namespace Dfc.ProviderPortal.FindACourse.Settings
{
    public class QualificationServiceSettings : IQualificationServiceSettings
    {
        public string SearchService { get; set; }
        public string QueryKey { get; set; }
        public string AdminKey { get; set; }
        public string Index { get; set; }
    }
}
