using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion
{
    public class ChooseRegion : ViewComponent
    {
        private readonly ICourseService _courseService;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public ChooseRegion(ICourseService courseService,IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(courseService, nameof(courseService));

            _courseService = courseService;
            _contextAccessor = contextAccessor;
        }

        public IViewComponentResult Invoke(ChooseRegionModel model)
        {
            if(model.Regions == null)
                model.Regions =_courseService.GetRegions();

            return View("~/ViewComponents/Courses/ChooseRegion/Default.cshtml", model);
        }
    }
}
