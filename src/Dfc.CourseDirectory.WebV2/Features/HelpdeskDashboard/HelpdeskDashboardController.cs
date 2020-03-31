using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.HelpdeskDashboard
{
    [RequireFeatureFlag(FeatureFlags.ApprenticeshipQA)]
    [Authorize(Policy = AuthorizationPolicyNames.ApprenticeshipQA)]
    public class HelpdeskDashboardController : Controller
    {
        [HttpGet("helpdesk-dashboard")]
        public IActionResult Dashboard() => View();
    }
}
