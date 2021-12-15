using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Flurl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification
{
    [Route("courses/choose-qualification")]
    [RequireProviderContext]
    public class ChooseQualificationController : Controller, IMptxController<FlowModel>
    {
        private readonly IMediator _mediator;
        public MptxInstanceContext<FlowModel> Flow { get; set; }
        private readonly ProviderContext _providerContext;

        public ChooseQualificationController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [StartsMptx]
        [HttpGet("search")]
        public async Task<IActionResult> Search(SearchQuery query, [FromServices] MptxManager mptxManager)
        {
            Flow = mptxManager.CreateInstance(new FlowModel());
            var returnUrl = new Url(Url.Action(nameof(CourseSelected)))
                .WithMptxInstanceId(Flow.InstanceId)
                .WithProviderContext(_providerContext);
            ViewData["ReturnUrl"] = returnUrl.ToString();

            return await _mediator.SendAndMapResponse(query,
              response => response.Match<IActionResult>(
                errors => this.ViewFromErrors(nameof(ChooseQualification), errors),
                success => View(nameof(ChooseQualification), success)));
        }

        [HttpGet("")]
        public async Task<IActionResult> ChooseQualification()
        {
            var query = new Query();
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [HttpGet("clearfilters")]
        public IActionResult ClearFilters(SearchQuery query) => RedirectToAction(nameof(Search), new { SearchTerm = query.SearchTerm });

        [HttpGet("add")]
        [MptxAction]
        public IActionResult CourseDescription()
        {
            return View();
        }

        [HttpPost("add")]
        [MptxAction]
        public async Task<IActionResult> CourseDescription(CourseDescription.Command command)
        {
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(nameof(CourseDescription), errors),
                    success => RedirectToAction(nameof(DeliveryMethod))
                        .WithProviderContext(_providerContext)
                        .WithMptxInstanceId(Flow)));
        }

        [MptxAction]
        [HttpGet("course-selected")]
        public async Task<IActionResult> CourseSelected(SelectCourse course)
        {
            await _mediator.Send(new CourseSelected.Command() { LarsCode = course.LearnAimRef });
            return RedirectToAction(nameof(CourseDescription))
                .WithMptxInstanceId(Flow.InstanceId)
                .WithProviderContext(_providerContext);
        }

        [HttpGet("deliverymethod")]
        [MptxAction]
        public IActionResult DeliveryMethod()
        {
            return View();
        }
    }
}


