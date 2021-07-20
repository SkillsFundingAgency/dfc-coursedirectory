using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.Mvc;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ErrorsWhatNext = Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Errors.WhatNext;

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

        [HttpGet("resolve")]
        [RequireProviderContext]
        public async Task<IActionResult> ResolveList() =>
            await _mediator.SendAndMapResponse(
                new ResolveList.Query(),
                result => result.Match<IActionResult>(
                    noErrors => RedirectToAction(nameof(CheckAndPublish)).WithProviderContext(_providerContextProvider.GetProviderContext()),
                    vm => View(vm)));

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
                            _ => throw new NotSupportedException($"Unknown delivery mode: '{command.DeliveryMode}'.")
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
        public async Task<IActionResult> ResolveRowDetails(
            [FromRoute] int rowNumber,
            [ModelBinder(typeof(DeliveryModeModelBinder))] CourseDeliveryMode deliveryMode)
        {
            var query = new ResolveRowDetails.Query()
            {
                DeliveryMode = deliveryMode,
                RowNumber = rowNumber
            };

            return await _mediator.SendAndMapResponse(
                query,
                errors => this.ViewFromErrors(errors, statusCode: System.Net.HttpStatusCode.OK));
        }

        [HttpPost("resolve/{rowNumber}/details")]
        [RequireValidModelState(forKey: "deliveryMode")]
        public async Task<IActionResult> ResolveRowDetails(
            [FromRoute] int rowNumber,
            [ModelBinder(typeof(DeliveryModeModelBinder))] CourseDeliveryMode deliveryMode,
            ResolveRowDetails.Command command)
        {
            command.RowNumber = rowNumber;
            command.DeliveryMode = deliveryMode;

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
        public IActionResult DeleteUploadSuccess() => View();

        [HttpGet("download")]
        [RequireProviderContext]
        public async Task<IActionResult> Download() => await _mediator.SendAndMapResponse(
            new Download.Query(),
            result => new CsvResult<CsvCourseRow>(result.FileName, result.Rows));

        [HttpGet("resolve/{rowNumber}/course/delete/")]
        public async Task<IActionResult> DeleteRowGroup([FromRoute] int rowNumber) =>
            await _mediator.SendAndMapResponse(new DeleteRowGroup.Query() { RowNumber = rowNumber }, vm => View(vm));

        [HttpPost("resolve/{rowNumber}/course/delete/")]
        public async Task<IActionResult> DeleteRowGroup([FromRoute] int rowNumber, DeleteRowGroup.Command command)
        {
            command.RowNumber = rowNumber;
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    status => status switch
                    {
                        UploadStatus.ProcessedSuccessfully => RedirectToAction(nameof(CheckAndPublish))
                            .WithProviderContext(_providerContextProvider.GetProviderContext()),
                        _ => RedirectToAction(nameof(ResolveList))
                            .WithProviderContext(_providerContextProvider.GetProviderContext())
                    }));
        }

        [HttpGet("check-publish")]
        public IActionResult CheckAndPublish() => Ok();

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<CsvCourseRow>("courses-template.csv", Enumerable.Empty<CsvCourseRow>());

        [HttpGet("formatting")]
        public IActionResult Formatting() => View();

        [HttpGet("errors")]
        public async Task<IActionResult> Errors() =>
            await _mediator.SendAndMapResponse(
                new Errors.Query(),
                result => result.Match<IActionResult>(
                    noErrors => RedirectToAction(nameof(CheckAndPublish)).WithProviderContext(_providerContextProvider.GetProviderContext()),
                    vm => View(vm)));

        [HttpPost("errors")]
        public async Task<IActionResult> Errors(Errors.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => (command.WhatNext switch
                    {
                        ErrorsWhatNext.UploadNewFile => RedirectToAction(nameof(Index)),
                        ErrorsWhatNext.DeleteUpload => RedirectToAction(nameof(DeleteUpload)),
                        ErrorsWhatNext.ResolveOnScreen => RedirectToAction(nameof(ResolveList)),
                        _ => throw new NotSupportedException($"Unknown value: '{command.WhatNext}'.")
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));

        [HttpGet("resolve/{rowNumber}/details/delete")]
        [RequireProviderContext]
        public async Task<IActionResult> DeleteRow(DeleteRow.Query request) =>
            await _mediator.SendAndMapResponse(
                request,
                vm => View(vm));

        [HttpPost("resolve/{rowNumber}/details/delete")]
        [RequireProviderContext]
        public async Task<IActionResult> DeleteRow([FromRoute] int rowNumber, DeleteRow.Command command)
        {
            command.RowNumber = rowNumber;
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    uploadStatus => uploadStatus switch
                    {
                        UploadStatus.ProcessedSuccessfully => RedirectToAction(nameof(CheckAndPublish)).WithProviderContext(_providerContextProvider.GetProviderContext()),
                        _ => RedirectToAction(nameof(ResolveList)).WithProviderContext(_providerContextProvider.GetProviderContext())
                    }));
        }
    }
}
