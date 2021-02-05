using System;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevelDetail
{
    public class TLevelLocationViewModel
    {
        public Guid TLevelLocationId { get; set; }
        public string VenueName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
