using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult
{
    public class LarsSearchResultModel : ValueObject<LarsSearchResultModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public string SearchTerm { get; }
        public string OriginalSearchTerm { get; }
        public IEnumerable<LarsSearchResultItemModel> Items { get; }
        public IEnumerable<LarsSearchFilterModel> Filters { get; }
        public bool HasSelectedFilters => Filters.SelectMany(x => x.Items).Any(x => x.IsSelected);
        public int? TotalCount { get; }

        public LarsSearchResultModel()
        {
            Errors = new string[] { };
            Items = new LarsSearchResultItemModel[] { };
        }

        public LarsSearchResultModel(string error)
        {
            Errors = new string[] { error };
            Items = new LarsSearchResultItemModel[] { };
        }

        public LarsSearchResultModel(
            string searchTerm,
            IEnumerable<LarsSearchResultItemModel> items,
            IEnumerable<LarsSearchFilterModel> filters = null,
            int? totalCount = null)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));
            Throw.IfNull(items, nameof(items));

            Errors = new string[] { };
            SearchTerm = searchTerm;
            OriginalSearchTerm = searchTerm;
            Items = items;
            Filters = filters ?? new LarsSearchFilterModel[] { };
            TotalCount = totalCount;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return SearchTerm;
            yield return OriginalSearchTerm;
            yield return Items;
            yield return Filters;
            yield return HasSelectedFilters;
            yield return TotalCount;
        }
    }
}