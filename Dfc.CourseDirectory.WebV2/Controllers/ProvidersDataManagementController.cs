using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Filters;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.Mvc;
using Upload = Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Upload;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ErrorsWhatNext = Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Courses.Errors.WhatNext;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [Route("data-upload/providers")]
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
        public async Task<IActionResult> Index() =>
            await _mediator.SendAndMapResponse(new Upload.Query(), vm => View("Upload", vm));

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
    }
}
