using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
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
