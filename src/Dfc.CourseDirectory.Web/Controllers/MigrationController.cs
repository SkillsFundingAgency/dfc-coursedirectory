using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Web.ViewModels.Migration;


namespace Dfc.CourseDirectory.Web.Controllers
{

    public class MigrationController : Controller
    {
        private readonly ILogger<MigrationController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBulkUploadService _bulkUploadService;

        private IHostingEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public MigrationController(
                ILogger<MigrationController> logger,
                IHttpContextAccessor contextAccessor,
                IBulkUploadService bulkUploadService,
                IHostingEnvironment env)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(bulkUploadService, nameof(bulkUploadService));
            Throw.IfNull(env, nameof(env));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _bulkUploadService = bulkUploadService;
            _env = env;
        }


        [Authorize]
        public IActionResult Index()
        {
            _session.SetString("Option", "Migration");
            return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Options()
        {
            return View("../Migration/Options/Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Options(OptionsViewModel viewModel )
        {
            switch (viewModel.MigrationOption)
            {
                case MigrationOptions.CheckCourses:
                    return RedirectToAction("Index", "ProviderCourses");
                case MigrationOptions.StartAgain:
                    return RedirectToAction("Index", "BulkUpload");
                default:
                  return RedirectToAction("Options", "Migration");
            }
        }
    }
}