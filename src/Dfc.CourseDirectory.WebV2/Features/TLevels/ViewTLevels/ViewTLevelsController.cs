using System;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ViewTLevels
{
    [Route("t-levels/list")]
    public class ViewTLevelsController : Controller
    {
        // Stub action for link on AddTLevel/Success
        [HttpGet("")]
        public IActionResult List()
        {
            throw new NotImplementedException();
        }
    }
}
