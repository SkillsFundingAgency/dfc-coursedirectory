using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Courses
{
    [Route("courses/expired")]
    [RequireProviderContext]
    public class ExpiredCourseRunsController : Controller
    {
        private readonly IMediator _mediator;

        public ExpiredCourseRunsController(IMediator mediator)
        {
            _mediator = mediator;
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

        [HttpPost ("updated")]
        public async Task<IActionResult> UpdatedCourses(string day, string month, string year, Guid [] selectedRows)
        {
            DateTime startDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
            var query = new ExpiredCourseRuns.NewStartDateQuery();
            query.NewStartDate = startDate;
            query.SelectedCourses = selectedRows.ToArray();

            return await _mediator.SendAndMapResponse(query, vm => View("updated", vm));
          
        }
       

    }

}

