using Dfc.CourseDirectory.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.PostcodeLookup
{
    public class PostcodeLookup : ViewComponent
    {
        public IViewComponentResult Invoke(PostcodeLookupModel model)
        {
            var actualModel = model;

            return View("~/ViewComponents/PostcodeLookup/Default.cshtml", actualModel);
        }
    }
}
