using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                 .Where(p => p > 0)
                 .OrderBy(p => p)
                 .Distinct();

            var viewModel = new PagerViewModel();

            viewModel.PageNumbers = pageNumbers;
            viewModel.TotalPages = totalPages;
            viewModel.GetPageUrl = getPageUrl;
            viewModel.CurrentPage = currentPage;
            return View("~/SharedViews/Components/Pager.cshtml", viewModel);
        }
    }
}
