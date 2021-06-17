using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Mvc;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses
{
    [Route("data-upload/courses")]
    [RequireFeatureFlag(FeatureFlags.CoursesDataManagement)]
    [RequireProviderContext]
    [RestrictProviderTypes(ProviderType.FE)]
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
        public IActionResult Index() => View("Upload");

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
                    result =>
                        RedirectToAction("Index", nameof(ProviderDashboard))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        ));
        }

        [HttpGet("errors")]
        public IActionResult Errors() => Ok();

        [HttpGet("check-publish")]
        [RequireProviderContext]
        public async Task<IActionResult> CheckAndPublish()
        {
            var query = new CheckAndPublish.Query();
            return await _mediator.SendAndMapResponse(
                query,
                result => result.Match<IActionResult>(
                    hasErrors => RedirectToAction(nameof(Errors)).WithProviderContext(_providerContextProvider.GetProviderContext()),
                    command => View(command)));
        }

        [HttpPost("check-publish")]
        [JourneyMetadata("PublishCourseUpload", typeof(PublishJourneyModel), appendUniqueKey: false, requestDataKeys: "providerId?")]
        public async Task<IActionResult> CheckAndPublish(CheckAndPublish.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    publishResult => publishResult.Status == Core.DataManagement.PublishResultStatus.Success ?
                        RedirectToAction(nameof(Published)).WithProviderContext(_providerContextProvider.GetProviderContext()) :
                        RedirectToAction(nameof(Errors)).WithProviderContext(_providerContextProvider.GetProviderContext())));

        [HttpGet("success")]
        [RequireJourneyInstance]
        [JourneyMetadata("PublishCourseUpload", typeof(PublishJourneyModel), appendUniqueKey: false, requestDataKeys: "providerId?")]
        public async Task<IActionResult> Published() => await _mediator.SendAndMapResponse(new Published.Query(), vm => View(vm));

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<CsvCourseRow>("courses-template.csv", Enumerable.Empty<CsvCourseRow>());
    }
}
