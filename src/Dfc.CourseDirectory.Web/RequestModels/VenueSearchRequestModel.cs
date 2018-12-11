using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class VenueSearchRequestModel
    {
        public string SearchTerm { get; set; }
        public string NewAddressId { get; set; }
        public VenueSearchRequestModel()
        {

        }
    }
}
