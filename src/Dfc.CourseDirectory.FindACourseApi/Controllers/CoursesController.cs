using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _log;

        public CoursesController(
            IMediator mediator,
            ILogger<CoursesController> logger)
        {
            _mediator = mediator;
            _log = logger;
        }

        [HttpPost("~/coursesearch")]  // Kept around to avoid breaking API change
        [HttpPost("~/search")]
        [ProducesResponseType(typeof(Features.Search.SearchViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseSearch([FromBody] Features.Search.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                p =>
                {
                    _log.LogWarning($"{nameof(CourseSearch)} failed. {nameof(p.Title)}: {{{nameof(p.Title)}}}, {nameof(p.Detail)}: {{{nameof(p.Detail)}}}.", p.Title, p.Detail);

                    return new ObjectResult(p)
                    {
                        ContentTypes = new MediaTypeCollection()
                        {
                            new MediaTypeHeaderValue("application/problem+json")
                        },
                        StatusCode = p.Status ?? StatusCodes.Status400BadRequest
                    };
                },
                r => Ok(r));
        }

        [HttpGet("~/courserundetail")]
        [ProducesResponseType(typeof(Features.CourseRunDetail.CourseRunDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseRunDetail([FromQuery] Features.CourseRunDetail.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => NotFound(),
                r => Ok(r));
        }

        [HttpGet("~/tleveldetail")]
        [ProducesResponseType(typeof(Features.TLevelDetail.TLevelDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult TLevelDetail([FromQuery] Features.TLevelDetail.Query request)
        {
            // Dummy data

            return Ok(new Features.TLevelDetail.TLevelDetailViewModel()
            {
                AttendancePattern = Core.Models.CourseAttendancePattern.Daytime,
                Cost = 0,
                DeliveryMode = Core.Models.CourseDeliveryMode.ClassroomBased,
                DurationUnit = Core.Models.CourseDurationUnit.Years,
                DurationValue = 2,
                EntryRequirements = "You will need either 5 GCSEs (at grade 4 or above), including English, maths and science, or a pass in a relevant level 2 qualification.\n\nIf you do not have the recommended entry qualifications, you may still be considered if you have relevant experience or show a natural ability for the subject.",
                HowYoullBeAssessed = "You will be assessed by completing an employer set project and a project on your specialist subject. You'll have time to research and complete tasks. There'll also be 2 exams. \n\nYou'll work with your tutor to decide when you're ready to be assessed.",
                HowYoullLearn = "Your learning will combine classroom theory and practical learning and include 9 weeks (minimum) of industry placement. The placement will provide you with a real experience of the workplace.",
                Locations = new[]
                {
                    new Features.TLevelDetail.TLevelLocationViewModel()
                    {
                        AddressLine1 = "Demo provider line 1",
                        Town = "Coventry",
                        Postcode = "CV1 2AA",
                        Latitude = 1.23m,
                        Longitude = 1.23m,
                        VenueName = "Provider venue"
                    }
                },
                OfferingType = Core.Search.Models.FindACourseOfferingType.TLevel,
                Provider = new Features.TLevelDetail.ProviderViewModel()
                {
                    AddressLine1 = "Demo provider line 1",
                    Town = "Coventry",
                    Postcode = "CV1 2AA",
                    ProviderName = "Demo provider",
                    Ukprn = "123456"
                },
                Qualification = new Features.TLevelDetail.QualificationViewModel()
                {
                    FrameworkCode = 36,
                    ProgType = 31,
                },
                StartDate = new System.DateTime(2021, 10, 1),
                StudyMode = Core.Models.CourseStudyMode.FullTime,
                TLevelId = request.TLevelId,
                Website = "https://provider.com/tlevel",
                WhatYouCanDoNext = "You'll have the industry knowledge and experience to progress into roles like:\n\nCivil engineering technician\nTechnical surveyor\nBuilding technician\nEngineering construction technician\nArchitectural technician\n\nor go onto an apprenticeship or higher education.",
                WhatYoullLearn = "You'll learn specific topics in design, surveying and planning:\n\nProject management\nBudgeting and resource allocation\nProcurement\nRisk management\n\nIn addition to the core content, you will choose one of the following as a specialist subject:\n\nSurveying and design for construction and the built environment\nCivil engineering\nBuilding services design\nHazardous materials analysis and surveying",
                WhoFor = "This T Level is suitable for anyone wanting a career in construction, specifically in surveying and design, civil engineering, building services design, or hazardous materials surveying.\n\nThe T Level is a 2 year programme and will include a 9 week (minimum) industry placement and a Technical Qualification, where you will also choose a specialist subject.",
                YourReference = "TLEVEL"
            });
        }
    }
}
