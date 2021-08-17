using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditApprenticeship;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers.EditApprenticeships
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class EditApprenticeshipController : Controller
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        private ISession Session => HttpContext.Session;

        public EditApprenticeshipController(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(Guid? apprenticeshipid)
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
                var result = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeship { ApprenticeshipId = apprenticeshipid.Value });

                if (result != null)
                {
                    EditApprenticeshipViewModel vm = new EditApprenticeshipViewModel
                    {
                        ApprenticeshipTitle = result.Standard.StandardName,
                        Information = result.MarketingInformation,
                        WebSite = result.ContactWebsite,
                        Email = result.ContactEmail,
                        Telephone = result.ContactTelephone,
                        ContactUsURL = result.ApprenticeshipWebsite
                    };

                    return View("EditApprenticeship", vm);
                }
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(EditApprenticeshipViewModel model)
        {
            if (model.ApprenticeshipId.HasValue)
            {
                var apprenticeship = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeship() { ApprenticeshipId = model.ApprenticeshipId.Value });

                await _sqlQueryDispatcher.ExecuteQuery(new UpdateApprenticeship()
                {
                    ApprenticeshipId = model.ApprenticeshipId.Value,
                    ContactEmail = model.Email,
                    ContactTelephone = model.Telephone,
                    ContactWebsite = model.WebSite,
                    MarketingInformation = model.Information,
                    ApprenticeshipWebsite = model.ContactUsURL,
                    ApprenticeshipLocations = apprenticeship.ApprenticeshipLocations.Select(CreateApprenticeshipLocation.FromModel),
                    UpdatedBy = _currentUserProvider.GetCurrentUser(),
                    UpdatedOn = _clock.UtcNow
                });

                return RedirectToAction("Index", "PublishApprenticeships");
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
