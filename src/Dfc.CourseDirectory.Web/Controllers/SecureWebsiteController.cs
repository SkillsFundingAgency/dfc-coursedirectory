using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Dfc.CourseDirectory.Core.Services;

namespace Dfc.CourseDirectory.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SecureWebsiteController : ControllerBase
    {

        private readonly IWebRiskService _webRiskService;
        public SecureWebsiteController(IWebRiskService webRiskService)
        {
            _webRiskService = webRiskService;
        }

        [Authorize]
        [HttpGet]
        public async Task<bool> Validate([FromQuery] string url)
        {
            return await _webRiskService.CheckForSecureUri(url);
        }
    }
}
