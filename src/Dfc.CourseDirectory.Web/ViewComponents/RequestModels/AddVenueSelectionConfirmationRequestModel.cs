using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class AddVenueSelectionConfirmationRequestModel
    {
        public string Id { get; set; }
        public string VenueName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string TownOrCity { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
    }
}
