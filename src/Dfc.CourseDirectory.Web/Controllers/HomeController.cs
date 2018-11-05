using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILarsSearchService _larsSearchService;

        public HomeController(
            ILogger<HomeController> logger,
            ILarsSearchService larsSearchService)
        {
            _logger = logger;
            _larsSearchService = larsSearchService;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogMethodEnter();
            _logger.LogTrace("0");
            _logger.LogDebug("1");
            _logger.LogInformation("2");
            _logger.LogWarning("3");
            _logger.LogError("4");
            _logger.LogCritical("5");

            var criteria = new LarsSearchCriteria(
                "business Management",
                true,
                "NotionalNVQLevelv2 eq '4' and AwardOrgCode eq 'NONE'",
                new LarsSearchFacet[]
                {
                    LarsSearchFacet.NotionalNVQLevelv2,
                    LarsSearchFacet.AwardOrgCode
                });

            var actual = await _larsSearchService.SearchAsync(criteria);

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