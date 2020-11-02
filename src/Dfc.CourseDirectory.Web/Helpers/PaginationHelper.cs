using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Web.ViewComponents.Pagination;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class PaginationHelper : IPaginationHelper
    {
        public int GetCurrentPageNo(
            string url,
            string pageParamName,
            int defaultPageNo = 1)
        {
            Throw.IfNullOrWhiteSpace(url, nameof(url));
            Throw.IfNullOrWhiteSpace(pageParamName, nameof(pageParamName));
            Throw.IfLessThan(1, defaultPageNo, nameof(defaultPageNo));

            var ub = new UriBuilder(url);
            var query = QueryHelpers.ParseQuery(ub.Query);
            var currentPage = defaultPageNo;

            if (query.ContainsKey(pageParamName))
            {
                var queryValue = query.GetValueOrDefault(pageParamName);

                if (int.TryParse(queryValue, out int parsed) && parsed > defaultPageNo)
                {
                    currentPage = parsed;
                }
            }

            return currentPage;
        }

        public IEnumerable<PaginationItemModel> GetPages(
            string url,
            string pageParamName,
            int currentPageNo,
            int startAt,
            int endAt)
        {
            Throw.IfNullOrWhiteSpace(url, nameof(url));
            Throw.IfNullOrWhiteSpace(pageParamName, nameof(pageParamName));
            Throw.IfLessThan(1, currentPageNo, nameof(currentPageNo));
            Throw.IfLessThan(1, startAt, nameof(startAt));
            Throw.IfLessThan(1, endAt, nameof(endAt));

            var pages = new List<PaginationItemModel>();

            for (var i = startAt; i <= endAt; i++)
            {
                var urlWithPageNo = GetUrlWithPageNo(url, pageParamName, i);
                pages.Add(new PaginationItemModel(
                    urlWithPageNo,
                    i.ToString(),
                    GetPageAriaLabel(currentPageNo, i),
                    i == currentPageNo));
            }

            return pages;
        }

        public (int, int) GetStartAtEndAt(
            int totalNoOfPages,
            int noOfPagesToDisplay,
            int currentPageNo,
            bool isSliding)
        {
            Throw.IfLessThan(1, totalNoOfPages, nameof(totalNoOfPages));
            Throw.IfLessThan(1, noOfPagesToDisplay, nameof(noOfPagesToDisplay));
            Throw.IfLessThan(1, currentPageNo, nameof(currentPageNo));

            var startAt = 1;
            var endAt = totalNoOfPages < noOfPagesToDisplay
                ? totalNoOfPages
                : noOfPagesToDisplay;

            if (isSliding && totalNoOfPages >= noOfPagesToDisplay)
            {
                var ceiling = (int)Math.Ceiling((decimal)noOfPagesToDisplay / 2);

                if (ceiling <= currentPageNo)
                {
                    startAt = (currentPageNo + 1 - ceiling) >= (totalNoOfPages - noOfPagesToDisplay)
                        ? totalNoOfPages - noOfPagesToDisplay + 1
                        : currentPageNo + 1 - ceiling;
                    endAt = (noOfPagesToDisplay - 1 + startAt) < totalNoOfPages
                        ? noOfPagesToDisplay - 1 + startAt
                        : totalNoOfPages;
                }
            }

            return (startAt, endAt);
        }

        public int GetTotalNoOfPages(
                                    int totalNoOfItems,
            int itemsPerPage)
        {
            if (totalNoOfItems <= itemsPerPage || totalNoOfItems == 0 || itemsPerPage == 0) return 1;

            return (int)Math.Ceiling((decimal)totalNoOfItems / itemsPerPage);
        }

        public bool TryGetNext(
            string url,
            string pageParamName,
            int currentPageNo,
            int totalNoOfPages,
            out PaginationItemModel paginationItemModel)
        {
            Throw.IfNullOrWhiteSpace(url, nameof(url));
            Throw.IfNullOrWhiteSpace(pageParamName, nameof(pageParamName));
            Throw.IfLessThan(1, currentPageNo, nameof(currentPageNo));
            Throw.IfLessThan(1, totalNoOfPages, nameof(totalNoOfPages));

            paginationItemModel = null;
            if (currentPageNo >= totalNoOfPages) return false;

            var pageNo = currentPageNo + 1;
            var ariaLabel = $"Goto the next page, page {pageNo}";
            var urlWithPageNo = GetUrlWithPageNo(url, pageParamName, pageNo);

            paginationItemModel = new PaginationItemModel(
                urlWithPageNo,
                "Next",
                ariaLabel);

            return true;
        }

        public bool TryGetPrevious(
                    string url,
            string pageParamName,
            int currentPageNo,
            out PaginationItemModel paginationItemModel)
        {
            Throw.IfNullOrWhiteSpace(url, nameof(url));
            Throw.IfNullOrWhiteSpace(pageParamName, nameof(pageParamName));
            Throw.IfLessThan(1, currentPageNo, nameof(currentPageNo));

            paginationItemModel = null;
            if (currentPageNo == 1) return false;

            var pageNo = currentPageNo - 1;
            var ariaLabel = $"Goto the previous page, page {pageNo}";
            var urlWithPageNo = GetUrlWithPageNo(url, pageParamName, pageNo);

            paginationItemModel = new PaginationItemModel(
                urlWithPageNo,
                "Previous",
                ariaLabel);

            return true;
        }
        internal static string GetPageAriaLabel(
            int currentPageNo,
            int pageNo)
        {
            Throw.IfLessThan(1, currentPageNo, nameof(currentPageNo));
            Throw.IfLessThan(1, pageNo, nameof(pageNo));

            if (pageNo == currentPageNo)
            {
                return $"Current page, page {pageNo}";
            }

            return $"Goto page {pageNo}";
        }

        internal static string GetUrlWithPageNo(
            string url,
            string pageParamName,
            int pageNo)
        {
            Throw.IfNullOrWhiteSpace(url, nameof(url));
            Throw.IfNullOrWhiteSpace(pageParamName, nameof(pageParamName));
            Throw.IfLessThan(1, pageNo, nameof(pageNo));

            var ub = new UriBuilder(url);
            var query = QueryHelpers.ParseQuery(ub.Query);
            var items = query
                .SelectMany(
                    x => x.Value,
                    (col, value) => new KeyValuePair<string, string>(col.Key, value))
                .ToList();

            items.RemoveAll(x => x.Key == pageParamName);

            var qb = new QueryBuilder(items)
            {
                { pageParamName, pageNo.ToString() }
            };

            ub.Query = qb.ToQueryString().ToString();

            return ub.ToString();
        }
    }
}