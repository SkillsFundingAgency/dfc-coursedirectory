using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.Mvc;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using ErrorsWhatNext = Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Errors.WhatNext;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Providers
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
