namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb
{
    public class Configuration
    {
        public string DatabaseId { get; set; } = "providerportal";
        public string ApprenticeshipCollectionName { get; set; } = "apprenticeship";
        public string CoursesCollectionName { get; set; } = "courses";
        public string ProviderCollectionName { get; set; } = "ukrlp";
        public string FrameworksCollectionName { get; set; } = "frameworks";
        public string StandardsCollectionName { get; set; } = "standards";
        public string VenuesCollectionName { get; set; } = "venues";
        public string ProgTypesCollectionName { get; set; } = "progtypes";
        public string StandardSectorCodesCollectionName { get; set; } = "standardsectorcodes";
        public string SectorSubjectAreaTier1sCollectionName { get; set; } = "sectorsubjectareatier1s";
        public string SectorSubjectAreaTier2sCollectionName { get; set; } = "sectorsubjectareatier2s";
        public string FeChoicesCollectionName { get; set; } = "fechoices";
    }
}
