using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class DQIController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public DQIController(
            IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            _contextAccessor = contextAccessor;
        }

        [Authorize]
        public IActionResult Index(string msg)
        {

            //TODO: Refactor the DQI journey as to allow for expansion for other journeys

            if (msg.Contains("apprenticeship"))
            {
                return RedirectToAction("Index", "PublishApprenticeships");
            }
            else
            {
                _session.SetString("Option", "DQI");
                return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.DataQualityIndicator });
            }
            
        }
    }
}