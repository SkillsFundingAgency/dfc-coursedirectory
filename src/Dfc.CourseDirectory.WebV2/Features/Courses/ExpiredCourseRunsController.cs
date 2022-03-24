using System;
using System.Linq;
using System.Threading.Tasks;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace Dfc.CourseDirectory.WebV2.Features.Courses
{
    [Route("courses/expired")]
    [RequireProviderContext]
    [JourneyMetadata(
        journeyName: "Courses",
        stateType: typeof(ExpiredCourseRuns.SelectedCoursesJourneyModel),
        appendUniqueKey: false,
        requestDataKeys: new[] { "ProviderId" })]
    public class ExpiredCourseRunsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private JourneyInstance _journeyInstance;

        public ExpiredCourseRunsController(IMediator mediator, JourneyInstanceProvider journeyInstanceProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _journeyInstance = journeyInstanceProvider.GetInstance();

        }

        [HttpGet("")]
        public async Task<IActionResult> Index() =>
            await _mediator.SendAndMapResponse(new ExpiredCourseRuns.Query(), vm => View("ExpiredCourseRuns", vm));

        [HttpPost]
        [RequireJourneyInstance]
        public async Task<IActionResult> Update(Guid[] selectedCourses)
        {

            _journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(() => new ExpiredCourseRuns.SelectedCoursesJourneyModel());


            var query = new ExpiredCourseRuns.SelectedQuery();
            query.CheckedRows = selectedCourses.ToArray();
            return await _mediator.SendAndMapResponse(query, vm => View("SelectedExpiredCourseRuns", vm));
            
        }

        [HttpPost("updated")]
        public async Task<IActionResult> Updated (DateTime Day, DateTime Month, DateTime Year)
        {
         
            string startDate = string.Format("{0}-{1}-{2}", Day, Month, Year);
            DateTime specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                System.Globalization.CultureInfo.InvariantCulture);

             var query = new ExpiredCourseRuns.UpdatedStartDate();
             query.StartDate = specifiedStartDate;

            return await _mediator.SendAndMapResponse(query, vm => View("updated", vm));

        } 

    }
          
}
