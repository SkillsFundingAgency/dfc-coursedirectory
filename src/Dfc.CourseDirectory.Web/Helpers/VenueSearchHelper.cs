using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;

[assembly: InternalsVisibleTo("Dfc.CourseDirectory.Services.Web.Tests")]

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class VenueSearchHelper : IVenueSearchHelper
    {
        public VenueSearchCriteria GetVenueSearchCriteria(
            VenueSearchRequestModel venueSearchRequestModel)
        {
            if (venueSearchRequestModel == null)
            {
                throw new ArgumentNullException(nameof(venueSearchRequestModel));
            }

            var criteria = new VenueSearchCriteria(venueSearchRequestModel.SearchTerm, venueSearchRequestModel.NewAddressId);
            return criteria;
        }
        public IEnumerable<VenueSearchResultItemModel> GetVenueSearchResultItemModels(
           IEnumerable<Venue> venueSearchResult)
        {
            if (venueSearchResult == null)
            {
                throw new ArgumentNullException(nameof(venueSearchResult));
            }

            var items = new List<VenueSearchResultItemModel>();

            foreach (var item in venueSearchResult.Where(x => x.Status == VenueStatus.Live))
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
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentNullException($"{nameof(searchTerm)} cannot be null or empty or whitespace.", nameof(searchTerm));
            }

            var split = searchTerm
                .Split(' ')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return split.Count() > 1 ? string.Join("*+", split) + "*" : $"{split[0]}*";
        }

    }

}
