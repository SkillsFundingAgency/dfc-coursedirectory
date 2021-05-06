using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
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
            result => new CsvResult<CsvVenueRow>(result.FileName, result.Rows));

        [HttpGet("download-errors")]
        [RequireProviderContext]
        public async Task<IActionResult> DownloadErrors() => await _mediator.SendAndMapResponse(
            new DownloadErrors.Query(),
            result => new CsvResult<CsvVenueRowWithErrors>(result.FileName, result.Rows));

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
                    errors =>
                    {
                        ViewBag.MissingHeaders = errors.MissingHeaders;
                        return this.ViewFromErrors(errors);
                    },
                    result =>
                        RedirectToAction(
                            result switch
                            {
                                Venues.Upload.UploadSucceededResult.ProcessingCompletedSuccessfully => nameof(CheckAndPublish),
                                Venues.Upload.UploadSucceededResult.ProcessingCompletedWithErrors => nameof(Errors),
                                _ => nameof(InProgress)
                            })
                        .WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("in-progress")]
        [RequireProviderContext]
        public async Task<IActionResult> InProgress() => await _mediator.SendAndMapResponse(
            new InProgress.Query(),
            result => result.Match(
                notFound => NotFound(),
                status => status switch
                {
                    UploadStatus.ProcessedSuccessfully => (IActionResult)RedirectToAction(nameof(CheckAndPublish))
                        .WithProviderContext(_providerContextProvider.GetProviderContext()),
                    UploadStatus.ProcessedWithErrors => RedirectToAction(nameof(Errors))
                        .WithProviderContext(_providerContextProvider.GetProviderContext()),
                    _ => View(status)
                }));

        [HttpGet("errors")]
        [RequireProviderContext]
        public IActionResult Errors()
        {
            return View();
        }

        [HttpGet("check-publish")]
        [RequireProviderContext]
        public IActionResult CheckAndPublish()
        {
            return View();
        }

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<CsvVenueRow>("venues-template.csv", Enumerable.Empty<CsvVenueRow>());
    }
}
