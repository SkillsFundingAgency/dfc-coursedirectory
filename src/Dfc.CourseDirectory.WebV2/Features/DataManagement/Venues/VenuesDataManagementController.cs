using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Mvc;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues
{
    [Route("data-upload/venues")]
    [RequireProviderContext]
    [RequireFeatureFlag(FeatureFlags.DataManagement)]
    public class VenuesDataManagementController : Controller
    {
        private readonly IMediator _mediator;

        public VenuesDataManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download() => await _mediator.SendAndMapResponse(
            new Download.Query(),
            result => new CsvResult<VenueRow>(result.FileName, result.Rows));
    }
}
