using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Filters;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.Core.MultiPageTransaction;
using Flurl;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.WebV2.ViewComponents.ChooseQualification;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [Route("courses")]
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
        [RequireProviderContext]
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
                success => View("ChooseQualification", success)));
        }


        [RequireProviderContext]
        [HttpGet("choose-qualification")]
        public async Task<IActionResult> ChooseQualification()
        {
            var returnUrl = new Url(Url.Action(nameof(Search)))
                .WithProviderContext(_providerContext);
            var query = new Query { ProviderId = _providerContext.ProviderInfo.ProviderId.ToString() };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response).WithViewData("SearchUrl", returnUrl));
        }

        [HttpGet("clearfilters")]
        public IActionResult ClearFilters(SearchQuery query) => RedirectToAction(nameof(Search), new { query.SearchTerm }).WithProviderContext(_providerContext);

        [RequireProviderContext]
        [HttpGet("add")]
        [MptxAction]
        public async Task<IActionResult> CourseDescription(ViewComponents.ChooseQualification.CourseDescription.Query query)
        {
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [RequireProviderContext]
        [HttpPost("add")]
        [MptxAction]
        public async Task<IActionResult> CourseDescription(ViewComponents.ChooseQualification.CourseDescription.Command command)
        {
            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(nameof(CourseDescription), errors),
                    success => RedirectToAction(nameof(SelectDeliveryMode))
                        .WithProviderContext(_providerContext)
                        .WithMptxInstanceId(Flow)));
        }

        [RequireProviderContext]
        [MptxAction]
        [HttpGet("course-selected")]

        public async Task<IActionResult> CourseSelected(SelectCourse course)
        {
            await _mediator.Send(new ViewComponents.ChooseQualification.CourseSelected.Command() { LarsCode = course.LearnAimRef, CourseName = course.CourseName });
            return RedirectToAction(nameof(CourseDescription))
                .WithMptxInstanceId(Flow.InstanceId)
                .WithProviderContext(_providerContext);
        }

        [RequireProviderContext]
        [HttpGet("add/delivery")]
        [MptxAction]
        public async Task<IActionResult> SelectDeliveryMode(ViewComponents.ChooseQualification.DeliveryMode.Query query)
        {
            return await _mediator.SendAndMapResponse(
                query,
                result => result.Match<IActionResult>(
                    notFound => NotFound(),
                    command => View(command)));
        }

        [RequireProviderContext]
        [HttpPost("add/delivery")]
        [MptxAction]
        public async Task<IActionResult> SelectDeliveryMode(ViewComponents.ChooseQualification.DeliveryMode.Command command)
        {
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    notFound => NotFound(),
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(CourseRun), new
                    {
                        deliveryMode = command.DeliveryMode switch
                        {
                            CourseDeliveryMode.BlendedLearning => "blended",
                            CourseDeliveryMode.ClassroomBased => "classroom",
                            CourseDeliveryMode.Online => "online",
                            CourseDeliveryMode.WorkBased => "work",
                            _ => throw new NotSupportedException($"Unknown delivery mode: '{command.DeliveryMode}'.")
                        }
                    }).WithMptxInstanceId(Flow.InstanceId)
                        .WithProviderContext(_providerContext)));

        }

        [RequireProviderContext]
        [HttpGet("add-courserun")]
        [MptxAction]
        [RequireValidModelState]
        public async Task<IActionResult> CourseRun([ModelBinder(typeof(DeliveryModeModelBinder))] CourseDeliveryMode deliveryMode)
        {
            var query = new ViewComponents.ChooseQualification.CourseRun.Query
            {
                DeliveryMode = deliveryMode,
            };
            var returnUrl = $"add/delivery?{Constants.InstanceIdQueryParameter}={Flow.InstanceId}&providerId={_providerContext.ProviderInfo.ProviderId}";

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return await _mediator.SendAndMapResponse(
                query,
                result => View(result).WithViewData("ReturnUrl", returnUrl));
        }

        [RequireProviderContext]
        [HttpPost("add-courserun")]
        [MptxAction]
        public async Task<IActionResult> CourseRun(ViewComponents.ChooseQualification.CourseRun.Command command)
        {
            var returnUrl = $"add/delivery?{Constants.InstanceIdQueryParameter}={Flow.InstanceId}&providerId={_providerContext.ProviderInfo.ProviderId}";
            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors).WithViewData("ReturnUrl", returnUrl),
                    Success => RedirectToAction(nameof(CheckAndPublish))
                        .WithMptxInstanceId(Flow.InstanceId)
                        .WithProviderContext(_providerContext)));
        }

        [RequireProviderContext]
        [HttpGet("add/check-and-publish")]
        [MptxAction]
        public IActionResult CheckAndPublish()
        {
            return View();
        }

    }
}


