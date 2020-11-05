using System;
using Dfc.CourseDirectory.Services.CourseService;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion
{
    public class ChooseRegion : ViewComponent
    {
        private readonly ICourseService _courseService;

        public ChooseRegion(ICourseService courseService)
        {
            if (courseService == null)
            {
                throw new ArgumentNullException(nameof(courseService));
            }

            _courseService = courseService;
        }

        public IViewComponentResult Invoke(ChooseRegionModel model)
        {
            if(model.Regions == null)
                model.Regions =_courseService.GetRegions();

            return View("~/ViewComponents/Courses/ChooseRegion/Default.cshtml", model);
        }
    }
}
