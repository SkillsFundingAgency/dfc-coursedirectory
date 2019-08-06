using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

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

        private ISession _session => _contextAccessor.HttpContext.Session;

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

        public IActionResult Check()
        {
            int UKPRN = 0;
            if (User.Identity.IsAuthenticated)
            {
                if (_session.GetInt32("UKPRN") == null)
                {
                    Claim UKPRNClaim = User.Claims.Where(x => x.Type == "UKPRN").SingleOrDefault();
                    if (!String.IsNullOrEmpty(UKPRNClaim.Value))
                    {
                        _session.SetInt32("UKPRN", Int32.Parse(UKPRNClaim.Value));
                    }
                }

                if (_session.GetInt32("UKPRN").HasValue)
                {
                    UKPRN = _session.GetInt32("UKPRN").Value;
                }

                var authorised = _authorizationService.AuthorizeAsync(User, "ElevatedUserRole").Result;

                if (authorised.Succeeded)
                {
                    // Making sure this change goes through...
                    return RedirectToAction("Index", "SearchProvider");
                }

                IEnumerable<Course> courses = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                            .Result
                                            .Value
                                            .Value
                                            .SelectMany(o => o.Value)
                                            .SelectMany(i => i.Value);

                IEnumerable<CourseRun> migrationPendingCourses = courses.SelectMany(c => c.CourseRuns).Where(x => x.RecordStatus == RecordStatus.MigrationPending);

                if (migrationPendingCourses.Count() > 0)
                {
                    return RedirectToAction("Report", "Migration");
                }

                // Making sure this change goes through...
                return Redirect("/");
            }
            else
            {
                return View("../Provider/Landing");
            }
        }

        public IActionResult Index(string errmsg)
        {
            _session.SetInt32("ProviderSearch", 1);
            _logger.LogMethodEnter();
            _logger.LogTrace("0");
            _logger.LogDebug("1");
            _logger.LogInformation("2");
            _logger.LogWarning("3");
            _logger.LogError("4");
            _logger.LogCritical("5");
            _session.SetString("Option", "Home");
            ViewBag.StatusMessage = errmsg;

            if (User.Identity.IsAuthenticated)
            {
                if (_session.GetInt32("UKPRN") == null)
                {
                    Claim UKPRN = User.Claims.Where(x => x.Type == "UKPRN").SingleOrDefault();
                    if (!String.IsNullOrEmpty(UKPRN.Value))
                    {
                        _session.SetInt32("UKPRN", Int32.Parse(UKPRN.Value));
                    }
                }

                var authorised = _authorizationService.AuthorizeAsync(User, "ElevatedUserRole").Result;

                if (authorised.Succeeded)
                {
                    // Making sure this change goes through...
                    return RedirectToAction("Index", "SearchProvider");
                }

                return View("../Provider/Dashboard");
            }
            else
            {
                return View("../Provider/Landing");
            }
        }

        public IActionResult IndexSuccess(DashboardViewModel vm)
        {
            if (_session.GetInt32("UKPRN") == null)
            {
                return View();
            }
            else
            {
                if (vm == null)
                {
                    vm = DashboardController.GetDashboardViewModel(_courseService, _blobStorageService, _session.GetInt32("UKPRN"), "");
                }
                if (vm.PendingCourseCount > 0)
                {
                    _session.SetString("PendingCourses", "true");
                }
                else
                {
                    _session.SetString("PendingCourses", "false");
                }

                return View("Index", vm);
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View("../Home/Privacy");
        }

        public IActionResult Help()
        {
            return View("../Home/Help");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = exceptionHandlerPathFeature.Error != null ? exceptionHandlerPathFeature.Error.Message : "There has been an error, please contact support",
                ErrorPath = exceptionHandlerPathFeature.Path != null ? exceptionHandlerPathFeature.Path : string.Empty
            });
        }
    }
}