﻿
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILarsSearchService _larsSearchService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public HomeController(
            ILogger<HomeController> logger,
            ILarsSearchService larsSearchService,
            ICourseService courseService,
            IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _larsSearchService = larsSearchService;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            //Set this todisplay the Search Provider fork of the ProviderSearchResult ViewComponent
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
            _session.SetString("Option","Home");
            ViewBag.StatusMessage = errmsg;

            if (_session.GetInt32("UKPRN") == null)
                return View();
            else
                return View(DashboardController.GetDashboardViewModel(_courseService, _session.GetInt32("UKPRN")));
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
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}