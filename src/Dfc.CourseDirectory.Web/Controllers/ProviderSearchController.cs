using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.ProviderService;
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
        private readonly IProviderServiceSettings _providerServiceSettings;
        private readonly IProviderService _providerService;
        private readonly IProviderSearchHelper _providerSearchHelper;

        public ProviderSearchController(
            ILogger<ProviderSearchController> logger,
            IOptions<ProviderServiceSettings> providerServiceSettings,
            IProviderService providerService,
            IProviderSearchHelper providerSearchHelper
            )
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(providerServiceSettings, nameof(providerServiceSettings));
            Throw.IfNull(providerService, nameof(providerService));
            Throw.IfNull(providerSearchHelper, nameof(providerSearchHelper));

            _logger = logger;
            _providerServiceSettings = providerServiceSettings.Value;
            _providerService = providerService;
            _providerSearchHelper = providerSearchHelper;
        }
        public async Task<IActionResult> Index([FromQuery] ProviderSearchRequestModel requestModel)
        {
            _logger.LogMethodEnter();
            _logger.LogInformationObject("RequestModel", requestModel);

            ProviderSearchResultModel model;

            if (requestModel == null)
            {
                model = new ProviderSearchResultModel();
            }
            else
            {
                var criteria = _providerSearchHelper.GetProviderSearchCriteria(requestModel);

                var result = await _providerService.GetProviderByPRNAsync(criteria);

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
                        if (providerContactTypeL.FirstOrDefault()?.ContactAddress.Locality != null) 
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
                    }
                }
            }
            _logger.LogInformationObject("Model", model);
            _logger.LogMethodExit();
            return ViewComponent(nameof(ViewComponents.ProviderSearchResult.ProviderSearchResult), model);
        }

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
            return Json(new { success = Success,  resultText = ResultText });
        }
    }
}