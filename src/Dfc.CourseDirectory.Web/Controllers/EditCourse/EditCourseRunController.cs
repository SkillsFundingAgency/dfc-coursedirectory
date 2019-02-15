using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers.EditCourse
{
    public class EditCourseRunController : Controller
    {
        private readonly ILogger<EditCourseRunController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly ICourseService _courseService;

        //private ISession _session => _contextAccessor.HttpContext.Session;
        //private readonly IVenueSearchHelper _venueSearchHelper;
        //private readonly IVenueService _venueService;
        //private const string SessionAddCourseSection1 = "AddCourseSection1";
        //private const string SessionAddCourseSection2 = "AddCourseSection2";
        //private const string SessionVenues = "Venues";
        //private const string SessionRegions = "Regions";
        private readonly ICourseTextService _courseTextService;

        public EditCourseRunController(
            ILogger<EditCourseRunController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService, ICourseTextService courseTextService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseSearchSettings, nameof(courseSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(courseTextService, nameof(courseTextService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _courseTextService = courseTextService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(Guid? courseId, Guid courseRunId)
        {
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Index(EditCourseRunViewModel editCourseRunViewModel)
        {

            return View();
        }


    }
}