using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Mvc;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues
{
    [Route("data-upload/venues")]
    [RequireFeatureFlag(FeatureFlags.DataManagement)]
    public class VenuesDataManagementController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IProviderContextProvider _providerContextProvider;

        public VenuesDataManagementController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContextProvider = providerContextProvider;
        }

        [HttpGet("")]
        [RequireProviderContext]
        public IActionResult Index() => View("Upload");

        [HttpGet("download")]
        [RequireProviderContext]
        public async Task<IActionResult> Download() => await _mediator.SendAndMapResponse(
            new Download.Query(),
            result => new CsvResult<VenueRow>(result.FileName, result.Rows));

        [HttpPost("upload")]
        [RequireProviderContext]
        public async Task<IActionResult> Upload(Upload.Command command)
        {
            var file = Request.Form.Files?.GetFile(nameof(command.File));

            return await _mediator.SendAndMapResponse(
                new Upload.Command()
                {
                    File = file
                },
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(CheckAndPublish))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("check-publish")]
        [RequireProviderContext]
        public IActionResult CheckAndPublish()
        {
            return View();
        }

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<VenueRow>("venues-template.csv", Enumerable.Empty<VenueRow>());
    }
}
