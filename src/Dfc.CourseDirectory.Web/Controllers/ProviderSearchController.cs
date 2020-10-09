using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.ProviderService;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderSearchController : Controller
    {
        private readonly ILogger<ProviderSearchController> _logger;
        private readonly IProviderService _providerService;
        private readonly IHttpContextAccessor _contextAccessor;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public ProviderSearchController(
            ILogger<ProviderSearchController> logger,
            IProviderService providerService,
            IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(providerService, nameof(providerService));

            _logger = logger;
            _providerService = providerService;
            _contextAccessor = contextAccessor;
        }

        [Authorize(Policy = "ElevatedUserRole")]
        [HttpPost]
        public async Task<JsonResult> OnBoardProvider([FromBody] ProviderAjaxRequestModel ajaxRequest)
        {
            _logger.LogMethodEnter();
            _logger.LogInformationObject("RequestModel", ajaxRequest);
            string ResultText = string.Empty;
            bool Success = true;

            if (string.IsNullOrEmpty(ajaxRequest.ProviderId))
            {
                ResultText = "ProviderId was NOT passed to our system";
                Success = false;
            }
            else if (ajaxRequest.ProviderId.Equals("00000000-0000-0000-0000-000000000000", StringComparison.InvariantCultureIgnoreCase))
            {
                ResultText = "Invalid ProviderId was passed to our system";
                Success = false;
            }
            else
            {
                try
                {
                    // TODO - UpdatedBy will be updated with the name of logged person
                    ProviderAdd providerAdd = new ProviderAdd(new Guid(ajaxRequest.ProviderId), (int)Status.Onboarded, "ProviderPortal - Add Provider");
                    var result = await _providerService.AddProviderAsync(providerAdd);
                    if (result.IsSuccess && result.HasValue)
                    {
                        ResultText = "Provider added.";
                    }
                    else
                    {
                        ResultText = "Provider Add Service did NOT return a result.";
                        Success = false;
                    }
                }
                catch (Exception ex)
                {
                    ResultText = ex.Message;
                    Success = false;
                }
            }

            _logger.LogInformation("Success", Success);
            _logger.LogInformation("ResultText", ResultText);
            _logger.LogMethodExit();
            _session.SetInt32("UKPRN", Convert.ToInt32(ajaxRequest.UKPRN));
            return Json(new { success = Success, resultText = ResultText });
        }


        [Authorize(Policy = "ElevatedUserRole")]
        public async Task<IActionResult> SearchProvider(
            int UKPRN,
            [FromServices] IProviderInfoCache providerInfoCache,
            [FromServices] IProviderContextProvider providerContextProvider)
        {
            var providerInfo = await providerInfoCache.GetProviderInfoForUkprn(UKPRN);
            providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

            _session.SetInt32("UKPRN", UKPRN);
            return View("../Provider/Dashboard");
        }
    }
}