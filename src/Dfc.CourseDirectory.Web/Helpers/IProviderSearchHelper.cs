using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IProviderSearchHelper
    {
        IProviderSearchCriteria GetProviderSearchCriteria(ProviderSearchRequestModel providerSearchRequestModel);
        //IEnumerable<ProviderSearchResultItemModel> GetProviderSearchResultItemModels(IEnumerable<ProviderSearchResultItem> providerSearchResultItems);
        //IEnumerable<Provider> GetProviderSearchResultItemModels(IEnumerable<Provider> providerSearchResultItems);
    }
}
