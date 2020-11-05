﻿using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests.StubControllers
{
    public class ProviderController : Controller
    {
        [HttpGet("dashboard")]
        public IActionResult Dashboard() => Ok();
		
        [HttpGet("provider/provider-type")]
        public IActionResult AddOrEditProviderType([FromQuery] bool updateProviderType) => Ok();
    }
}
