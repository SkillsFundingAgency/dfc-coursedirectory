
using System;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Settings
{
    public class SearchServiceSettings : ISearchServiceSettings
    {
        public string SearchService { get; set; }
        public string ApiUrl { get; set; }
        public string ProviderApiUrl { get; set; }
        public string LARSApiUrl { get; set; }
        public string ONSPDApiUrl { get; set; }
        public string ApiVersion { get; set; }
        public string QueryKey { get; set; }
        public string AdminKey { get; set; }
        public string Index { get; set; }
        public string onspdIndex { get; set; }
        public int DefaultTop { get; set; }
        public int MaxTop { get; set; }
        public string RegionBoostScoringProfile { get; set; }
        public int ThresholdVenueCount { get; set; }
    }
}
