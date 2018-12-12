using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class ProviderSearchSettings : IProviderSearchSettings
    {
        public string ApiUrl { get; set; }
        public string ApiVersion { get; set; }
        public string ApiKey { get; set; }
    }
}
