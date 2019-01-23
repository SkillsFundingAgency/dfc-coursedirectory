using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderController : Controller
    {
        private readonly ILogger<ProviderController> _logger;

        public ProviderController(ILogger<ProviderController> logger)
        {
            Throw.IfNull(logger, nameof(logger));

            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogMethodEnter();
            _logger.LogMethodExit();
            return View();
        }
    }
}