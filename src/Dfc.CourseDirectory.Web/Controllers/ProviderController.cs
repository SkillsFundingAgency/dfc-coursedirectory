using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Dfc.CourseDirectory.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderController : Controller
    {
        private readonly ILogger<ProviderController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public ProviderController(ILogger<ProviderController> logger,
               IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(logger, nameof(logger));

            _logger = logger;
            _contextAccessor = contextAccessor;
            //Set this to 0 so that we display the Add Provider logic within the ProviderSearchResult ViewComponent
            _session.SetInt32("ProviderSearch", 0);
        }

        public IActionResult Index()
        {
            _logger.LogMethodEnter();
            _logger.LogMethodExit();
            return View();
        }
    }
}