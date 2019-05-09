using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Web.Controllers
{

    public class BulkUploadController : Controller
    {
        private readonly ILogger<BulkUploadController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBulkUploadService _bulkUploadService;

        private IHostingEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;

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
            _session.SetString("Option", "BulkUpload");
            var model = new BulkUploadViewModel();           

            return View("Index", model);
        }

        [Authorize]
        [HttpPost("BulkUpload")]
        public async Task<IActionResult> Index(IFormFile bulkUploadFile)
        {
            int? UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            BulkUploadViewModel vm = new BulkUploadViewModel();
            string errorMessage;

            if (ValidateFile(bulkUploadFile, out errorMessage))
            {
                int providerUKPRN = UKPRN.Value;
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                string webRoot = _env.WebRootPath;
                if (!Directory.Exists(webRoot))
                    Directory.CreateDirectory(webRoot);
                string bulkUploadFileNewName = string.Format(@"{0}-{1}", DateTime.Now.ToString("yyMMdd-HHmmss"), bulkUploadFile.FileName);
                string savedCsvFilePath = string.Format(@"{0}\BulkUploads\{1}", webRoot, bulkUploadFileNewName);

                using (var stream = new FileStream(savedCsvFilePath, FileMode.Create))
                {
                    await bulkUploadFile.CopyToAsync(stream);
                }

                var errors = _bulkUploadService.ProcessBulkUpload(savedCsvFilePath, providerUKPRN, userId);

                if (System.IO.File.Exists(savedCsvFilePath))
                {
                    System.IO.File.Delete(savedCsvFilePath);
                }

                if (errors.Any())
                {
                    vm.errors = errors;

                    return View(vm);
                }
                else
                {
                    // All good => redirect to BulkCourses action
                    return RedirectToAction("Index", "PublishCourses", new
                    {
                        publishMode = PublishMode.BulkUpload
                    });
                }
            }
            else
            {

                var noFileError = new List<string>
                {
                    errorMessage
                };
                vm.errors = noFileError;
            }
            return View(vm);
  

        }
        /// <summary>
        /// Server side validation to match and extend the client-side validation
        /// </summary>
        /// <param name="bulkUploadFile"></param>
        /// <returns></returns>
        private bool ValidateFile(IFormFile bulkUploadFile, out string errorMessage)
        {
            if(bulkUploadFile.Length == 0)
            {
                errorMessage = "No file uploaded";
                return false;
            }

            if (!bulkUploadFile.FileName.EndsWith(".csv") || bulkUploadFile.FileName.Replace(".csv", string.Empty).Contains(".") || bulkUploadFile.Name != "bulkUploadFile")
            {
                errorMessage = "Invalid file name";
                return false;
            }
            if(!bulkUploadFile.ContentDisposition.Contains("filename"))
            {
                errorMessage = "Invalid upload";
                return false;
            }
            if (bulkUploadFile.Length > 209715200)
            {
                errorMessage = "File too large";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

    }
}