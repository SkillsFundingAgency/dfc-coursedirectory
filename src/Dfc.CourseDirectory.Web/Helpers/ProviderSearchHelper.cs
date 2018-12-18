using System.Linq;
using System.Runtime.CompilerServices;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.ProviderService;

[assembly: InternalsVisibleTo("Dfc.CourseDirectory.Services.Web.Tests")]


namespace Dfc.CourseDirectory.Web.Helpers
{
    public class ProviderSearchHelper : IProviderSearchHelper
    {
        public IProviderSearchCriteria GetProviderSearchCriteria(
                ProviderSearchRequestModel providerSearchRequestModel)
        {
            Throw.IfNull(providerSearchRequestModel, nameof(providerSearchRequestModel));
            var criteria = new ProviderSearchCriteria(providerSearchRequestModel.SearchTerm);
            return criteria;
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
