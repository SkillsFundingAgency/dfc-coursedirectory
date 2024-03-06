using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    public class SectorsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SectorsController> _log;

        public SectorsController(IMediator mediator, ILogger<SectorsController> log)
        {
            _mediator = mediator;
            _log = log;
        }

        [HttpGet("~/sectors")]
        [ProducesResponseType(typeof(IEnumerable<Features.Sectors.SectorViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Sectors()
        {
            _log.LogInformation($"Start getting all sectors");

            return Ok(await _mediator.Send(new Features.Sectors.Query()));
        }        
    }
}
