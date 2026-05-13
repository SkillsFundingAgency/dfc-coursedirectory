using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourses;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourseUpdates;
using System;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Globalization;

namespace Dfc.CourseDirectory.Api.Controllers
{
    [ApiController]
    public class GetCourseDataController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetCourseDataController> _log;

        public GetCourseDataController(IMediator mediator, ILogger<GetCourseDataController> log)
        {
            _mediator = mediator;
            _log = log;
        }

        [HttpGet("~/public/courses/list")]
        [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CoursesList(int pageSize, int pageNumber)
        {
            _log.LogInformation("Started Executing '{MethodName}' Method", nameof(CoursesList));

            if (pageSize <= 0 || pageNumber <= 0)
            {
                _log.LogWarning("Invalid pagination parameters provided. Page Size [{PageSize}] and Page Number [{PageNumber}]. Response Code [BAD REQUEST]", pageSize, pageNumber);
                return BadRequest("PageSize and PageNumber must be greater than zero.");
            }
            else if(pageSize > 100)
            {
                _log.LogWarning("Page Size [{PageSize}] exceeds the maximum allowed limit. Response Code [BAD REQUEST]", pageSize);
                return BadRequest("PageSize must not exceed 100.");
            }

            var request = new CourseRequest()
                {
                    PageSize = pageSize,
                    PageNumber = pageNumber
                };

            _log.LogInformation("Start Getting for List of Courses by the Page Size [{PageSize}] and Page Number [{PageNumber}]", request.PageSize, request.PageNumber);
            var result = await _mediator.Send(request);
           
            return result.Match<IActionResult>(
                _ =>
                {
                    _log.LogWarning("Failed to retrieve List of Courses with given search criteria.Response Code [NOT FOUND]");
                    _log.LogInformation("Completed Executing '{MethodName}' Method", nameof(CoursesList));
                    return NotFound();
                },
                r =>
                {
                    _log.LogInformation("List of Courses found. Returning data in Json format. Response Code [OK]");
                    _log.LogInformation("Completed Executing '{MethodName}' Method", nameof(CoursesList));
                    return Ok(r);
                });
        }

        [HttpGet("~/public/courses/updates")]
        [ProducesResponseType(typeof(CourseUpdateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseUpdates(string cutOffDate, int pageSize, int pageNumber)
        {
            _log.LogInformation("Started Executing '{MethodName}' Method", nameof(CourseUpdates));

            var isValidDate = DateTime.TryParseExact(cutOffDate, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedCutOffDate);
            var isValidDateTime = DateTime.TryParseExact(cutOffDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedCutOffDateTime);

            if (!isValidDate && !isValidDateTime)
            {
                _log.LogWarning("Invalid CutOffDate parameter provided. Response Code [BAD REQUEST]");
                return BadRequest("CutOffDate must be a valid date.");
            }
            else if ((isValidDate && parsedCutOffDate > DateTime.Now) 
                || (isValidDateTime && parsedCutOffDateTime > DateTime.Now))
            {
                _log.LogWarning("CutOffDate parameter provided is a future date. Response Code [BAD REQUEST]");
                return BadRequest("CutOffDate cannot be a future date.");
            }

            if (pageSize <= 0 || pageNumber <= 0)
            {
                _log.LogWarning("Invalid pagination parameters provided. Page Size [{PageSize}] and Page Number [{PageNumber}]. Response Code [BAD REQUEST]", pageSize, pageNumber);
                return BadRequest("PageSize and PageNumber must be greater than zero.");
            }
            else if (pageSize > 100)
            {
                _log.LogWarning("Page Size [{PageSize}] exceeds the maximum allowed limit. Response Code [BAD REQUEST]", pageSize);
                return BadRequest("PageSize must not exceed 100.");
            }
            

            var request = new CourseUpdateRequest()
            {
                CutOffDate = isValidDate ? parsedCutOffDate : parsedCutOffDateTime,
                PageSize = pageSize,
                PageNumber = pageNumber
            };

            _log.LogInformation("Start Getting for List of Course Updates by the CutOffDate [{CutOffDate}], Page Size [{PageSize}] and Page Number [{PageNumber}]", request.CutOffDate, request.PageSize, request.PageNumber);
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ =>
                {
                    _log.LogWarning("Failed to retrieve List of Courses with given search criteria.Response Code [NOT FOUND]");
                    _log.LogInformation("Completed Executing '{MethodName}' Method", nameof(CourseUpdates));
                    return NotFound();
                },
                r =>
                {
                    _log.LogInformation("List of Courses found. Returning data in Json format. Response Code [OK]");
                    _log.LogInformation("Completed Executing '{MethodName}' Method", nameof(CourseUpdates));
                    return Ok(r);
                });
        }
    }
}
