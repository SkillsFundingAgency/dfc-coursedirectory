namespace Dfc.CourseDirectory.Models.Models
{
    public class SubRegionItemModel
    {
        public string Id { get; set; }
        public string SubRegionName { get; set; }
        public bool? Checked { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
