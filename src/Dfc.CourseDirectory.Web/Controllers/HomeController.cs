using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILarsSearchService _larsSearchService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IAuthorizationService _authorizationService;

        public HomeController(
            ILogger<HomeController> logger,
            ILarsSearchService larsSearchService,
            ICourseService courseService,
            IBlobStorageService blobStorageService,
            IHttpContextAccessor contextAccessor,
            IAuthorizationService authorizationService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(blobStorageService, nameof(blobStorageService));
            Throw.IfNull(authorizationService, nameof(authorizationService));

            _logger = logger;
            _larsSearchService = larsSearchService;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _blobStorageService = blobStorageService;
            _authorizationService = authorizationService;
        }

        public IActionResult Index(string errmsg, [FromServices] IFeatureFlagProvider featureFlagProvider)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("../Provider/Landing");
            }

            HttpContext.Session.SetInt32("ProviderSearch", 1);
            HttpContext.Session.SetString("Option", "Home");
            ViewBag.StatusMessage = errmsg;

            ViewBag.HideHeaderBackLink = true;

            var admin = _authorizationService.AuthorizeAsync(User, "ElevatedUserRole").Result;

            if (admin.Succeeded)
            {
                return RedirectToAction("Dashboard", "HelpdeskDashboard");
            }
            else
            {
                var ukprn = HttpContext.Session.GetInt32("UKPRN");

                IEnumerable<Course> courses = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(ukprn))
                                            .Result
                                            .Value
                                            .Value
                                            .SelectMany(o => o.Value)
                                            .SelectMany(i => i.Value);

                IEnumerable<CourseRun> migrationPendingCourses = courses.SelectMany(c => c.CourseRuns).Where(x => x.RecordStatus == RecordStatus.MigrationPending);

                return View("../Provider/Dashboard");
            }
        }

        [Authorize]
        public IActionResult IndexSuccess(DashboardViewModel vm)
        {
            if (HttpContext.Session.GetInt32("UKPRN") == null)
            {
                return View();
            }
            else
            {
                if (vm == null)
                {
                    vm = DashboardController.GetDashboardViewModel(_courseService, _blobStorageService, HttpContext.Session.GetInt32("UKPRN"), "");
                }
                if (vm.PendingCourseCount > 0)
                {
                    HttpContext.Session.SetString("PendingCourses", "true");
                }
                else
                {
                    HttpContext.Session.SetString("PendingCourses", "false");
                }

                return View("Index", vm);
            }
        }

        [AllowDeactivatedProvider]
        public IActionResult Privacy()
        {
            return View("../Home/Privacy");
        }

        [AllowDeactivatedProvider]
        public IActionResult Help()
        {
            return View("../Home/Help");
        }
    }
}