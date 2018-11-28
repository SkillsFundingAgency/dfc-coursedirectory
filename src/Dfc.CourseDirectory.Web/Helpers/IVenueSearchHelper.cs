using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Venue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IVenueSearchHelper
    {
        IVenueSearchCriteria GetVenueSearchCriteria(VenueSearchRequestModel venueSearchRequestModel);
        IEnumerable<VenueSearchResultItemModel> GetVenueSearchResultItemModels(IEnumerable<VenueSearchResultItem> venueSearchResultItems);
    }
}
