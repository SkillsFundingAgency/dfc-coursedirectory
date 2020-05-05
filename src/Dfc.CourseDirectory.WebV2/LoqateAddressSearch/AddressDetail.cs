using System.Security.Policy;

namespace Dfc.CourseDirectory.WebV2.LoqateAddressSearch
{
    public class AddressDetail
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string Line5 { get; set; }
        public string PostTown { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string CountryName { get; set; }
    }
}
