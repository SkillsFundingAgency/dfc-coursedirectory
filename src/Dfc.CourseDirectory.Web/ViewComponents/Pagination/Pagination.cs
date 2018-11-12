using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class Pagination : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationModel model)
        {
            var actualModel = model ?? new PaginationModel();

            return View("~/ViewComponents/Pagination/Default.cshtml", actualModel);
        }
    }
}
