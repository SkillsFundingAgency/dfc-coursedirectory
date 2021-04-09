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
    [RequireProviderContext]
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
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download() => await _mediator.SendAndMapResponse(
            new Download.Query(),
            result => new CsvResult<VenueRow>(result.FileName, result.Rows));

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(Upload.Command command)
        {
            var file = Request.Form.Files?.GetFile(nameof(command.File));

            return await _mediator.SendAndMapResponse(
                new Upload.Command()
                {
                    File = file
                },
                response => response.Match(
                    errors => RedirectToAction(nameof(Validation))
                        .WithProviderContext(_providerContextProvider.GetProviderContext()),
                    success => RedirectToAction(nameof(CheckAndPublish)))
                        .WithProviderContext(_providerContextProvider.GetProviderContext()));
        }

        [HttpGet("validation")]
        public IActionResult Validation()
        {
            return View();
        }

        [HttpGet("check-publish")]
        public IActionResult CheckAndPublish()
        {
            return View();
        }
    }
}
