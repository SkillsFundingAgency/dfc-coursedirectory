using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Venues;

[assembly: InternalsVisibleTo("Dfc.CourseDirectory.Services.Web.Tests")]

namespace Dfc.CourseDirectory.Web.Helpers
{

    public class VenueSearchHelper : IVenueSearchHelper
    {
        public IVenueSearchCriteria GetVenueSearchCriteria(
            VenueSearchRequestModel venueSearchRequestModel)
        {
            Throw.IfNull(venueSearchRequestModel, nameof(venueSearchRequestModel));

            var criteria = new VenueSearchCriteria(venueSearchRequestModel.SearchTerm, venueSearchRequestModel.NewAddressId);
            return criteria;
        }
        public IEnumerable<VenueSearchResultItemModel> GetVenueSearchResultItemModels(
           IEnumerable<Venue> venueSearchResult)
        {
            Throw.IfNull(venueSearchResult, nameof(venueSearchResult));

            var items = new List<VenueSearchResultItemModel>();

            foreach (var item in venueSearchResult)
            {
                items.Add(new VenueSearchResultItemModel(
                    item.VenueName,
                    item.Address1,
                    item.Address2,
                    item.Town,
                    item.County,
                    item.PostCode,item.ID));
            }

            return items;
        }
        internal static string FormatSearchTerm(string searchTerm)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));

            var split = searchTerm
                .Split(' ')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return split.Count() > 1 ? string.Join("*+", split) + "*" : $"{split[0]}*";
        }

    }

}
