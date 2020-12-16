using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderSearchController : Controller
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        private ISession Session => HttpContext.Session;

        public ProviderSearchController(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, ICurrentUserProvider currentUserProvider, IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        [Authorize(Policy = "ElevatedUserRole")]
        [HttpPost]
        public async Task<JsonResult> OnBoardProvider([FromBody] ProviderAjaxRequestModel ajaxRequest)
        {
            if (string.IsNullOrEmpty(ajaxRequest.ProviderId))
            {
                return Result(false, "ProviderId was NOT passed to our system");
            }

            if (!Guid.TryParse(ajaxRequest.ProviderId, out var providerId))
            {
                return Result(false, "Invalid ProviderId was passed to our system");
            }

            try
            {
                await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderOnboarded
                {
                    ProviderId = providerId,
                    UpdatedBy = _currentUserProvider.GetCurrentUser(),
                    UpdatedDateTime = _clock.UtcNow.ToLocalTime()
                });

                Session.SetInt32("UKPRN", Convert.ToInt32(ajaxRequest.UKPRN));

                return Result(true, "Provider added.");
            }
            catch (Exception ex)
            {
                return Result(false, ex.Message);
            }
            
            JsonResult Result(bool success, string resultText) => Json(new { success, resultText });
        }

        [Authorize(Policy = "ElevatedUserRole")]
        public async Task<IActionResult> SearchProvider(
            int UKPRN,
            [FromServices] IProviderInfoCache providerInfoCache,
            [FromServices] IProviderContextProvider providerContextProvider)
        {
            var providerInfo = await providerInfoCache.GetProviderInfoForUkprn(UKPRN);
            providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

            Session.SetInt32("UKPRN", UKPRN);
            return RedirectToAction("Index", "ProviderDashboard");
        }
    }
}
