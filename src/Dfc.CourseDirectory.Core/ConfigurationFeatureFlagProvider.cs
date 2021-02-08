using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Dfc.CourseDirectory.Core
{
    public class ConfigurationFeatureFlagProvider : IFeatureFlagProvider
    {
        private const string ConfigurationKey = "FeatureFlags";

        private readonly IConfiguration _configuration;

        public ConfigurationFeatureFlagProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IReadOnlyCollection<string> GetFeatureFlags() =>
            new HashSet<string>(
                _configuration[ConfigurationKey]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
    }
}
