using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Settings;

namespace Dfc.Providerportal.FindAnApprenticeship.Settings
{
    public class CosmosDbSettings : ICosmosDbSettings
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseId { get; set; }
    }
}
