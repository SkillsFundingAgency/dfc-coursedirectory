using Dfc.CourseDirectory.Web.ViewComponents.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueAddressSelectionConfirmation
{
    public class VenueAddressSelectionConfirmationModel
    {
        public string VenueName { get; set; }
        public AddressModel Address { get; set; }
    }
}
