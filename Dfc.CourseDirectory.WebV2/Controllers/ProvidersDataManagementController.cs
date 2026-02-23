using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Filters;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Home;
using Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Upload;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using OneOf.Types;
using ErrorsWhatNext = Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Courses.Errors.WhatNext;
using Home = Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Home;
using Upload = Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Upload;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [Route("data-upload/providers")]
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    public class ProvidersDataManagementController : Controller
    {
        private readonly IMediator _mediator;

        public ProvidersDataManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public IActionResult Index(ProviderUploadType? providerUploadType )
        {
            return RedirectToAction("ActiveProviders");
        }

        [HttpPost]
        public IActionResult Index(Home.ViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            switch(vm.ProviderUploadType) {
                case Home.ProviderUploadType.Inactive:
                    return RedirectToAction("InactiveProviders");
                default:
                    return RedirectToAction("ActiveProviders");
            }
        }


        [HttpGet("active-providers-in-progress/{providerUploadId}")]
        public async Task<IActionResult> InProgress([FromRoute] Guid providerUploadId) => await _mediator.SendAndMapResponse(
            new ViewModels.DataManagement.Providers.InProgress.Query() { ProviderUploadId = providerUploadId },
            result => result.Match(
                notFound => NotFound(),
                status => status switch
                {
                    UploadStatus.Published => (IActionResult)RedirectToAction(actionName: nameof(Result), routeValues: new { providerUploadId }),
                    UploadStatus.ProcessedWithErrors => View("Error"),
                    _ => View(new ViewModels.DataManagement.Providers.InProgress.ViewModel { UploadStatus = status, ProviderUploadId= providerUploadId})
                }));

        [HttpGet("upload-active-providers-summary/{providerUploadId}")]
        public  async Task<IActionResult> Result([FromRoute] Guid providerUploadId) =>    await _mediator.SendAndMapResponse(
            new ViewModels.DataManagement.Providers.Result.Query() { ProviderUploadId = providerUploadId },
            result => result.Match(
                notFound => NotFound(),
                resultSummary => (IActionResult) View(new ViewModels.DataManagement.Providers.Result.ViewModel { ProviderUploadId = providerUploadId, UploadResultSummary = resultSummary })
                ));

        [HttpGet("active-report-upload")]
        public async Task<IActionResult> ActiveProviders() =>
           await _mediator.SendAndMapResponse(new Upload.Query(), vm => View("Upload", vm));

        [HttpGet("inactive")]
        public async Task<IActionResult> InactiveProviders() =>
            await _mediator.SendAndMapResponse(new Upload.Query(), vm => View("UploadInactive", vm));

        [HttpPost("active-report-upload")]
        public async  Task<IActionResult>  Upload(Upload.Command command)
        {
            var file = Request.Form.Files?.GetFile(nameof(command.File));

            if (file != null && !file.FileName.Contains("Active_Providers", StringComparison.CurrentCultureIgnoreCase))
            {
                ModelState.AddModelError(nameof(command.File), "The selected file name must include 'active_providers'");
                return View();

            }
            return await _mediator.SendAndMapResponse(
                new Upload.Command()
                {
                    File = file,
                    Duration = command.Duration
                },
                response => response.Match<IActionResult>(
                    errors =>
                    {
                        ViewBag.MissingHeaders = errors.MissingHeaders;
                        return this.ViewFromErrors(errors);
                    },
                    success =>success.UploadSucceededResult == UploadSucceededResult.ProcessingCompletedWithErrors ? View ("Error"): (IActionResult) RedirectToAction(actionName: nameof(InProgress), routeValues: new { success.ProviderUploadId})));
        }

        [HttpPost("uploadinactive")]
        public async Task<IActionResult> UploadInactive(Upload.Command command)
        {
            var file = Request.Form.Files?.GetFile(nameof(command.File));

            if (file != null && !file.FileName.Contains("Inactive_Providers", StringComparison.CurrentCultureIgnoreCase))
            {
                ModelState.AddModelError(nameof(command.File), "The file name doesn't contain Inactive_Providers");
                return View();

            }
            return await _mediator.SendAndMapResponse(
                new Upload.Command()
                {
                    File = file,
                    InactiveProviders = true,
                },
                response => response.Match<IActionResult>(
                    errors =>
                    {
                        ViewBag.MissingHeaders = errors.MissingHeaders;
                        return this.ViewFromErrors(errors);
                    },
                    success => View()));
        }
    }
}
