using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.GdsPagination
{
    public class GdsPagination : ViewComponent
    {
        public IViewComponentResult Invoke(GdsPaginationModel pagination, string pageUrlFormat)
        {
            var viewModel = new GdsPaginationViewModel
            {
                Pagination = pagination,
                PageUrlFormat = pageUrlFormat
            };

            return View("~/ViewComponents/GdsPagination/Default.cshtml", viewModel);
        }
    }
}
