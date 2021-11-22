using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification
{
    [Route("courses/choosequalification")]
    [RequireProviderContext]
    public class ChooseQualificationController : Controller
    {
        private readonly IMediator _mediator;

        public ChooseQualificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(Query query)
        {
            return await _mediator.SendAndMapResponse(
               query,
               response => View("ChooseQualification", response));
        }

    }
}
