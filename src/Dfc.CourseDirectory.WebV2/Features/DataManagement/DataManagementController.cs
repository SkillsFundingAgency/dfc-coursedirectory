using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement
{
    [RequireProviderContext]
    [Route("data-upload")]
    public class DataManagementController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ProviderContext _providerContext;

        public DataManagementController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [HttpGet("venues")]
        public IActionResult Venues()
        {
            return View();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(Venues.Command command)
        {
            var file = Request.Form.Files?.FirstOrDefault();

            return await _mediator.SendAndMapResponse(
                new Venues.Command()
                {
                    File = file,
                    ProviderId = _providerContext.ProviderInfo.ProviderId
                },
                response => response.Match<IActionResult>(
                    errors => RedirectToAction("Validation"),
                    success => RedirectToAction("CheckAndPublishVenues")));
        }

        [HttpGet("venues/validation")]
        public IActionResult Validation()
        {
            return View();
        }

        [HttpGet("venues/checkandpublish")]
        public IActionResult CheckAndPublishVenues()
        {
            return View();
        }

    }
}
