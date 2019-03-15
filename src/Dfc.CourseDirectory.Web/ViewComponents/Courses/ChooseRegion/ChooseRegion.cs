using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Web.RequestModels;

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
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ChooseRegionModel model = new ChooseRegionModel();
            model.Regions = _courseService.GetRegions();

            return View("~/ViewComponents/Courses/ChooseRegion/Default.cshtml", model);

        }
    }
}
