using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Settings;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Settings
{
    public class CosmosDbSettings : ICosmosDbSettings
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseId { get; set; }
    }
}
