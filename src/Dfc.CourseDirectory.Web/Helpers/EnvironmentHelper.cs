using System;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Environment;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class EnvironmentHelper : IEnvironmentHelper
    {
        private readonly EnvironmentSettings _environmentSettings;

        public EnvironmentHelper(IOptions<EnvironmentSettings> environmentSettings)
        {
            if (environmentSettings == null)
            {
                throw new ArgumentNullException(nameof(environmentSettings));
            }

            _environmentSettings = environmentSettings.Value;
        }

        public EnvironmentType GetEnvironmentType()
        {
            return _environmentSettings.EnvironmentName.ToEnum(EnvironmentType.Undefined);
        }
    }
}
