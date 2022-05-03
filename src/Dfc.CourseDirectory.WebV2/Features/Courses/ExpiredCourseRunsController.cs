using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Features.Courses
{
    [Route("courses/expired")]
    [RequireProviderContext]
    public class ExpiredCourseRunsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ProviderContext _providerContext;

        public ExpiredCourseRunsController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();

        }

        [HttpGet("")]
        public async Task<IActionResult> Index() =>
            await _mediator.SendAndMapResponse(new ExpiredCourseRuns.Query(), vm => View("ExpiredCourseRuns", vm));

        [HttpPost]
        public async Task<IActionResult> Update(Guid[] selectedCourses)
        {

            var query = new ExpiredCourseRuns.SelectedQuery();
            query.CheckedRows = selectedCourses.ToArray();
            return await _mediator.SendAndMapResponse(query, vm => View("SelectedExpiredCourseRuns", vm));
            
        }

        [HttpPost("updated")]
        [RequireProviderContext]
        public async Task<IActionResult> UpdatedCourses(string year, string month, string day, Guid[] selectedRows)
        {
            
            DateTime startDate = new DateTime(int.Parse(year) , int.Parse(month), int.Parse(day));
            var query = new ExpiredCourseRuns.NewStartDateQuery();
            query.NewStartDate = startDate;
            query.SelectedCourses = selectedRows.ToArray();

            var returnUrl = $"/courses/expired/?providerId={_providerContext.ProviderInfo.ProviderId}";
            // return await _mediator.SendAndMapResponse(query, vm => View("updated", vm ));
            return await _mediator.SendAndMapResponse(
                query,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors("SelectedExpiredCourseRuns", errors).WithViewData("ReturnUrl", returnUrl),
                    Success => RedirectToAction(nameof(Updated))
                        .WithProviderContext(_providerContext)));

        }

    
        [HttpGet("/courses/expired/updated")]
        [RequireProviderContext]
        public IActionResult Updated()
        {
            return View();

        }

    }
    }



