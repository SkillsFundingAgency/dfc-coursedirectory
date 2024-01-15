﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.Mvc;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;
using ErrorsWhatNext = Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Errors.WhatNext;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses
{
    [Route("data-upload/courses")]
    [RequireFeatureFlag(FeatureFlags.CoursesDataManagement)]
    [RequireProviderContext]
    [RestrictProviderTypes(ProviderType.FE | ProviderType.NonLARS)]
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

        [HttpGet("nonlars")]
        public async Task<IActionResult> NonLars() =>
            await _mediator.SendAndMapResponse(new Upload.Query(), vm => View("UploadNonLars", vm));

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(Upload.Command command)
        {
            var file = Request.Form.Files?.GetFile(nameof(command.File));

            return await _mediator.SendAndMapResponse(
                new Upload.Command()
                {
                    File = file,
                    IsNonLars = false
                },
                response => response.Match<IActionResult>(
                    errors =>
                    {
                        ViewBag.MissingHeaders = errors.MissingHeaders;
                        ViewBag.MissingLearnAimRefs = errors.MissingLearnAimRefs;
                        ViewBag.InvalidLearnAimRefs = errors.InvalidLearnAimRefs;
                        ViewBag.ExpiredLearnAimRefs = errors.ExpiredLearnAimRefs;

                        if (errors.MissingLearnAimRefs.Count > 0 ||
                            errors.InvalidLearnAimRefs.Count > 0 ||
                            errors.ExpiredLearnAimRefs.Count > 0)
                        {
                            ModelState.AddModelError(nameof(command.File), "The file contains errors and could not be uploaded");
                        }

                        return this.ViewFromErrors(errors);
                    },
                    success => RedirectToAction(nameof(InProgress),false).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpPost("uploadnonlars")]
        public async Task<IActionResult> UploadNonLars(Upload.Command command)
        {
            var file = Request.Form.Files?.GetFile(nameof(command.File));

            return await _mediator.SendAndMapResponse(
                new Upload.Command()
                {
                    File = file,
                    IsNonLars = true
                },
                response => response.Match<IActionResult>(
                    errors =>
                    {
                        ViewBag.MissingHeaders = errors.MissingHeaders;
                        return this.ViewFromErrors(errors);
                    },
                    success => RedirectToAction(nameof(InProgress), true).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("resolve")]
        public async Task<IActionResult> ResolveList(bool isNonLars) =>
            await _mediator.SendAndMapResponse(
                new ResolveList.Query() { IsNonLars = isNonLars },
                result => result.Match<IActionResult>(
                    noErrors => RedirectToAction(nameof(CheckAndPublish), isNonLars).WithProviderContext(_providerContextProvider.GetProviderContext()),
                    vm => View(vm)));

        [HttpGet("resolve/{rowNumber}/delivery")]
        public async Task<IActionResult> ResolveRowDeliveryMode(ResolveRowDeliveryMode.Query query) =>
            await _mediator.SendAndMapResponse(
                query,
                result => result.Match<IActionResult>(
                    notFound => NotFound(),
                    command => View(command)));

        [HttpPost("resolve/{rowNumber}/delivery")]
        public async Task<IActionResult> ResolveRowDeliveryMode([FromRoute] int rowNumber, [FromRoute] bool isNonLars, ResolveRowDeliveryMode.Command command)
        {
            command.RowNumber = rowNumber;
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    notFound => NotFound(),
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ResolveRowDetails), new
                    {
                        rowNumber,
                        isNonLars,
                        deliveryMode = command.DeliveryMode switch
                        {
                            CourseDeliveryMode.BlendedLearning => "blended",
                            CourseDeliveryMode.ClassroomBased => "classroom",
                            CourseDeliveryMode.Online => "online",
                            CourseDeliveryMode.WorkBased => "work",
                            _ => throw new NotSupportedException($"Unknown delivery mode: '{command.DeliveryMode}'.")
                        }
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("resolve/{rowNumber}/description")]
        public async Task<IActionResult> ResolveRowDescription(ResolveRowDescription.Query query)
        {
            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";
            return await _mediator.SendAndMapResponse(query, errors => this.ViewFromErrors(errors, statusCode: System.Net.HttpStatusCode.OK));
        }

        [HttpPost("resolve/{rowNumber}/description")]
        public async Task<IActionResult> ResolveRowDescription([FromRoute] int rowNumber,[FromRoute] bool isNonLars, ResolveRowDescription.Command command)
        {
            command.RowNumber = rowNumber;
            command.IsNonLars = isNonLars;

            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    uploadStatus => (uploadStatus switch
                    {
                        UploadStatus.ProcessedSuccessfully => RedirectToAction(nameof(CheckAndPublish), isNonLars),
                        _ => RedirectToAction(nameof(ResolveList), isNonLars)
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("resolve/{rowNumber}/details")]
        [RequireValidModelState]
        public async Task<IActionResult> ResolveRowDetails(
            [FromRoute] int rowNumber,
            [FromRoute] bool isNonLars,
            [ModelBinder(typeof(DeliveryModeModelBinder))] CourseDeliveryMode deliveryMode)
        {
            var query = new ResolveRowDetails.Query()
            {
                DeliveryMode = deliveryMode,
                RowNumber = rowNumber,
                IsNonLars = isNonLars
            };

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return await _mediator.SendAndMapResponse(
                query,
                errors => this.ViewFromErrors(errors, statusCode: System.Net.HttpStatusCode.OK));
        }

        [HttpPost("resolve/{rowNumber}/details")]
        [RequireValidModelState(forKey: "deliveryMode")]
        public async Task<IActionResult> ResolveRowDetails(
            [FromRoute] int rowNumber,
            [FromRoute] bool isNonLars,
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
                        UploadStatus.ProcessedSuccessfully => RedirectToAction(nameof(CheckAndPublish), isNonLars),
                        _ => RedirectToAction(nameof(ResolveList),isNonLars)
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));
        }

        [HttpGet("in-progress")]
        public async Task<IActionResult> InProgress(bool isNonLars) => await _mediator.SendAndMapResponse(
            new InProgress.Query() { IsNonLars = isNonLars},
            result => result.Match(
                notFound => NotFound(),
                status => status switch
                {
                    UploadStatus.ProcessedSuccessfully => (IActionResult)RedirectToAction(nameof(CheckAndPublish),isNonLars)
                        .WithProviderContext(_providerContextProvider.GetProviderContext()),
                    UploadStatus.ProcessedWithErrors => RedirectToAction(nameof(Errors), isNonLars)
                        .WithProviderContext(_providerContextProvider.GetProviderContext()),
                    _ => View(status)
                }));

        [HttpGet("delete")]
        public async Task<IActionResult> DeleteUpload(bool isNonLars) =>
            await _mediator.SendAndMapResponse(
                new DeleteUpload.Query() { IsNonLars = isNonLars},
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

        [HttpGet("download")]
        public async Task<IActionResult> Download() => await _mediator.SendAndMapResponse(
            new Download.Query() { IsNonLars = false},
            result => new CsvResult<CsvCourseRow>(result.FileName, result.Rows));

        [HttpGet("downloadnonlars")]
        public async Task<IActionResult> DownloadNonLars() => await _mediator.SendAndMapResponse(
           new Download.Query() { IsNonLars = true },
           result => new CsvResult<CsvNonLarsCourseRow>(result.FileName, result.NonLarsRows));

        [HttpGet("resolve/{rowNumber}/course/delete/")]
        public async Task<IActionResult> DeleteRowGroup([FromRoute] int rowNumber) =>
            await _mediator.SendAndMapResponse(new DeleteRowGroup.Query() { RowNumber = rowNumber }, vm => View(vm));

        [HttpPost("resolve/{rowNumber}/course/delete/")]
        public async Task<IActionResult> DeleteRowGroup([FromRoute] int rowNumber, [FromRoute] bool isNonLars, DeleteRowGroup.Command command)
        {
            command.RowNumber = rowNumber;
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    status => status switch
                    {
                        UploadStatus.ProcessedSuccessfully => RedirectToAction(nameof(CheckAndPublish),false)
                            .WithProviderContext(_providerContextProvider.GetProviderContext()),
                        _ => RedirectToAction(nameof(ResolveList), isNonLars)
                            .WithProviderContext(_providerContextProvider.GetProviderContext())
                    }));
        }

        [HttpGet("check-publish")]
        public async Task<IActionResult> CheckAndPublish(bool isNonLars)
        {
            var query = new CheckAndPublish.Query() { IsNonLars = isNonLars};

            return await _mediator.SendAndMapResponse(
                query,
                result => result.Match<IActionResult>(
                    hasErrors => RedirectToAction(nameof(Errors),isNonLars).WithProviderContext(_providerContextProvider.GetProviderContext()),
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
        public async Task<IActionResult> Published() 
        {
            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return await _mediator.SendAndMapResponse(new Published.Query(), vm => View(vm)); 
        }

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<CsvCourseRow>("courses-template.csv", Enumerable.Empty<CsvCourseRow>());

        [HttpGet("templatenonlars")]
        public IActionResult TemplateNonLars() =>
           new CsvResult<CsvNonLarsCourseRow>("nonlars-courses-template.csv", Enumerable.Empty<CsvNonLarsCourseRow>());

        [HttpGet("formatting")]
        public IActionResult Formatting()
        {
            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return View(); 
        }

        [HttpGet("nonlarsformatting")]
        public IActionResult NonLarsFormatting()
        {
            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return View();
        }
        [HttpGet("download-errors")]
        public async Task<IActionResult> DownloadErrors(bool isNonLars) => await _mediator.SendAndMapResponse(
            new DownloadErrors.Query(),
            result => new CsvResult<CsvCourseRowWithErrors>(result.FileName, result.Rows));

        [HttpGet("errors")]
        public async Task<IActionResult> Errors(bool isNonLars) =>
            await _mediator.SendAndMapResponse(
                new Errors.Query() { IsNonLars = isNonLars},
                result => result.Match<IActionResult>(
                    noErrors => RedirectToAction(nameof(CheckAndPublish),isNonLars).WithProviderContext(_providerContextProvider.GetProviderContext()),
                    vm => View(vm)));

        [HttpPost("errors")]
        public async Task<IActionResult> Errors(Errors.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => (command.WhatNext switch
                    {
                        ErrorsWhatNext.UploadNewFile => command.IsNonLars ? RedirectToAction(nameof(NonLars)) : RedirectToAction(nameof(Index)),
                        ErrorsWhatNext.DeleteUpload => RedirectToAction(nameof(DeleteUpload),command.IsNonLars),
                        ErrorsWhatNext.ResolveOnScreen => RedirectToAction(nameof(ResolveList), command.IsNonLars),
                        _ => throw new NotSupportedException($"Unknown value: '{command.WhatNext}'.")
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));

        [HttpGet("resolve/{rowNumber}/details/delete")]
        public async Task<IActionResult> DeleteRow(DeleteRow.Query request) =>
            await _mediator.SendAndMapResponse(
                request,
                vm => View(vm));

        [HttpPost("resolve/{rowNumber}/details/delete")]
        public async Task<IActionResult> DeleteRow([FromRoute] int rowNumber, [FromRoute] bool isNonLars, DeleteRow.Command command)
        {
            command.RowNumber = rowNumber;
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    uploadStatus => uploadStatus switch
                    {
                        UploadStatus.ProcessedSuccessfully => RedirectToAction(nameof(CheckAndPublish), isNonLars).WithProviderContext(_providerContextProvider.GetProviderContext()),
                        _ => RedirectToAction(nameof(ResolveList), isNonLars).WithProviderContext(_providerContextProvider.GetProviderContext())
                    }));
        }
    }
}
