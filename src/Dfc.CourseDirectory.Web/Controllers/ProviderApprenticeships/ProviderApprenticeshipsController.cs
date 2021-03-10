using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewModels.ProviderApprenticeships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class ProviderApprenticeshipsController : Controller
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        private ISession Session => HttpContext.Session;

        public ProviderApprenticeshipsController(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
        }

        [Authorize]
        public async Task<IActionResult> Index(Guid? apprenticeshipId, string message)
        {
            int? UKPRN = Session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var liveApprenticeships = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeships
            {
                Predicate = a =>
                    a.ProviderUKPRN == UKPRN
                    && a.RecordStatus == (int)ApprenticeshipStatus.Live
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
                Apprenticeships = liveApprenticeships.Values.Select(Apprenticeship.FromCosmosDbModel).ToList()
            });
        }

        [Authorize]
        public async Task<IActionResult> ProviderApprenticeshipsSearch([FromQuery] ProviderApprenticeShipSearchRequestModel requestModel)
        {
            int? UKPRN = Session.GetInt32("UKPRN");

            var liveApprenticeships = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeships
            {
                Predicate = a =>
                    a.ProviderUKPRN == UKPRN
                    && a.RecordStatus == (int)ApprenticeshipStatus.Live
            });

            var model = new ProviderApprenticeshipsSearchResultModel();

            if (string.IsNullOrWhiteSpace(requestModel.SearchTerm))
            {
                model.Items = liveApprenticeships.Values.Select(Apprenticeship.FromCosmosDbModel).ToList();
            }
            else
            {
                var searchTermWords = requestModel.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                model.Items = liveApprenticeships.Values
                    .Where(r =>
                        $"{r.ApprenticeshipTitle} {r.MarketingInformation} {r.NotionalNVQLevelv2}"
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Any(w => searchTermWords.Any(s => s.Equals(w, StringComparison.OrdinalIgnoreCase)))
                        && r.RecordStatus == (int)ApprenticeshipStatus.Live)
                    .Select(Apprenticeship.FromCosmosDbModel)
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
