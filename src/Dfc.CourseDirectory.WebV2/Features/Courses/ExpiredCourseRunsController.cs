using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Features.Courses.ExpiredCourseRuns;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;

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
            if (!selectedCourses.Any())
            {
                return await _mediator.SendAndMapResponse(new ExpiredCourseRuns.Query(), vm => this.ViewFromErrors("ExpiredCourseRuns", new ModelWithErrors<ViewModel>(vm,
                    new ValidationResult()
                    {
                        
                        Errors = { new ValidationFailure("CheckedRows", "Select a course to update") }
                    })));
            }
            var query = new ExpiredCourseRuns.SelectedQuery { SelectedCourses = selectedCourses.ToArray() };
            return await _mediator.SendAndMapResponse(query, vm => View("SelectedExpiredCourseRuns", vm));
        }

        [HttpPost("updated")]
        [RequireProviderContext]
        public async Task<IActionResult> UpdatedCourses(DateTime? NewStartDate, Guid[] selectedCourses)
        {
            
            
            var query = new ExpiredCourseRuns.NewStartDateQuery();
            query.NewStartDate = NewStartDate;
          
            query.SelectedCourses =selectedCourses.ToArray();

            var returnUrl = $"/courses/expired/?providerId={_providerContext.ProviderInfo.ProviderId}";
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



