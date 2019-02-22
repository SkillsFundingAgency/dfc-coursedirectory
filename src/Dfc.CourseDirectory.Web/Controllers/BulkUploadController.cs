using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class BulkUploadController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            var model = new BulkUploadViewModel();
            model.AbraKadabra = "Welcome to BulkUpload UI! <br /> Get ready to be amazed!";

            return View("Index", model);
        }
    }
} 