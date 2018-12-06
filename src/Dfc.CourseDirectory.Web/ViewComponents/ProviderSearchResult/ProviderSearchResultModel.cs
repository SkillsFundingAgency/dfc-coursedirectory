using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult
{
    public class ProviderSearchResultModel : ValueObject<ProviderSearchResultModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public string SearchTerm { get; }
        public IEnumerable<Provider> Items { get; set; }

        public ProviderSearchResultModel()
        {
            Errors = new string[] { };
            Items = new Provider[] { };
        }

        public ProviderSearchResultModel(string error)
        {
            Errors = new string[] { error };
            Items = new Provider[] { };
        }

        public ProviderSearchResultModel(
            string searchTerm,
            IEnumerable<Provider> items)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));
            Throw.IfNull(items, nameof(items));

            Errors = new string[] { };
            SearchTerm = searchTerm;
            Items = items;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return SearchTerm;
            yield return Items;
        }
    }
}
