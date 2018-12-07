using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.PostcodeLookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IPostcodeLookupHelper
    {
        IPostCodeSearchCriteria GetPostCodeSearchCriteria(PostcodeLookupRequestModel postcodeLookupRequestModel);
        IEnumerable<PostcodeLookupItemModel> GetPostCodeSearchResultItemModels(IEnumerable<PostCodeSearchResultItem> postCodeSearchResultItems);
    }
}
