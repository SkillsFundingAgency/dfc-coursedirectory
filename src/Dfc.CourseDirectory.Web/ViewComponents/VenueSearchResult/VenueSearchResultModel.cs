using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult
{
    public class VenueSearchResultModel : ValueObject<VenueSearchResultModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public string SearchTerm { get; }
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
            VenueSearchResultItemModel newItem)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));
            Throw.IfNull(items, nameof(items));

            Errors = new string[] { };
            SearchTerm = searchTerm;
            NewItem = newItem;
            Items = items;
            NewItem = newItem;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return SearchTerm;
            yield return Items;
            yield return NewItem;
        }
    }

}
