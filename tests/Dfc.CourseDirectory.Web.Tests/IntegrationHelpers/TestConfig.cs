using Microsoft.Extensions.Configuration;

namespace Dfc.CourseDirectory.Web.Tests.IntegrationHelpers
{
    public class TestConfig
    {
        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .Build();
        }

        public static T GetSettings<T>(string sectionName) where T : new()
        {
            var settings = new T();
            var iConfig = GetIConfigurationRoot(System.Environment.CurrentDirectory + @"\IntegrationHelpers");

            iConfig
                .GetSection(sectionName)
                .Bind(settings);

            return settings;
        }
    }
}
