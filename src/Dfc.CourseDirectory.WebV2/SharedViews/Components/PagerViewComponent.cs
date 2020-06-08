using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class PagerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int currentPage, int totalPages, Func<int, string> getPageUrl)
        {
            var pageNumbers = new[]
            {
                1,
                currentPage - 1,
                currentPage,
                currentPage + 1,
                totalPages
            }
            .Where(p => p > 0 && p <= totalPages)
            .OrderBy(p => p)
            .Distinct()
            .ToList();

            var viewModel = new PagerViewModel()
            {
                PageNumbers = pageNumbers,
                TotalPages = totalPages,
                GetPageUrl = getPageUrl,
                CurrentPage = currentPage
            };

            return View("~/SharedViews/Components/Pager.cshtml", viewModel);
        }
    }
}
