
using System.Linq;
using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;


namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderAzureSearchResult
{
    public class ProviderAzureSearchResultModel : ValueObject<ProviderAzureSearchResultModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; set; }
        public string SearchTerm { get; }
        public IEnumerable<ProviderAzureSearchResultItem> Items { get; set; }

        public ProviderAzureSearchResultModel()
        {
            Errors = new string[] { };
            Items = new ProviderAzureSearchResultItem[] { };
        }

        public ProviderAzureSearchResultModel(string error)
        {
            Errors = new string[] { error };
            Items = new ProviderAzureSearchResultItem[] { };
        }

        public ProviderAzureSearchResultModel(
            string searchTerm,
            IEnumerable<ProviderAzureSearchResultItem> items)
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
