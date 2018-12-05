using System.Collections.Generic;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearchResult;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IPostCodeSearchHelper
    {
        IEnumerable<PostCodeSearchResultItemModel> GetPostCodeSearchResultItemModels(IEnumerable<PostCodeSearchResultItem> postCodeSearchResultItems);
    }
}