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

        [HttpGet("errors")]
        public IActionResult Errors() => Ok();

        [HttpGet("check-publish")]
        public IActionResult CheckAndPublish() => Ok();

        [HttpGet("template")]
        public IActionResult Template() =>
           new CsvResult<CsvCourseRow>("courses-template.csv", Enumerable.Empty<CsvCourseRow>());

        [HttpGet("formatting")]
        public IActionResult Formatting() => View();

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
                        ErrorsWhatNext.UploadNewFile => RedirectToAction(nameof(Index)),
                        _ => throw new NotSupportedException($"Unknown value: '{command.WhatNext}'.")
                    }).WithProviderContext(_providerContextProvider.GetProviderContext())));
    }
}
