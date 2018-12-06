using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
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
            ProviderSearchResultModel model;

            _logger.LogMethodEnter();
            _logger.LogInformationObject("Model", requestModel);

            if (requestModel == null)
            {
                model = new ProviderSearchResultModel();
            }
            else
            {
                var criteria = _providerSearchHelper.GetProviderSearchCriteria(
                    requestModel);

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
            }

            _logger.LogMethodExit();
            return ViewComponent(nameof(ViewComponents.ProviderSearchResult.ProviderSearchResult), model);

        }
    }
}