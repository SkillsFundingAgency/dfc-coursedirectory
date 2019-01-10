using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseRun
{
    public class CourseRun : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(CourseRunModel model)
        {
            return View("~/ViewComponents/Courses/CourseRun/Default.cshtml", model);
        }
    }
}
