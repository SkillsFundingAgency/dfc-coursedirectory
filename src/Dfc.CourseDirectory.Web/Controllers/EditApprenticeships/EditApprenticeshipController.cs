using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditApprenticeship;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers.EditApprenticeships
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class EditApprenticeshipController : Controller
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        private ISession Session => HttpContext.Session;

        public EditApprenticeshipController(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(Guid? apprenticeshipid, Apprenticeship request)
        {
            int? UKPRN;

            if (Session.GetInt32("UKPRN") != null)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (apprenticeshipid.HasValue)
            {
                var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeshipById { ApprenticeshipId = apprenticeshipid.Value });

                if (result != null)
                {
                    EditApprenticeshipViewModel vm = new EditApprenticeshipViewModel
                    {
                        ApprenticeshipTitle = result.ApprenticeshipTitle,
                        Information = result.MarketingInformation,
                        WebSite = result.ContactWebsite,
                        Email = result.ContactEmail,
                        Telephone = result.ContactTelephone,
                        ContactUsURL = result.Url
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
                        apprenticeshipForEdit.RecordStatus = ApprenticeshipStatus.BulkUploadReadyToGoLive;
                    }

                    await _cosmosDbQueryDispatcher.ExecuteQuery(apprenticeshipForEdit.ToUpdateApprenticeship());
                }

                return RedirectToAction("Index", "PublishApprenticeships");
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
