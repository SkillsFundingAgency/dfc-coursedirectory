using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearchResult;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class PostCodeSearchController : Controller
    {
        private readonly ILogger<PostCodeSearchController> _logger;
        private readonly IPostCodeSearchSettings _postCodeSearchSettings;
        private readonly IPostCodeSearchService _postCodeSearchService;
        private readonly IPostCodeSearchHelper _postCodeSearchHelper;

        public PostCodeSearchController(
            ILogger<PostCodeSearchController> logger,
            IOptions<PostCodeSearchSettings> postCodeSearchSettings,
            IPostCodeSearchService postCodeSearchService,
            IPostCodeSearchHelper postCodeSearchHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(postCodeSearchSettings, nameof(postCodeSearchSettings));
            Throw.IfNull(postCodeSearchService, nameof(postCodeSearchService));
            Throw.IfNull(postCodeSearchHelper, nameof(postCodeSearchHelper));

            _logger = logger;
            _postCodeSearchSettings = postCodeSearchSettings.Value;
            _postCodeSearchService = postCodeSearchService;
            _postCodeSearchHelper = postCodeSearchHelper;
        }

        public async Task<IActionResult> Index([FromQuery] PostCodeSearchRequestModel requestModel)
        {
            _logger.LogMethodEnter();

            PostCodeSearchResultModel model;

            PostCodeSearchCriteria criteria = new PostCodeSearchCriteria(requestModel.PostCode);

            var searchResult = await _postCodeSearchService.SearchAsync(criteria);

            AddressSelectionCriteria criteria1 = new AddressSelectionCriteria("GB|RM|B|51879423");
            var x = await _postCodeSearchService.RetrieveAsync(criteria1);

            if (searchResult.IsSuccess && searchResult.HasValue)
            {
                var items = _postCodeSearchHelper.GetPostCodeSearchResultItemModels(searchResult.Value.Value);
                model = new PostCodeSearchResultModel(
                    requestModel.PostCode,null,null,
                    items);
            }
            else
            {
                model = new PostCodeSearchResultModel(searchResult.Error);
            }

            _logger.LogMethodExit();

            return ViewComponent(nameof(ViewComponents.PostCodeSearchResult.PostCodeSearchResult), model);
        }
    }
}