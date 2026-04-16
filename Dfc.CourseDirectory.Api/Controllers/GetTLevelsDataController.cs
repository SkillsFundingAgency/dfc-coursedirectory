using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourseUpdates;
using Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelList;
using Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelUpdates;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Api.Controllers
{
    [ApiController]
    public class GetTLevelsDataController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetTLevelsDataController> _log;

        public GetTLevelsDataController(IMediator mediator, ILogger<GetTLevelsDataController> log)
        {
            _mediator = mediator;
            _log = log;
        }
        
        [HttpGet("~/public/tlevels/list")]
        [ProducesResponseType(typeof(TLevelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TLevelsList(int pageSize, int pageNumber)
        {
            _log.LogInformation("Started Executing '{MethodName}' Method", nameof(TLevelsList));

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

            var request = new TLevelRequest()
            {
                PageSize = pageSize,
                PageNumber = pageNumber
            };

            _log.LogInformation("Start Getting for List of TLevels by the Page Size [{PageSize}] and Page Number [{PageNumber}]", pageSize, pageNumber);

            var result = await _mediator.Send(request);

            
            return result.Match<IActionResult>(
                _ =>
                {
                    _log.LogWarning("Failed to retrieve List of TLevels with given search criteria.Response Code [NOT FOUND]");
                    _log.LogInformation("Completed Executing '{MethodName}' Method", nameof(TLevelsList));

                    return NotFound();
                },
                r =>
                {
                    _log.LogInformation("List of TLevels found. Returning data in Json format. Response Code [OK]");
                    _log.LogInformation("Completed Executing '{MethodName}' Method", nameof(TLevelsList));

                    return Ok(r);
                });
        }

        [HttpGet("~/public/tlevels/updates")]
        [ProducesResponseType(typeof(TLevelUpdateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TLevelUpdates(DateTime cutOffDate, int pageSize, int pageNumber)
        {
            _log.LogInformation("Started Executing '{MethodName}' Method", nameof(TLevelUpdates));

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
            if (cutOffDate < DateTime.MinValue || cutOffDate > DateTime.MaxValue)
            {
                _log.LogWarning("Invalid CutOffDate parameter provided. Response Code [BAD REQUEST]");
                return BadRequest("CutOffDate must be a valid date.");
            }

            var request = new TLevelUpdateRequest()
            {
                CutOffDate = cutOffDate,
                PageSize = pageSize,
                PageNumber = pageNumber
            };

            _log.LogInformation("Start Getting for List of TLevel Updates by the CutOffDate [{CutOffDate}], Page Size [{PageSize}] and Page Number [{PageNumber}]", request.CutOffDate, request.PageSize, request.PageNumber);
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ =>
                {
                    _log.LogWarning("Failed to retrieve List of TLevels with given search criteria.Response Code [NOT FOUND]");
                    _log.LogInformation("Completed Executing '{MethodName}' Method", nameof(TLevelUpdates));
                    return NotFound();
                },
                r =>
                {
                    _log.LogInformation("List of TLevels found. Returning data in Json format. Response Code [OK]");
                    _log.LogInformation("Completed Executing '{MethodName}' Method", nameof(TLevelUpdates));
                    return Ok(r);
                });
        }
    }
}
