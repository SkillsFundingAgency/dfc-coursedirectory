using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;

namespace Dfc.CourseDirectory.Web.ViewComponents.Venue
{
    public class VenueListModel
    {
        public IEnumerable<VenueItemModel> Venues{ get; set; }
    }
}