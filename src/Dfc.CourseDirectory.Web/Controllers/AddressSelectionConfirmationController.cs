using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class AddressSelectionConfirmationController : Controller
    {
        private readonly ILogger<AddressSelectionConfirmationController> _logger;
        private readonly IPostCodeSearchService _postCodeSearchService;

        public AddressSelectionConfirmationController(
            ILogger<AddressSelectionConfirmationController> logger,
            IPostCodeSearchService postCodeSearchService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(postCodeSearchService, nameof(postCodeSearchService));

            _logger = logger;
            _postCodeSearchService = postCodeSearchService;
        }
        //[Authorize]
        //public async Task<IActionResult> Index([FromQuery] AddressSelectionRequestModel requestModel)
        //{
        //    _logger.LogMethodEnter();

        //    AddressSelectionResult model;

        //    AddressSelectionCriteria criteria = new AddressSelectionCriteria(requestModel.Id);
        //    var searchResult = await _postCodeSearchService.RetrieveAsync(criteria);

        //    if (searchResult.IsSuccess && searchResult.HasValue)
        //    {
        //        model = new AddressSelectionResult(
        //            //searchResult.Value.Id,
        //            searchResult.Value.Line1,
        //            searchResult.Value.Line2,
        //            searchResult.Value.City,
        //            searchResult.Value.County,
        //            searchResult.Value.PostCode);
        //    }
        //    else
        //    {
        //        model = new AddressSelectionResult(searchResult.Error);
        //    }

        //    _logger.LogMethodExit();

        //    return ViewComponent(nameof(ViewComponents.PostCodeSearchResult.PostCodeSearchResult), model);
        //}
    }
}