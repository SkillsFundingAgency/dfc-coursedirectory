using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class Pagination : ViewComponent
    {
        private readonly IPaginationHelper _paginationHelper;

        public Pagination(IPaginationHelper paginationHelper)
        {
            _paginationHelper = paginationHelper;
        }

        public IViewComponentResult Invoke(
            string url,
            string pageParamName,
            int totalNoOfItems,
            int itemsPerPage,
            int noOfPagesToDisplay,
            bool isSliding)
        {
            var totalNoOfPages = _paginationHelper.GetTotalNoOfPages(totalNoOfItems, itemsPerPage);
            var currentPageNo = _paginationHelper.GetCurrentPageNo(url, pageParamName);
            var (startAt, endAt) = _paginationHelper.GetStartAtEndAt(totalNoOfPages, noOfPagesToDisplay, currentPageNo, isSliding);
            var items = new List<PaginationItemModel>();

            if (_paginationHelper.TryGetPrevious(url, pageParamName, currentPageNo, out PaginationItemModel previous))
            {
                items.Add(previous);
            }

            items.AddRange(_paginationHelper.GetPages(url, pageParamName, currentPageNo, startAt, endAt));

            if (_paginationHelper.TryGetNext(url, pageParamName, currentPageNo, totalNoOfPages, out PaginationItemModel next))
            {
                items.Add(next);
            }

            var actualModel = new PaginationModel(items);

            return View("~/ViewComponents/Pagination/Default.cshtml", actualModel);
        }
    }
}