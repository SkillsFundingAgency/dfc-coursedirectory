using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class BulkUploadController : Controller
    {
        private readonly ILogger<BulkUploadController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        //private readonly ICourseService _courseService;
        private readonly IBulkUploadService _bulkUploadService;

        private IHostingEnvironment _env;

        public BulkUploadController(
                ILogger<BulkUploadController> logger,
                IHttpContextAccessor contextAccessor,
                IBulkUploadService bulkUploadService,
                IHostingEnvironment env)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(bulkUploadService, nameof(bulkUploadService));
            Throw.IfNull(env, nameof(env));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _bulkUploadService = bulkUploadService;
            _env = env;
        }


        [Authorize]
        public IActionResult Index()
        {
            var model = new BulkUploadViewModel();
            model.AbraKadabra = "Welcome to BulkUpload UI! <br /> Get ready to be amazed!";

            return View("Index", model);
        }

        [Authorize]
        [HttpPost("BulkUpload")]
        public async Task<IActionResult> Index(IFormFile bulkUploadFile)
        {
            string errorReturn = "ToDisplay";

            if (bulkUploadFile.Length > 0)
            {
                int providerUKPRN = 1000001;
                string userId = "qwe5rty-guid-userId-as-string-from-identity";

                string webRoot = _env.WebRootPath;
                string bulkUploadFileNewName = string.Format(@"{0}-{1}", DateTime.Now.ToString("yyMMdd-HHmmss"), bulkUploadFile.FileName);
                string savedCsvFilePath = string.Format(@"{0}\BulkUploads\{1}", webRoot, bulkUploadFileNewName);

                using (var stream = new FileStream(savedCsvFilePath, FileMode.Create))
                {
                    await bulkUploadFile.CopyToAsync(stream);
                }

                var errors = _bulkUploadService.ProcessBulkUpload(savedCsvFilePath, providerUKPRN, userId);

                if (errors.Any())
                {
                    foreach(var error in errors)
                    {
                        ModelState.AddModelError("Errors", error);
                    }
                }
                else
                {
                    // All good => redirect to BulkCourses action
                }

            }
            else
            {
                // error

                errorReturn = "No file uploaded";

                ModelState.AddModelError("Errors", errorReturn);
            }

            return Ok(new { errorReturn });
        }

    }
}