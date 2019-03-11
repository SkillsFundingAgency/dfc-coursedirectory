using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILarsSearchService _larsSearchService;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public HomeController(
            ILogger<HomeController> logger,
            ILarsSearchService larsSearchService,
            IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _larsSearchService = larsSearchService;
            _contextAccessor = contextAccessor;
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
            return View();
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