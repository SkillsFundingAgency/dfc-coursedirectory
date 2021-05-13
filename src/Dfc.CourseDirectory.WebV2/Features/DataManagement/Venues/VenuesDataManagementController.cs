using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Mvc;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FormFlow;
using ErrorsWhatNext = Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.Errors.WhatNext;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues
{
    [Route("data-upload/venues")]
    [RequireFeatureFlag(FeatureFlags.VenuesDataManagement)]
    public class VenuesDataManagementController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;

        public VenuesDataManagementController(
            IMediator mediator,
            IProviderContextProvider providerContextProvider,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _mediator = mediator;
            _providerContextProvider = providerContextProvider;
            _journeyInstanceProvider = journeyInstanceProvider;
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
        public async Task<IActionResult> Errors() =>
            await _mediator.SendAndMapResponse(
                new Errors.Query(),
                result => result.Match<IActionResult>(
                    noErrors => RedirectToAction(nameof(CheckAndPublish)).WithProviderContext(_providerContextProvider.GetProviderContext()),
                    vm => View(vm)));

        [HttpPost("errors")]
        [RequireProviderContext]
        public async Task<IActionResult> Errors(Errors.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => (command.WhatNext switch
                    {
                        ErrorsWhatNext.ResolveOnScreen => RedirectToAction(nameof(Resolve)),
                        ErrorsWhatNext.UploadNewFile => RedirectToAction(nameof(Index)),
                        ErrorsWhatNext.DeleteUpload => RedirectToAction(nameof(DeleteUpload)),
                        _ => throw new NotSupportedException($"Unknown value: '{command.WhatNext}'.")
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));

        [HttpGet("resolve")]
        [RequireProviderContext]
        public IActionResult Resolve() => Ok();

        [HttpGet("resolve/{rowNumber}")]
        public async Task<IActionResult> ResolveRowErrors(ResolveRowErrors.Query query) =>
            await _mediator.SendAndMapResponse(
                query,
                result => result.Match<IActionResult>(
                    notFound => NotFound(),
                    vm => this.ViewFromErrors(vm, statusCode: System.Net.HttpStatusCode.OK)));

        [HttpPost("resolve/{rowNumber}")]
        public async Task<IActionResult> ResolveRowErrors([FromRoute] int rowNumber, ResolveRowErrors.Command command)
        {
            command.RowNumber = rowNumber;

            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    uploadStatus => (uploadStatus switch
                    {
                        UploadStatus.ProcessedSuccessfully => RedirectToAction(nameof(CheckAndPublish)),
                        _ => RedirectToAction(nameof(Resolve))
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("delete")]
        [RequireProviderContext]
        public IActionResult DeleteUpload() => Ok();

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
        [RequireProviderContext]
        [JourneyMetadata("PublishVenueUpload", typeof(PublishJourneyModel), appendUniqueKey: false, requestDataKeys: "providerId?")]
        public async Task<IActionResult> CheckAndPublish(CheckAndPublish.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    publishResult => publishResult.Status == Core.DataManagement.PublishResultStatus.Success ?
                        RedirectToAction(nameof(Published)).WithProviderContext(_providerContextProvider.GetProviderContext()) :
                        RedirectToAction(nameof(Errors)).WithProviderContext(_providerContextProvider.GetProviderContext())));

        [HttpGet("success")]
        [RequireProviderContext]
        [RequireJourneyInstance]
        [JourneyMetadata("PublishVenueUpload", typeof(PublishJourneyModel), appendUniqueKey: false, requestDataKeys: "providerId?")]
        public async Task<IActionResult> Published() => await _mediator.SendAndMapResponse(new Published.Query(), vm => View(vm));

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<CsvVenueRow>("venues-template.csv", Enumerable.Empty<CsvVenueRow>());

        [HttpGet("formatting")]
        [RequireProviderContext]
        public IActionResult Formatting()
        {
            return View();
        }
    }
}
