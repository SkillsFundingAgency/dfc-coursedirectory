using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddTestConfigurationSources(this IConfigurationBuilder builder)
        {
            var fileProvider = new ManifestEmbeddedFileProvider(typeof(ConfigurationBuilderExtensions).Assembly);

            builder
                .AddJsonFile(fileProvider, "appsettings.Testing.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();

            return builder;
        }
    }
}
