
using System;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class AzureSearchVenueModel
    {
        public Guid? id { get; set; }
        public string VENUE_NAME { get; set; }
        public string ADDRESS_1 { get; set; }
        public string ADDRESS_2 { get; set; }
        public string TOWN { get; set; }
        public string COUNTY { get; set; }
        public string POSTCODE { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
