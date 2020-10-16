using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.BulkUpload
{
    [RequiresProviderContext]
    [Route("bulk-upload")]
    public class BulkUploadController : Controller
    {
        private readonly IMediator _mediator;

        public BulkUploadController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("courses")]
        public IActionResult Courses()
        {
            return View();
        }

        [HttpGet("courses-formatting")]
        public IActionResult CoursesFormatting()
        {
            return View();
        }

        [HttpGet("apprenticeships")]
        public IActionResult Apprenticeships()
        {
            return View();
        }

        [HttpGet("apprenticeships-formatting")]
        public IActionResult ApprenticeshipsFormatting()
        {
            return View();
        }

        [HttpGet("regions")]
        public IActionResult Regions()
        {
            return View();
        }

        [HttpGet("/BulkUpload/PublishYourFile")]
        public async Task<IActionResult> CoursesPublishFile(ProviderContext providerContext) =>
            await _mediator.SendAndMapResponse(new CoursesPublishFile.Query{ProviderId = providerContext.ProviderInfo.ProviderId}, vm => View(vm));
    }
}