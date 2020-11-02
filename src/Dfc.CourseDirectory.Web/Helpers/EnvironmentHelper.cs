using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Environment;
using Dfc.CourseDirectory.Services;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class EnvironmentHelper : IEnvironmentHelper
    {
        private readonly EnvironmentSettings _environmentSettings;

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
