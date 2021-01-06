using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Settings;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Settings
{
    public class CosmosDbCollectionSettings : ICosmosDbCollectionSettings
    {
        public string StandardsCollectionId { get; set; }
        public string FrameworksCollectionId { get; set; }
        public string ApprenticeshipCollectionId { get; set; }
        public string ProgTypesCollectionId { get; set; }
    }
}
