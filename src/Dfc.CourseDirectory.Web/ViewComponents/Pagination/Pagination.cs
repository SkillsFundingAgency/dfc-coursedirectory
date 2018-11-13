using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class Pagination : ViewComponent
    {
        public IViewComponentResult Invoke(
            string url,
            string paramName,
            int currentPage,
            int totalItems,
            int itemsPerPage,
            int pagesToDisplay,
            bool isSliding)
        {
            var actualModel = new PaginationModel();

            return View("~/ViewComponents/Pagination/Default.cshtml", actualModel);
        }

        internal int GetTotalNoOfPages(
            int totalItems, 
            int itemsPerPage)
        {
            if (totalItems <= itemsPerPage || totalItems == 0 || itemsPerPage == 0) return 1;

            return (int)Math.Ceiling((decimal)totalItems / itemsPerPage);
        }

        internal bool TryGetPrevious(
            string url, 
            string paramName, 
            int currentPage, 
            out PaginationItemModel paginationItemModel)
        {
            paginationItemModel = null;
            if (currentPage <= 1) return false;

            var pageNo = currentPage - 1;
            var ariaLabel = $"Goto the previous page, page {pageNo}";
            var urlWithPageNo = GetUrlWithPageNo(url, paramName, pageNo);

            paginationItemModel = new PaginationItemModel(
                urlWithPageNo,
                "Previous",
                ariaLabel);

            return true;
        }

        internal IEnumerable<PaginationItemModel> GetPages(
            string url, 
            string paramName, 
            PageBoundary pageBoundary)
        {
            var pages = new List<PaginationItemModel>();

            for (var i = pageBoundary.StartAt; i <= pageBoundary.PagesToDisplay; i++)
            {
                var urlWithPageNo = GetUrlWithPageNo(url, paramName, i);
            }

            return pages;
        }

        internal bool TryGetNext(
            string url, 
            string paramName, 
            int currentPage, 
            int totalNoOfPages,
            out PaginationItemModel paginationItemModel)
        {
            paginationItemModel = null;
            if (currentPage >= totalNoOfPages) return false;

            var pageNo = currentPage + 1;
            var ariaLabel = $"Goto the next page, page {pageNo}";
            var urlWithPageNo = GetUrlWithPageNo(url, paramName, pageNo);

            paginationItemModel = new PaginationItemModel(
                urlWithPageNo,
                "Next",
                ariaLabel);

            return true;
        }

        internal string GetUrlWithPageNo(
            string url, 
            string paramName, 
            int pageNo)
        {
            var ub = new UriBuilder(url);
            var query = QueryHelpers.ParseQuery(ub.Query);
            var items = query
                .SelectMany(
                    x => x.Value,
                    (col, value) => new KeyValuePair<string, string>(col.Key, value))
                .ToList();

            items.RemoveAll(x => x.Key == paramName);

            var qb = new QueryBuilder(items)
            {
                { paramName, pageNo.ToString() }
            };

            ub.Query = qb.ToQueryString().ToString();

            return ub.ToString();
        }
    }


}
