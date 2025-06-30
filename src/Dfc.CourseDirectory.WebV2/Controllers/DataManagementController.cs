using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Attributes;
namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [RequireFeatureFlag(FeatureFlags.DataManagement)]
    [Route("data-upload")]
    public class DataManagementController : Controller
    {
        private readonly IMediator _mediator;

        public DataManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        [RequireProviderContext]
        public async Task<IActionResult> Dashboard() =>
            await _mediator.SendAndMapResponse(new ViewModels.DataManagement.Venues.Dashboard.Query(), vm => View("~/Views/DataManagement/Dashboard.cshtml", vm));

        [HttpGet("regions")]
        public IActionResult Regions() => View("~/Views/DataManagement/Regions.cshtml");
    }
}
