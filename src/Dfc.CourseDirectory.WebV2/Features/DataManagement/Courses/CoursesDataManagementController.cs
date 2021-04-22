using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Mvc;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses
{
    [Route("data-upload/courses")]
    [RequireFeatureFlag(FeatureFlags.DataManagement)]
    public class CoursesDataManagementController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IProviderContextProvider _providerContextProvider;

        public CoursesDataManagementController(IMediator mediator, IProviderContextProvider providerContextProvider)
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
            result => new CsvResult<CourseRow>(result.FileName, result.Rows));

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
                    result =>
                        RedirectToAction(
                            result == Courses.Upload.UploadResult.ProcessingCompleted ? nameof(CheckAndPublish) : nameof(InProgress))
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
                    UploadStatus.Processed => (IActionResult)RedirectToAction(nameof(CheckAndPublish))
                        .WithProviderContext(_providerContextProvider.GetProviderContext()),
                    _ => View(status)
                }));

        [HttpGet("check-publish")]
        [RequireProviderContext]
        public IActionResult CheckAndPublish()
        {
            return View();
        }

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<CourseRow>("courses-template.csv", Enumerable.Empty<CourseRow>());
    }
}
