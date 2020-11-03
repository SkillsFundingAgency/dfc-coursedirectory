using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Environment;
using Dfc.CourseDirectory.Models.Models.Environment;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    
    public class EnvironmentHelper : IEnvironmentHelper
    {
        private readonly IEnvironmentSettings _environmentSettings;
        public EnvironmentHelper(IOptions<EnvironmentSettings> environmentSettings)
        {
            Throw.IfNull(environmentSettings, nameof(environmentSettings));
            _environmentSettings = environmentSettings.Value;
        }
        public EnvironmentType GetEnvironmentType()
        {
            return _environmentSettings.EnvironmentName.ToEnum(EnvironmentType.Undefined);
        }

    }
}
