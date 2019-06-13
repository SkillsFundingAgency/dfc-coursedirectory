using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.Helpers.Attributes;
using Dfc.CourseDirectory.Web.ViewModels.Migration;
using Microsoft.ApplicationInsights;
using NuGet.Frameworks;


namespace Dfc.CourseDirectory.Web.Controllers
{
    [SelectedProviderNeeded]
    public class MigrationController : Controller
    {
        private readonly ILogger<MigrationController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBulkUploadService _bulkUploadService;
        private readonly IUserHelper _userHelper;
        private readonly ICourseService _courseService;

        private IHostingEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public MigrationController(
            ILogger<MigrationController> logger,
            IHttpContextAccessor contextAccessor,
            IBulkUploadService bulkUploadService,
            IHostingEnvironment env, IUserHelper userHelper, ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(bulkUploadService, nameof(bulkUploadService));
            Throw.IfNull(env, nameof(env));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _bulkUploadService = bulkUploadService;
            _env = env;
            _userHelper = userHelper;
            _courseService = courseService;
        }


        [Authorize]
        public IActionResult Index()
        {
            _session.SetString("Option", "Migration");
            return RedirectToAction("Index", "PublishCourses", new {publishMode = PublishMode.Migration});
        }

        [Authorize]
        [HttpGet]
        public IActionResult Options()
        {
            return View("../Migration/Options/Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Options(OptionsViewModel viewModel)
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

        [Authorize]
        [HttpGet]
        public IActionResult Errors(int? liveCourses, int? errors)
        {
            var model = new ErrorsViewModel
            {
                LiveCourses = liveCourses,
                Errors = errors
            };

            return View("../Migration/Errors/Index", model);
        }

        [Authorize]
        [HttpPost]
        
        public async Task<IActionResult> Errors(ErrorsViewModel model)
        {
            switch (model.MigrationErrors)
            {
                case MigrationErrors.FixErrors:
                    return RedirectToAction("Index", "PublishCourses", new {publishMode = PublishMode.Migration});
                case MigrationErrors.DeleteCourses:
                    return RedirectToAction("Index", "Home");
                case MigrationErrors.StartAgain:
                    return RedirectToAction("Index", "BulkUpload");
                default:
                    return RedirectToAction("Errors");
            }
        }

        [Authorize("Admin")]
        public async Task<IActionResult> Delete()
        {
            var ukprn = _session.GetInt32("UKPRN").Value;
            var courseCounts =  await _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(ukprn));
            var courseErrors = courseCounts.Value.SingleOrDefault(x => x.Status == (int) RecordStatus.MigrationPending);
            var model = new DeleteViewModel {CourseErrors = courseErrors?.Count};

            return View("../Migration/Delete/Index", model);
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteViewModel model)
        {
            switch (model.MigrationDeleteOptions)
            {
                case MigrationDeleteOptions.DeleteMigrations:
                    await _courseService.ChangeCourseRunStatusesForUKPRNSelection(1, (int)RecordStatus.MigrationPending,
                        (int)RecordStatus.Archived);
                    return View("../Migration/DeleteConfirmed/Index");
                case MigrationDeleteOptions.Cancel:
                    return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration });
                default:
                    return RedirectToAction("index");
            }
        }
    }
}