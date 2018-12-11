using Dfc.CourseDirectory.Web.ViewComponents.Shared;

namespace Dfc.CourseDirectory.Web.ViewComponents.EditVenueAddress
{
    public class EditVenueAddressModel
    {
        public string VenueName { get; set; }
        public string PostcodeId { get; set; }
        public AddressModel Address { get; set; }
    }
}