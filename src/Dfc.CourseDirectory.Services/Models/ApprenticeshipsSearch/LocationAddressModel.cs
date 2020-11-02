namespace Dfc.CourseDirectory.Services.Models.ApprenticeshipsSearch
{
    public class LocationAddressModel
    {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string town { get; set; }
        public string county { get; set; }
        public string postcode { get; set; }
        public double? lat { get; set; }
        public double? @long { get; set; }
    }
}
