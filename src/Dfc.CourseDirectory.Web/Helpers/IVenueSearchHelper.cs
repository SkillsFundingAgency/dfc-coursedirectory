using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IVenueSearchHelper
    {
        IVenueSearchCriteria GetVenueSearchCriteria(VenueSearchRequestModel venueSearchRequestModel);
        IEnumerable<VenueSearchResultItemModel> GetVenueSearchResultItemModels(IEnumerable<Venue> venueSearchResult);
    }
}
