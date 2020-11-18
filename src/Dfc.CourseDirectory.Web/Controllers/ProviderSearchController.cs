using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderSearchController : Controller
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        private ISession Session => HttpContext.Session;

        public ProviderSearchController(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
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
                // TODO - UpdatedBy will be updated with the name of logged person
                await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderOnboarded
                {
                    ProviderId = providerId,
                    UpdatedBy = "ProviderPortal - Add Provider"
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
            return RedirectToAction("Dashboard", "Provider");
        }
    }
}
