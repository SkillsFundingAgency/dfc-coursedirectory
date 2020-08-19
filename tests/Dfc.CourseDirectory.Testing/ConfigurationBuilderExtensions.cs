using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Dfc.CourseDirectory.Testing
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddTestConfigurationSources(this IConfigurationBuilder builder)
        {
            var fileProvider = new ManifestEmbeddedFileProvider(typeof(ConfigurationBuilderExtensions).Assembly);

            builder
                .AddJsonFile(fileProvider, "appsettings.Testing.json", optional: false, reloadOnChange: false)
                .AddUserSecrets(typeof(ConfigurationBuilderExtensions).Assembly)
                .AddEnvironmentVariables();

            return builder;
        }
    }
}
