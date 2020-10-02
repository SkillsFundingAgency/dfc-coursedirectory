using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.HelpdeskDashboard
{
    [Authorize(Policy = AuthorizationPolicyNames.ApprenticeshipQA)]
    public class HelpdeskDashboardController : Controller
    {
        [HttpGet("helpdesk-dashboard")]
        public IActionResult Dashboard() => View();
    }
}
