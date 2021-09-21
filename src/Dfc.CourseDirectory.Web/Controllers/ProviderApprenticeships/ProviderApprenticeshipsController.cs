using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewModels.ProviderApprenticeships;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class ProviderApprenticeshipsController : Controller
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderContextProvider _providerContextProvider;

        private ISession Session => HttpContext.Session;

        public ProviderApprenticeshipsController(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }

        [Authorize]
        public async Task<IActionResult> Index(Guid? apprenticeshipId, string message)
        {
            int? UKPRN = Session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var liveApprenticeships = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeshipsForProvider()
            {
                ProviderId = _providerContextProvider.GetProviderId(withLegacyFallback: true)
            });

            if (apprenticeshipId.HasValue)
            {
                var linkMessage = $"<a id=\"apprenticeshiplink\" class=\"govuk-link\" href=\"#\" data-apprenticeshipid=\"{apprenticeshipId.Value}\">{message}</a>";
                
                if (!string.IsNullOrEmpty(message))
                {
                    ViewBag.Message = linkMessage;
                }

                ViewBag.ApprenticeshipId = apprenticeshipId.Value;
            }

            return View(new ProviderApprenticeshipsViewModel
            {
                Apprenticeships = liveApprenticeships.Select(Apprenticeship.FromSqlModel).ToList()
            });
        }

        [Authorize]
        public async Task<IActionResult> ProviderApprenticeshipsSearch([FromQuery] ProviderApprenticeShipSearchRequestModel requestModel)
        {
            int? UKPRN = Session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var liveApprenticeships = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeshipsForProvider()
            {
                ProviderId = _providerContextProvider.GetProviderId(withLegacyFallback: true)
            });

            var model = new ProviderApprenticeshipsSearchResultModel();

            if (string.IsNullOrWhiteSpace(requestModel.SearchTerm))
            {
                model.Items = liveApprenticeships.Select(Apprenticeship.FromSqlModel).ToList();
            }
            else
            {
                var searchTermWords = requestModel.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                model.Items = liveApprenticeships
                    .Where(r =>
                        $"{r.Standard.StandardName} {r.MarketingInformation} {r.Standard.NotionalNVQLevelv2}"
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Any(w => searchTermWords.Any(s => s.Equals(w, StringComparison.OrdinalIgnoreCase))))
                    .Select(Apprenticeship.FromSqlModel)
                    .ToList();
            }

            return ViewComponent(nameof(ProviderApprenticeshipSearchResult), model);
        }

        [Authorize]
        public IActionResult DeleteApprenticeship()
        {
            return RedirectToAction("Index", "Apprenticeships");
        }

    }
}
