using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearch
{
    public class PostCodeSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new PostCodeSearchModel();

            return View("~/ViewComponents/PostCodeSearch/Default.cshtml", model);
        }
    }
}