using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Web.RequestModels;


namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IProviderSearchHelper
    {
        IProviderSearchCriteria GetProviderSearchCriteria(ProviderSearchRequestModel providerSearchRequestModel);
    }
}
