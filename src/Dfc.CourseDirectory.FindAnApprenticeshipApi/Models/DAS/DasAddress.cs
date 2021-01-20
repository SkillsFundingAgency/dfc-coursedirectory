using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.DAS;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS
{
    public class DasAddress : IDasAddress
    {
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string County { get; set; }

        public double? Lat { get; set; }

        public double? Long { get; set; }

        public string Postcode { get; set; }

        public string Town { get; set; }

    }
}