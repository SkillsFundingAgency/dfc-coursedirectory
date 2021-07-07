using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.DeleteRow;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.Mvc;
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

        [HttpGet("errors")]
        public IActionResult Errors() => Ok();

        [HttpGet("resolve")]
        public IActionResult ResolveList() => Ok();

        [HttpGet("resolve/{rowNumber}/delivery")]
        public async Task<IActionResult> ResolveRowDeliveryMode(ResolveRowDeliveryMode.Query query) =>
            await _mediator.SendAndMapResponse(
                query,
                result => result.Match<IActionResult>(
                    notFound => NotFound(),
                    command => View(command)));

        [HttpPost("resolve/{rowNumber}/delivery")]
        public async Task<IActionResult> ResolveRowDeliveryMode([FromRoute] int rowNumber, ResolveRowDeliveryMode.Command command)
        {
            command.RowNumber = rowNumber;
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    notFound => NotFound(),
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ResolveRowDetails), new
                    {
                        rowNumber = rowNumber,
                        deliveryMode = command.DeliveryMode switch
                        {
                            CourseDeliveryMode.ClassroomBased => "classroom",
                            CourseDeliveryMode.Online => "online",
                            CourseDeliveryMode.WorkBased => "work",
                            _ => throw new System.NotSupportedException($"Unknown delivery mode: '{command.DeliveryMode}'.")
                        }
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("resolve/{rowNumber}/description")]
        public async Task<IActionResult> ResolveRowDescription(ResolveRowDescription.Query query) =>
            await _mediator.SendAndMapResponse(query, errors => this.ViewFromErrors(errors, statusCode: System.Net.HttpStatusCode.OK));

        [HttpPost("resolve/{rowNumber}/description")]
        public async Task<IActionResult> ResolveRowDescription([FromRoute] int rowNumber, ResolveRowDescription.Command command)
        {
            command.RowNumber = rowNumber;

            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    uploadStatus => (uploadStatus switch
                    {
                        UploadStatus.ProcessedSuccessfully => RedirectToAction(nameof(CheckAndPublish)),
                        _ => RedirectToAction(nameof(ResolveList))
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("resolve/{rowNumber}/details")]
        [RequireValidModelState]
        public IActionResult ResolveRowDetails(
            [FromRoute] int rowNumber,
            [ModelBinder(typeof(DeliveryModeModelBinder))] CourseDeliveryMode deliveryMode) => Ok();

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

        [HttpGet("delete")]
        [RequireProviderContext]
        public async Task<IActionResult> DeleteUpload() =>
            await _mediator.SendAndMapResponse(
                new DeleteUpload.Query(),
                command => View(command));

        [HttpPost("delete")]
        [RequireProviderContext]
        public async Task<IActionResult> DeleteUpload(DeleteUpload.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(DeleteUploadSuccess)).WithProviderContext(_providerContextProvider.GetProviderContext())));

        [HttpGet("resolve/delete/success")]
        [RequireProviderContext]
        public IActionResult DeleteUploadSuccess() => View();

        [HttpGet("check-publish")]
        public IActionResult CheckAndPublish() => Ok();

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<CsvCourseRow>("courses-template.csv", Enumerable.Empty<CsvCourseRow>());

        [HttpGet("formatting")]
        public IActionResult Formatting() => View();

        [HttpGet("resolve/{rowNumber}/delete")]
        [RequireProviderContext]
        public async Task<IActionResult> DeleteRow(DeleteRow.Query request)
        {
            return await _mediator.SendAndMapResponse(
                request,
                result => result.Match<IActionResult>(
                    _ => NotFound(),
                    course => View(course)));
        }

        [HttpPost("resolve/{rowNumber}/delete")]
        [RequireProviderContext]
        public async Task<IActionResult> DeleteRow([FromRoute] int rowNumber, DeleteRow.Command command)
        {
            command.Row = rowNumber;
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    _ => NotFound(),
                    success => success switch
                    {
                        DeleteRowResult.CourseRowDeletedHasMoreErrors => RedirectToAction(nameof(ResolveList)).WithProviderContext(_providerContextProvider.GetProviderContext()),
                        DeleteRowResult.CourseRowDeletedHasNoMoreErrors => RedirectToAction(nameof(CheckAndPublish)).WithProviderContext(_providerContextProvider.GetProviderContext()),
                        _ => throw new NotSupportedException($"Unknown value: '{success}'.")
                    }));
        }
    }
}
