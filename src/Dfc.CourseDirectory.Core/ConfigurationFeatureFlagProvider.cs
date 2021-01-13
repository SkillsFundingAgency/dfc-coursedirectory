using System;
using System.Collections.Generic;
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

        public ISet<string> GetFeatureFlags() => new HashSet<string>(
            (_configuration[ConfigurationKey] ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries),
            StringComparer.OrdinalIgnoreCase);
    }
}
