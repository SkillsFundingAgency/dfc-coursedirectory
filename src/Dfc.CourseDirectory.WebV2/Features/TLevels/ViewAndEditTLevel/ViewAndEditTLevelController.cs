using System;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel
{
    [Route("t-levels")]
    public class ViewAndEditTLevelController : Controller
    {
        [RequireFeatureFlag(FeatureFlags.TLevels)]
        [RestrictProviderTypes(ProviderType.TLevels)]
        [HttpGet("{tLevelId}")]
        public IActionResult View(Guid TLevelId)
        {
            // Stub action for link on ViewTLevels/List
            throw new NotImplementedException();
        }
    }
}
