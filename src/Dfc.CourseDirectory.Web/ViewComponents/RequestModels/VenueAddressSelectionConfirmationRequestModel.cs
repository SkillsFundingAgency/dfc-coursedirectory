using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Shared;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class VenueAddressSelectionConfirmationRequestModel
    {
        public string VenueName { get; set; }
        public string PostcodeId { get; set; }
        public string Id { get; set; }

    }
}
