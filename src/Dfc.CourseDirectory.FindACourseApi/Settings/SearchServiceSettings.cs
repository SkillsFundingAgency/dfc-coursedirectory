using Dfc.CourseDirectory.FindACourseApi.Interfaces;

namespace Dfc.CourseDirectory.FindACourseApi.Settings
{
    public class SearchServiceSettings : ISearchServiceSettings
    {
        public string SearchService { get; set; }
        public string ApiUrl { get; set; }
        public string ApiVersion { get; }
        public string QueryKey { get; set; }
        public string Index { get; set; }
        public string onspdIndex { get; set; }
        public int DefaultTop { get; set; }
        public int MaxTop { get; set; }
        public string RegionBoostScoringProfile { get; set; }
    }
}
