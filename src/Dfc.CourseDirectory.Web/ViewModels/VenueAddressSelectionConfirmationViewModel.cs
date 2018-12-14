using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Web.ViewComponents.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class VenueAddressSelectionConfirmationViewModel
    {
        public string Error { get; set; }
        public string VenueName { get; set; }
        public AddressModel Address { get; set; }

        public string Id { get; set; }
    }
}
