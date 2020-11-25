using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Settings;

namespace Dfc.Providerportal.FindAnApprenticeship.Settings
{
    public class CosmosDbCollectionSettings : ICosmosDbCollectionSettings
    {
        public string StandardsCollectionId { get; set; }
        public string FrameworksCollectionId { get; set; }
        public string ApprenticeshipCollectionId { get; set; }
        public string ProgTypesCollectionId { get; set; }
    }
}
