using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult
{
    public class VenueSearchResultModel : IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public string SearchTerm { get; }
        public bool Updated { get; }
        public bool Deleted { get; }
        public VenueSearchResultItemModel NewItem { get; }
        public IEnumerable<VenueSearchResultItemModel> Items { get; }

        public VenueSearchResultModel()
        {
            Errors = new string[] { };
            Items = new VenueSearchResultItemModel[] { };
            NewItem = null;
        }

        public VenueSearchResultModel(string error)
        {
            Errors = new string[] { error };
            Items = new VenueSearchResultItemModel[] { };
            NewItem = null;
        }

        public VenueSearchResultModel(
            string searchTerm,
            IEnumerable<VenueSearchResultItemModel> items,
            VenueSearchResultItemModel newItem,
            bool updated)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));
            Throw.IfNull(items, nameof(items));

            Errors = new string[] { };
            SearchTerm = searchTerm;
            NewItem = newItem;
            Items = items;
            Updated = updated;
        }
        public VenueSearchResultModel(
           IEnumerable<VenueSearchResultItemModel> items,
           VenueSearchResultItemModel deletedItem)
        {
            Throw.IfNull(items, nameof(items));

            Errors = new string[] { };
            NewItem = deletedItem;
            Deleted = true;
            Items = items;
        }
    }
}
