using System.Collections.Generic;
using Dfc.CourseDirectory.Web.ViewComponents.Pagination;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IPaginationHelper
    {
        int GetCurrentPageNo(string url, string pageParamName, int defaultPageNo = 1);
        IEnumerable<PaginationItemModel> GetPages(string url, string pageParamName, int currentPageNo, int startAt, int endAt);
        int GetTotalNoOfPages(int totalNoOfItems, int itemsPerPage);
        bool TryGetNext(string url, string pageParamName, int currentPageNo, int totalNoOfPages, out PaginationItemModel paginationItemModel);
        bool TryGetPrevious(string url, string pageParamName, int currentPageNo, out PaginationItemModel paginationItemModel);
        (int, int) GetStartAtEndAt(int totalNoOfPages, int noOfPagesToDisplay, int currentPageNo, bool isSliding);
    }
}