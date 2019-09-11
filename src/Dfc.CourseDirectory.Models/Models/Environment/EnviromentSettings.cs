using Dfc.CourseDirectory.Models.Interfaces.Environment;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Environment
{
    public class EnvironmentSettings : IEnvironmentSettings
    {
        public string EnvironmentName { get; set; }
    }
}
