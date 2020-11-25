namespace Dfc.CourseDirectory.FindACourseApi.Features.CourseRunDetail
{
    public class SubRegionViewModel
    {
        public string SubRegionId { get; set; }
        public string Name { get; set; }
        public RegionViewModel ParentRegion { get; set; }
    }
}
