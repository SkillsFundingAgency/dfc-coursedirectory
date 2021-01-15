using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.BulkUpload
{
    [RequireProviderContext]
    [Route("bulk-upload")]
    public class BulkUploadController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ProviderContext _providerContext;

        public BulkUploadController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();
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
        public async Task<IActionResult> CoursesPublishFile() =>
            await _mediator.SendAndMapResponse(
                new CoursesPublishFile.Query
                {
                    ProviderId = _providerContext.ProviderInfo.ProviderId
                },
                vm => View(vm));
    }
}
