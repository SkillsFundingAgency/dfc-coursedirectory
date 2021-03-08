using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditApprenticeship;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers.EditApprenticeships
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class EditDeliveryMethodController : Controller
    {
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        private ISession Session => HttpContext.Session;

        public EditDeliveryMethodController(
            IApprenticeshipService apprenticeshipService,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _apprenticeshipService = apprenticeshipService ?? throw new ArgumentNullException(nameof(apprenticeshipService));
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(Guid? apprenticeshipId)
        {
            if (!Session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (!apprenticeshipId.HasValue)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeshipById { ApprenticeshipId = apprenticeshipId.Value });

            if (result == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var apprenticeship = Apprenticeship.FromCosmosDbModel(result);

            if (apprenticeship.ApprenticeshipLocations == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            return View("EditDeliveryMethod", new EditApprenticeshipViewModel
            {
                locations = apprenticeship.ApprenticeshipLocations
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(EditApprenticeshipViewModel model)
        {
            if (model.ApprenticeshipId.HasValue)
            {
                var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeshipById { ApprenticeshipId = model.ApprenticeshipId.Value });
                var apprenticeshipForEdit = Apprenticeship.FromCosmosDbModel(result);

                if (apprenticeshipForEdit != null)
                {
                    apprenticeshipForEdit.MarketingInformation = model?.Information;
                    apprenticeshipForEdit.ContactEmail = model?.Email;
                    apprenticeshipForEdit.ContactWebsite = model?.WebSite;
                    apprenticeshipForEdit.ContactTelephone = model?.Telephone;
                    apprenticeshipForEdit.Url = model?.ContactUsURL;
                    apprenticeshipForEdit.UpdatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(); // User.Identity.Name;
                    apprenticeshipForEdit.UpdatedDate = DateTime.Now;
                    apprenticeshipForEdit.BulkUploadErrors = new List<BulkUploadError> { };
                    if (apprenticeshipForEdit.BulkUploadErrors.Count() == 0)
                    {
                        apprenticeshipForEdit.RecordStatus = Services.Models.RecordStatus.BulkUploadReadyToGoLive;
                    }
                    var updatedApprenticeship = await _apprenticeshipService.UpdateApprenticeshipAsync(apprenticeshipForEdit);
                }

                return RedirectToAction("Index", "PublishApprenticeships");
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
