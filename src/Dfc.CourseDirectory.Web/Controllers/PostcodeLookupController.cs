using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.PostcodeLookup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class PostcodeLookupController : Controller
    {
        private readonly ILogger<PostcodeLookupController> _logger;
        private readonly IPostCodeSearchService _postCodeSearchService;
        private readonly IPostcodeLookupHelper _postcodeLookupHelper;

        public PostcodeLookupController(
            ILogger<PostcodeLookupController> logger,
            IPostCodeSearchService postCodeSearchService,
            IPostcodeLookupHelper postcodeLookupHelper
        )
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(postCodeSearchService, nameof(postCodeSearchService));
            Throw.IfNull(postcodeLookupHelper, nameof(postcodeLookupHelper));

            _logger = logger;
            _postCodeSearchService = postCodeSearchService;
            _postcodeLookupHelper = postcodeLookupHelper;
        }

        public async Task<IActionResult> Index([FromQuery] PostcodeLookupRequestModel requestModel)
        {
            PostcodeLookupModel model;

            if (requestModel == null || string.IsNullOrWhiteSpace(requestModel.Postcode))
            {
                model = new PostcodeLookupModel();
            }
            else
            {
                var criteria = _postcodeLookupHelper.GetPostCodeSearchCriteria(requestModel);
                var result = await _postCodeSearchService.SearchAsync(criteria);

                if (result.IsSuccess && result.HasValue)
                {
                    var items = _postcodeLookupHelper.GetPostCodeSearchResultItemModels(result.Value.Value);

                    model = new PostcodeLookupModel(requestModel.Postcode, items);
                }
                else
                {
                    model = new PostcodeLookupModel(requestModel.Postcode, result.Error);
                }
            }

            return ViewComponent(nameof(ViewComponents.PostcodeLookup.PostcodeLookup), model);
        }
    }
}