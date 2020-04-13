namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb
{
    public class Configuration
    {
        public string DatabaseId { get; set; } = "providerportal";
        public string ApprenticeshipCollectionName { get; set; } = "apprenticeship";
        public string ProviderCollectionName { get; set; } = "ukrlp";
        public string FrameworksCollectionName { get; set; } = "frameworks";
        public string StandardsCollectionName { get; set; } = "standards";
        public string VenuesCollectionName { get; set; } = "venues";
    }
}
