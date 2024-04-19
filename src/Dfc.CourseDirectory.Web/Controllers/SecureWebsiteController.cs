using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Dfc.CourseDirectory.Core.Validation;

namespace Dfc.CourseDirectory.Web.Controllers
{
    /*
    [Route("api/[controller]")]
    [ApiController]
    public class SecureWebsiteController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public async Task<bool> Validate([FromQuery] bool isSecureWebsite)
        {
            return await Rules.SecureWebsite<bool>(isSecureWebsite);
        }
    }
    */
}
