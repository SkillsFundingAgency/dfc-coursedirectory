using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenueController : Controller
    {
        private readonly ILogger<VenueController> _logger;

        public VenueController(
            ILogger<VenueController> logger)
        {
            Throw.IfNull(logger, nameof(logger));

            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

       
    }
}