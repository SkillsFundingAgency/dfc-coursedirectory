using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel
{
    [Route("t-levels/{tLevelId}")]
    [RestrictProviderTypes(ProviderType.TLevels)]
    [AuthorizeTLevel]
    public class ViewTLevelController : Controller
    {
        private readonly IMediator _mediator;

        public ViewTLevelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromRoute] ViewTLevel.Query query)
        {
            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            string commonurl = "find-a-course/tdetails?tlevelId=";
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + commonurl;

            return await _mediator.SendAndMapResponse(
                query,
                r => r.Match<IActionResult>(
                    _ => NotFound(),
                    vm => View("ViewTLevel", vm)));
        }
    }
}
