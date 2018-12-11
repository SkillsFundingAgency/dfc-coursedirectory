using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Dfc.CourseDirectory.Services.Web.Tests")]

namespace Dfc.CourseDirectory.Web.Helpers
{

    public class VenueSearchHelper : IVenueSearchHelper
    {
        public IVenueSearchCriteria GetVenueSearchCriteria(
            VenueSearchRequestModel venueSearchRequestModel)
        {
            Throw.IfNull(venueSearchRequestModel, nameof(venueSearchRequestModel));

            //var criteria = new VenueSearchCriteria(FormatSearchTerm(venueSearchRequestModel.SearchTerm));
            var criteria = new VenueSearchCriteria(venueSearchRequestModel.SearchTerm, venueSearchRequestModel.NewAddressId);
            return criteria;
        }
        public IEnumerable<VenueSearchResultItemModel> GetVenueSearchResultItemModels(
           IEnumerable<VenueSearchResultItem> venueSearchResultItems)
        {
            Throw.IfNull(venueSearchResultItems, nameof(venueSearchResultItems));

            var items = new List<VenueSearchResultItemModel>();

            foreach (var item in venueSearchResultItems)
            {
                items.Add(new VenueSearchResultItemModel(
                    item.VenueName,
                    item.Address1,
                    item.Address2,
                    item.Town,
                    item.County,
                    item.PostCode));
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
