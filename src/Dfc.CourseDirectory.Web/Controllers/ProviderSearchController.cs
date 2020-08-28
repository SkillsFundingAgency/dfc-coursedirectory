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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.CourseService;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Web.ViewModels;
using System.Diagnostics;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderSearchController : Controller
    {
        private readonly ILogger<ProviderSearchController> _logger;
        private readonly IProviderServiceSettings _providerServiceSettings;
        private readonly IProviderService _providerService;
        private readonly IProviderSearchHelper _providerSearchHelper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUserHelper _userHelper;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;

        public ProviderSearchController(
            ILogger<ProviderSearchController> logger,
            IOptions<ProviderServiceSettings> providerServiceSettings,
            IProviderService providerService,
            IProviderSearchHelper providerSearchHelper,
            IHttpContextAccessor contextAccessor,
            IUserHelper userHelper,
            IOptions<CourseServiceSettings> courseSearchSettings,
            ICourseService courseService

            )
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(providerServiceSettings, nameof(providerServiceSettings));
            Throw.IfNull(providerService, nameof(providerService));
            Throw.IfNull(providerSearchHelper, nameof(providerSearchHelper));
            Throw.IfNull(userHelper, nameof(userHelper));
            Throw.IfNull(courseSearchSettings, nameof(courseSearchSettings));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _providerServiceSettings = providerServiceSettings.Value;
            _providerService = providerService;
            _providerSearchHelper = providerSearchHelper;
            _contextAccessor = contextAccessor;
            _userHelper = userHelper;
            _courseService = courseService;
        }

       // [Authorize(Policy = "ElevatedUserRole")]
        public async Task<IActionResult> Index([FromQuery] ProviderSearchRequestModel requestModel)
        {
           
                if (!_userHelper.IsUserAuthorised(policy: "ElevatedUserRole").Result)
                {
                    return new ContentResult
                    {
                        ContentType = "text/html",
                        Content = "<script>location.reload();</script>"
                    };
                }
                _logger.LogMethodEnter();
                _logger.LogInformationObject("RequestModel", requestModel);
           

            ProviderSearchResultModel model = new ProviderSearchResultModel();
            

            try
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
                        if (null != provider.ProviderContact)
                        {
                            var providerContactTypeP = provider.ProviderContact.Where(s => s.ContactType.Equals("P", StringComparison.InvariantCultureIgnoreCase));
                            string AddressLine1 = string.Empty;
                            if (!(string.IsNullOrEmpty(providerContactTypeP.FirstOrDefault()?.ContactAddress?.PAON?.Description)
                                && string.IsNullOrEmpty(providerContactTypeP.FirstOrDefault()?.ContactAddress?.StreetDescription)))
                            {
                                AddressLine1 = providerContactTypeP.FirstOrDefault()?.ContactAddress?.PAON?.Description
                                                + " " + providerContactTypeP.FirstOrDefault()?.ContactAddress?.StreetDescription + ", ";
                            }
                            string AddressLine2 = string.Empty;
                            if (providerContactTypeP.FirstOrDefault()?.ContactAddress?.Locality != null)
                            {
                                AddressLine2 = providerContactTypeP.FirstOrDefault()?.ContactAddress?.Locality.ToString() + ", ";
                            }

                            string AddressLine3 = string.Empty;
                            if (!string.IsNullOrEmpty(providerContactTypeP.FirstOrDefault()?.ContactAddress?.Items?.FirstOrDefault()))
                            {
                                AddressLine3 = providerContactTypeP.FirstOrDefault()?.ContactAddress?.Items?.FirstOrDefault() + ", ";
                            }

                            string PostCode = string.Empty;
                            if (!string.IsNullOrEmpty(providerContactTypeP?.FirstOrDefault()?.ContactAddress?.PostCode))
                            {
                                PostCode = providerContactTypeP?.FirstOrDefault()?.ContactAddress?.PostCode;
                            }
                        

                            model.AddressTypeL = string.Concat(AddressLine1, AddressLine2, AddressLine3, PostCode);
                            model.TelephoneTypeL = providerContactTypeP?.FirstOrDefault()?.ContactTelephone1;
                            model.WebTypeL = providerContactTypeP?.FirstOrDefault()?.ContactWebsiteAddress;
                            model.EmailTypeL = providerContactTypeP?.FirstOrDefault()?.ContactEmail;
                        }
                        if (provider.Status == Status.Onboarded)
                        {
                            var UKPRN = Convert.ToInt32(requestModel.SearchTerm);
                            //Set the UKPRN here in session
                            _session.SetInt32("UKPRN", Convert.ToInt32(requestModel.SearchTerm));
                            _session.SetString("PendingCourses", "true");

                            IEnumerable<ICourseStatusCountResult> counts = _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(UKPRN))
                                                                .Result
                                                                .Value;
                            int[] pendingStatuses = new int[] { (int)RecordStatus.Pending, (int)RecordStatus.BulkUploadPending, (int)RecordStatus.APIPending, (int)RecordStatus.MigrationPending };

                            var pendingCourseCount = (from ICourseStatusCountResult c in counts
                                                        join int p in pendingStatuses
                                                        on c.Status equals p
                                                        select c.Count).Sum();

                            if (pendingCourseCount > 0)
                            {
                                _session.SetString("PendingCourses", "true");
                            }
                            else
                            {
                                _session.SetString("PendingCourses", "false");
                            }

                        }
                           
                    }
                }
                _logger.LogInformationObject("Model", model);
                       
                    


            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                model.Errors = new string[] { "UKPRN required" };

                return ViewComponent(nameof(ViewComponents.ProviderSearchResult.ProviderSearchResult), model);
            }
            finally
            {
                _logger.LogMethodExit();

            }
            return ViewComponent(nameof(ViewComponents.ProviderSearchResult.ProviderSearchResult), model);
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