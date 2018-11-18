using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface ILarsSearchHelper
    {
        ILarsSearchCriteria GetLarsSearchCriteria(LarsSearchRequestModel larsSearchRequestModel, int currentPageNo, int itemsPerPage, IEnumerable<LarsSearchFacet> facets);
    }
}