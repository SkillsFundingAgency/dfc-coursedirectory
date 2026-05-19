namespace Dfc.CourseDirectory.WebV2.ViewComponents.RequestModels
{
    public class ProviderCourseSearchState
    {
        public string Keyword { get; set; }
        public string[] LevelFilter { get; set; } = System.Array.Empty<string>();
        public string[] DeliveryModeFilter { get; set; } = System.Array.Empty<string>();
        public string[] VenueFilter { get; set; } = System.Array.Empty<string>();
        public string[] AttendancePatternFilter { get; set; } = System.Array.Empty<string>();
        public string[] RegionFilter { get; set; } = System.Array.Empty<string>();
        public bool NonLarsCourse { get; set; }
    }
}
