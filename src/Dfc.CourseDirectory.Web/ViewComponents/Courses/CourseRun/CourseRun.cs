using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseRun
{
    public class CourseRun : ViewComponent
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public CourseRun(
            IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(contextAccessor, nameof(contextAccessor));

            _contextAccessor = contextAccessor;
        }
        public IViewComponentResult Invoke(Dfc.CourseDirectory.Models.Models.Courses.CourseRun model)
        {
            var UKPRN = _session.GetInt32("UKPRN");
            CourseRunModel courseRunModel = new CourseRunModel()
            {
                courseRun = model
            };
            return View("~/ViewComponents/Courses/CourseRun/Default.cshtml", courseRunModel);
        }
    }
}
