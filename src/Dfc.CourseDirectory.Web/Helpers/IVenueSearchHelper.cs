using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IVenueSearchHelper
    {
        VenueSearchCriteria GetVenueSearchCriteria(VenueSearchRequestModel venueSearchRequestModel);
        IEnumerable<VenueSearchResultItemModel> GetVenueSearchResultItemModels(IEnumerable<Venue> venueSearchResult);
    }
}
