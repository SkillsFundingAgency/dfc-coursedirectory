using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.Configuration;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditApprenticeship;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers.EditApprenticeships
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class EditDeliveryMethodController : Controller
    {
        private readonly ILogger<EditDeliveryMethodController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IOptions<ApprenticeshipSettings> _apprenticeshipSettings;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public EditDeliveryMethodController(
           ILogger<EditDeliveryMethodController> logger,
          IHttpContextAccessor contextAccessor,
           IApprenticeshipService apprenticeshipService,
           IOptions<ApprenticeshipSettings> apprenticeshipSettings)
        {
            Throw.IfNull(logger, nameof(logger));

            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));

            Throw.IfNull(apprenticeshipSettings, nameof(apprenticeshipSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));

            _apprenticeshipSettings = apprenticeshipSettings;
            _logger = logger;
            _contextAccessor = contextAccessor;

            _apprenticeshipService = apprenticeshipService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(Guid? apprenticeshipId)
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (!apprenticeshipId.HasValue)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var result = await _apprenticeshipService.GetApprenticeshipByIdAsync(apprenticeshipId.ToString());

            if (!result.IsSuccess)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (result.Value?.ApprenticeshipLocations == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            return View("EditDeliveryMethod", new EditApprenticeshipViewModel
            {
                locations = result.Value.ApprenticeshipLocations
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(EditApprenticeshipViewModel model)
        {
            if (model.ApprenticeshipId.HasValue)
            {
                string apprenticeshipGuidId = model.ApprenticeshipId.ToString();

                var apprenticeshipForEdit = await _apprenticeshipService.GetApprenticeshipByIdAsync(apprenticeshipGuidId);

                if (apprenticeshipForEdit.IsSuccess)
                {
                    apprenticeshipForEdit.Value.MarketingInformation = model?.Information;
                    apprenticeshipForEdit.Value.ContactEmail = model?.Email;
                    apprenticeshipForEdit.Value.ContactWebsite = model?.WebSite;
                    apprenticeshipForEdit.Value.ContactTelephone = model?.Telephone;
                    apprenticeshipForEdit.Value.Url = model?.ContactUsURL;
                    apprenticeshipForEdit.Value.UpdatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(); // User.Identity.Name;
                    apprenticeshipForEdit.Value.UpdatedDate = DateTime.Now;
                    apprenticeshipForEdit.Value.BulkUploadErrors = new List<BulkUploadError> { };
                    if (apprenticeshipForEdit.Value.BulkUploadErrors.Count() == 0)
                    {
                        apprenticeshipForEdit.Value.RecordStatus = Services.Enums.RecordStatus.BulkUploadReadyToGoLive;
                    }
                    var updatedApprenticeship = await _apprenticeshipService.UpdateApprenticeshipAsync(apprenticeshipForEdit.Value);
                }

                return RedirectToAction("Index", "PublishApprenticeships");
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
