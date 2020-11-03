using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
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
    public class EditApprenticeshipController : Controller
    {
        private readonly ILogger<EditApprenticeshipController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IOptions<ApprenticeshipSettings> _apprenticeshipSettings;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public EditApprenticeshipController(
           ILogger<EditApprenticeshipController> logger,
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
        public async Task<IActionResult> Index(Guid? apprenticeshipid, Apprenticeship request)
        {
            int? UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (apprenticeshipid.HasValue)
            {
                string apprenticeshipGuidId = apprenticeshipid.ToString();

                var result = await _apprenticeshipService.GetApprenticeshipByIdAsync(apprenticeshipGuidId);

                if (result.IsSuccess)
                {
                    EditApprenticeshipViewModel vm = new EditApprenticeshipViewModel
                    {
                        ApprenticeshipTitle = result.Value.ApprenticeshipTitle,
                        Information = result.Value.MarketingInformation,
                        WebSite = result.Value.ContactWebsite,
                        Email = result.Value.ContactEmail,
                        Telephone = result.Value.ContactTelephone,
                        ContactUsURL = result.Value.Url
                    };

                    return View("EditApprenticeship", vm);
                }
            }
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(EditApprenticeshipViewModel model)
        {
            if (model.ApprenticeshipId.HasValue)
            {
                string apprenticeshipGuidId = model.ApprenticeshipId.ToString();

                var apprenticeshipForEdit = await _apprenticeshipService.GetApprenticeshipByIdAsync(apprenticeshipGuidId);

                if (apprenticeshipForEdit.IsSuccess && apprenticeshipForEdit.HasValue)
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
