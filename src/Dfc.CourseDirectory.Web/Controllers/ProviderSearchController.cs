using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderSearchController : Controller
    {
        private readonly ILogger<ProviderSearchController> _logger;
        private readonly IProviderSearchSettings _providerSearchSettings;
        private readonly IProviderSearchService _providerSearchService;
        private readonly IProviderSearchHelper _providerSearchHelper;

        public ProviderSearchController(
            ILogger<ProviderSearchController> logger,
            IOptions<ProviderSearchSettings> providerSearchSettings,
            IProviderSearchService providerSearchService,
            IProviderSearchHelper providerSearchHelper
            )
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(providerSearchSettings, nameof(providerSearchSettings));
            Throw.IfNull(providerSearchService, nameof(providerSearchService));
            Throw.IfNull(providerSearchHelper, nameof(providerSearchHelper));

            _logger = logger;
            _providerSearchSettings = providerSearchSettings.Value;
            _providerSearchService = providerSearchService;
            _providerSearchHelper = providerSearchHelper;
        }
        public async Task<IActionResult> Index([FromQuery] ProviderSearchRequestModel requestModel)
        {
            //requestModel.SearchTerm = "10028213";
            ProviderSearchResultModel model;

            _logger.LogMethodEnter();
            _logger.LogInformationObject("Model", requestModel);

            if (requestModel == null)
            {
                model = new ProviderSearchResultModel();
            }
            else
            {
                var criteria = _providerSearchHelper.GetProviderSearchCriteria(requestModel);

                var result = await _providerSearchService.SearchAsync(criteria);

                if (result.IsSuccess && result.HasValue)
                {
                    model = new ProviderSearchResultModel(
                        requestModel.SearchTerm,
                        result.Value.Value);
                }
                else
                {
                    model = new ProviderSearchResultModel(result.Error);
                }
                model.ServiceWasCalled = true;

                foreach (var provider in model.Items)
                {
                    if (model.Items.FirstOrDefault() != null)
                    {
                        var providerContactTypeL = provider.ProviderContact.Where(s => s.ContactType.Equals("L", StringComparison.InvariantCultureIgnoreCase));
                        string AddressLine1 = string.Empty;
                        if(!(string.IsNullOrEmpty(providerContactTypeL.FirstOrDefault()?.ContactAddress.PAON.Description) 
                            && string.IsNullOrEmpty(providerContactTypeL.FirstOrDefault()?.ContactAddress.StreetDescription)))
                        {
                            AddressLine1 = providerContactTypeL.FirstOrDefault()?.ContactAddress.PAON.Description
                                            + " " + providerContactTypeL.FirstOrDefault()?.ContactAddress.StreetDescription + ", ";
                        }
                        string AddressLine2 = string.Empty;
                        if (providerContactTypeL.FirstOrDefault()?.ContactAddress.Locality != null) //!string.IsNullOrEmpty(providerContactTypeL.FirstOrDefault()?.ContactAddress.Locality?.ToString()))
                        {
                            AddressLine2 = providerContactTypeL.FirstOrDefault()?.ContactAddress.Locality.ToString() + ", ";
                        }

                        string AddressLine3 = string.Empty;
                        if (!string.IsNullOrEmpty(providerContactTypeL.FirstOrDefault()?.ContactAddress.Items[0]))
                        {
                            AddressLine3 = providerContactTypeL.FirstOrDefault()?.ContactAddress.Items[0] + ", ";
                        }

                        string PostCode = string.Empty;
                        if (!string.IsNullOrEmpty(providerContactTypeL.FirstOrDefault()?.ContactAddress.PostCode))
                        {
                            PostCode = providerContactTypeL.FirstOrDefault()?.ContactAddress.PostCode;
                        }

                        model.AddressTypeL = string.Concat(AddressLine1, AddressLine2, AddressLine3, PostCode);

                        model.TelephoneTypeL = providerContactTypeL.FirstOrDefault()?.ContactTelephone1;
                        model.WebTypeL = providerContactTypeL.FirstOrDefault()?.ContactWebsiteAddress;
                        model.EmailTypeL = providerContactTypeL.FirstOrDefault()?.ContactEmail;

                        // For DEVELOPMENT & TESTING => TODO To Be Removed
                        if (provider.UnitedKingdomProviderReferenceNumber.Equals("10057206", StringComparison.InvariantCultureIgnoreCase))
                            provider.ProviderStatus = "NotActive";
                        if (provider.UnitedKingdomProviderReferenceNumber.Equals("10057217", StringComparison.InvariantCultureIgnoreCase))
                            provider.Registered = Registered.Onboarded;
                    }
                }
            }

            _logger.LogMethodExit();
            return ViewComponent(nameof(ViewComponents.ProviderSearchResult.ProviderSearchResult), model);
        }

        [HttpPost]
        public JsonResult OnBoardProvider([FromBody] ProviderAjaxRequestModel ajaxRequest) 
        {
            string ResultText = string.Empty;
            bool Success = true;

            // For Development and Testing TODO. Clear on ready
            // ajaxRequest.ProviderId = "00000000-0000-0000-0000-000000000000";

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
                    //var result = await _providerSearchService.OnboardProvider(ajaxRequest.ProviderId);
                    ResultText = "Provider added  - " + ajaxRequest.ProviderId;
                }
                catch (Exception ex)
                {
                    ResultText = ex.Message;
                    Success = false;
                }
            }

            return Json(new { success = Success,  resultText = ResultText });
        }
    }
}