using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Components.LarsSearch
{
    public class LarsSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new LarsSearchModel();

            return View("~/Components/LarsSearch/Default.cshtml", model);
        }
    }
}
