using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult;
using Dfc.CourseDirectory.Models.Models.Providers;

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
