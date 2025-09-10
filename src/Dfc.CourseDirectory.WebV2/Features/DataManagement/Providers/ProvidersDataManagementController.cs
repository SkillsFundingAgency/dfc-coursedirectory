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

        [HttpPost("upload")]
        public IActionResult Upload(Upload.Command command)
        {
            var file = Request.Form.Files?.GetFile(nameof(command.File));

            if (file != null && !file.FileName.Contains("Active_Providers"))
            {
                ModelState.AddModelError(nameof(command.File), "The file name doesn't contain");

            }
            return View(command);
        }
    }
}
