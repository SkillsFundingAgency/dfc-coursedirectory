using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditApprenticeship;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers.EditApprenticeships
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class EditDeliveryMethodController : Controller
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        private ISession Session => HttpContext.Session;

        public EditDeliveryMethodController(
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

            var result = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeship { ApprenticeshipId = apprenticeshipId.Value });

            if (result == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var apprenticeship = Apprenticeship.FromSqlModel(result);

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
                var result = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeship { ApprenticeshipId = model.ApprenticeshipId.Value });

                await _sqlQueryDispatcher.ExecuteQuery(new UpdateApprenticeship()
                {
                    ApprenticeshipId = result.ApprenticeshipId,
                    ApprenticeshipLocations = result.ApprenticeshipLocations.Select(CreateApprenticeshipLocation.FromModel),
                    ApprenticeshipWebsite = model.ContactUsURL,
                    ContactEmail = model.Email,
                    ContactTelephone = model.Telephone,
                    ContactWebsite = model.WebSite,
                    MarketingInformation = model.Information,
                    Standard = result.Standard,
                    UpdatedBy = _currentUserProvider.GetCurrentUser(),
                    UpdatedOn = _clock.UtcNow
                });

                return RedirectToAction("Index", "PublishApprenticeships");
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
