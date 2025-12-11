using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Filters;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.Mvc;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IProviderContextProvider _providerContextProvider;

        public ProvidersDataManagementController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContextProvider = providerContextProvider;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View(new Home.ViewModel());
        }

        [HttpPost]
        public IActionResult Index(Home.ViewModel vm)
        {
            switch(vm.ProviderUploadType) {
                case Home.ProviderUploadType.Inactive:
                    return RedirectToAction("InactiveProviders");
                default:
                    return RedirectToAction("ActiveProviders");
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> ActiveProviders() =>
           await _mediator.SendAndMapResponse(new Upload.Query(), vm => View("Upload", vm));

        [HttpGet("inactive")]
        public async Task<IActionResult> InactiveProviders() =>
            await _mediator.SendAndMapResponse(new Upload.Query(), vm => View("UploadInactive", vm));

        [HttpPost("upload")]
        public async  Task<IActionResult>  Upload(Upload.Command command)
        {
            var file = Request.Form.Files?.GetFile(nameof(command.File));

            if (file != null && !file.FileName.Contains("Active_Providers", StringComparison.CurrentCultureIgnoreCase))
            {
                ModelState.AddModelError(nameof(command.File), "The file name doesn't contain Active_Providers");
                return View();

            }
            return await _mediator.SendAndMapResponse(
                new Upload.Command()
                {
                    File = file,
                },
                response => response.Match<IActionResult>(
                    errors =>
                    {
                        ViewBag.MissingHeaders = errors.MissingHeaders;
                        return this.ViewFromErrors(errors);
                    },
                    success =>  View()));
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
