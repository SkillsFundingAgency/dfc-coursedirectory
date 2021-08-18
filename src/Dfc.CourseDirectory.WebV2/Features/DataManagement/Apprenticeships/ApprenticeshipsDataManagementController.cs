using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Mvc;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Apprenticeships
{
    [Route("data-upload/apprenticeships")]
    [RequireFeatureFlag(FeatureFlags.ApprenticeshipsDataManagement)]
    [RequireProviderContext]
    [RestrictProviderTypes(ProviderType.Apprenticeships)]
    public class ApprenticeshipsDataManagementController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IProviderContextProvider _providerContextProvider;

        public ApprenticeshipsDataManagementController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContextProvider = providerContextProvider;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index() =>
            await _mediator.SendAndMapResponse(new Upload.Query(), vm => View("Upload", vm));

        [HttpPost("upload")]
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
                    success => RedirectToAction(nameof(InProgress)).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("in-progress")]
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

        [HttpGet("formatting")]
        public IActionResult Formatting() => View();

        [HttpGet("check-publish")]
        public IActionResult CheckAndPublish() => View();

        [HttpGet("download")]
        [RequireProviderContext]
        public IActionResult Download() => Ok();

        [HttpGet("errors")]
        public IActionResult Errors() => View();

        [HttpGet("template")]
        public IActionResult Template() =>
            new CsvResult<CsvApprenticeshipRow>("apprenticeships-template.csv", Enumerable.Empty<CsvApprenticeshipRow>());

        [HttpGet("delete")]
        public async Task<IActionResult> DeleteUpload() =>
            await _mediator.SendAndMapResponse(
                new DeleteUpload.Query(),
                command => View(command));

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteUpload(DeleteUpload.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(DeleteUploadSuccess)).WithProviderContext(_providerContextProvider.GetProviderContext())));

        [HttpGet("resolve/delete/success")]
        public IActionResult DeleteUploadSuccess() => View();
    }
}
