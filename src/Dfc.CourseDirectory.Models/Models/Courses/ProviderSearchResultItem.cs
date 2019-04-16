
using System;


namespace Dfc.CourseDirectory.Models.Models.Courses
{
    // TODO - Provider search is in the course service for now, needs moving!
    public class ProviderSearchResultItem
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        public string Postcode { get; set; }
        public string Town { get; set; }
        public string Region { get; set; }
        public string ProviderId { get; set; }
    }
}
