using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class LarsSearchController : Controller
    {
        public IActionResult Index(string searchTerm)
        {
            var model = new Components.LarsSearchResult.LarsSearchResultModel
            {
                SearchTerm = searchTerm
            };

            return ViewComponent("LarsSearchResult", model);
        }
    }
}