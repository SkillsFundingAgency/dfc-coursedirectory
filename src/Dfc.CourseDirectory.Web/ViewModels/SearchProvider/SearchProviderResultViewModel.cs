using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.ViewModels.SearchProvider
{
    public class SearchProviderResultViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Postcode { get; set; }

        public string Town { get; set; }

        public string Region { get; set; }

        public string UKPRN { get; set; }

        public ProviderStatus? Status { get; set; }

        public string ProviderStatus { get; set; }

        public string CourseDirectoryName { get; set; }

        public string TradingName { get; set; }

        public string ProviderAlias { get; set; }

        public static SearchProviderResultViewModel FromProvider(Core.Search.Models.Provider provider)
        {
            return new SearchProviderResultViewModel
            {
                Id = provider.Id,
                Name = provider.Name,
                Postcode = provider.Postcode,
                Town = provider.Town,
                Region = provider.Region,
                UKPRN = provider.UKPRN,
                Status = (ProviderStatus)provider.Status,
                ProviderStatus = provider.ProviderStatus,
                CourseDirectoryName = provider.CourseDirectoryName,
                TradingName = provider.TradingName,
                ProviderAlias = provider.ProviderAlias
            };
        }
    }
}
