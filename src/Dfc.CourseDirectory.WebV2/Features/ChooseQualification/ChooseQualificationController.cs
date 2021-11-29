using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification
{
    [Route("courses/choose-qualification")]
    public class ChooseQualificationController : Controller
    {
        private readonly IMediator _mediator;

        public ChooseQualificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(SearchQuery query)
        {
            return await _mediator.SendAndMapResponse(query,
              response => response.Match<IActionResult>(
                errors => this.ViewFromErrors("ChooseQualification", errors),
                success => View("ChooseQualification", success)));
        }


        [HttpGet("")]
        public async Task<IActionResult> Get(Query query)
        {
            return await _mediator.SendAndMapResponse(
                query,
                response => View("ChooseQualification", response));
        }

        [HttpGet("clearfilters")]
        public async Task<IActionResult> ClearFilters(SearchQuery query)
        {
            query.AwardingOrganisation = null;
            query.NotionalNVQLevelv2 = null;

            return await _mediator.SendAndMapResponse(query,
              response => response.Match<IActionResult>(
                errors => this.ViewFromErrors("ChooseQualification", errors),
                success => View("ChooseQualification", success)));
        }
    }
}
