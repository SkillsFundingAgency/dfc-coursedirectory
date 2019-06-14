
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;

namespace Dfc.CourseDirectory.Web.Controllers
{

    public class BulkUploadController : Controller
    {
        private readonly ILogger<BulkUploadController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBulkUploadService _bulkUploadService;
        private readonly IBlobStorageService _blobService;
        private readonly ICourseService _courseService;

        private IHostingEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public BulkUploadController(
                ILogger<BulkUploadController> logger,
                IHttpContextAccessor contextAccessor,
                IBulkUploadService bulkUploadService,
                IBlobStorageService blobService,
                ICourseService courseService,
                IHostingEnvironment env)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(bulkUploadService, nameof(bulkUploadService));
            Throw.IfNull(blobService, nameof(blobService));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(env, nameof(env));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _bulkUploadService = bulkUploadService;
            _blobService = blobService;
            _courseService = courseService;
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
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });


            BulkUploadViewModel vm = new BulkUploadViewModel();
            string errorMessage;

            if (ValidateFile(bulkUploadFile, out errorMessage))
            {
                int providerUKPRN = UKPRN.Value;
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string bulkUploadFileNewName = string.Format(@"{0}-{1}", DateTime.Now.ToString("yyMMdd-HHmmss"), bulkUploadFile.FileName);

                MemoryStream ms = new MemoryStream();
                bulkUploadFile.CopyTo(ms);
                Task task = _blobService.UploadFileAsync($"{UKPRN.ToString()}/Bulk Upload/Files/{bulkUploadFileNewName}", ms);
                task.Wait();
                var errors = _bulkUploadService.ProcessBulkUpload(ms, providerUKPRN, userId);

                if (errors.Any()) {
                    vm.errors = errors;
                    return View(vm);

                } else {
                    // All good => redirect to BulkCourses action
                    return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.BulkUpload });
                }

            } else {
                vm.errors = new string[] { errorMessage };
            }
            return View(vm);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> WhatDoYouWantToDoNext()
        {
            var model = new WhatDoYouWantToDoNextViewModel();

            return View("../BulkUpload/WhatDoYouWantToDoNext/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> WhatDoYouWantToDoNext(WhatDoYouWantToDoNextViewModel model)
        {
            switch (model.WhatDoYouWantToDoNext)
            {
                case Models.Enums.WhatDoYouWantToDoNext.OnScreen:
                    return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.BulkUpload });
                case Models.Enums.WhatDoYouWantToDoNext.DownLoad:
                    return RedirectToAction("DownloadErrorFile", "BulkUpload");
                case Models.Enums.WhatDoYouWantToDoNext.Delete:
                    return RedirectToAction("DeleteFile", "BulkUpload");
                default:
                    return RedirectToAction("WhatDoYouWantToDoNext", "BulkUpload");

            }
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DownloadErrorFile()
        {
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            var model = new DownloadErrorFileViewModel() { ErrorFileCreatedDate = DateTime.Now, UKPRN = UKPRN } ;
            return View("../Bulkupload/DownloadErrorFile/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DownloadErrorFile(DownloadErrorFileViewModel model)
        {
            // where to go????
            return View("../Bulkupload/WhatDoYouWantToDoNext/Index", new WhatDoYouWantToDoNextViewModel());
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteFile()
        {
            var model = new DeleteFileViewModel();


            return View("../Bulkupload/DeleteFile/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteFile(DeleteFileViewModel model)
        {
            DateTimeOffset fileUploadDate = new DateTimeOffset();
            int? sUKPRN = _session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
            else
            {
                UKPRN = sUKPRN ?? 0;
            }

            IEnumerable<Services.BlobStorageService.BlobFileInfo> list = _blobService.GetFileList(UKPRN + "/Bulk Upload/Files/").OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
            {
                fileUploadDate = list.FirstOrDefault().DateUploaded.Value;
                var archiveFilesResult = _blobService.ArchiveFiles($"{UKPRN.ToString()}/Bulk Upload/Files/");
            }

           
            var deleteBulkuploadResults = _courseService.DeleteBulkUploadCourses(UKPRN);

            if (deleteBulkuploadResults.Result.IsSuccess)
            {
                return RedirectToAction("DeleteFileConfirmation", "Bulkupload", new { fileUploadDate = fileUploadDate });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Delete All Bulk Uploaded Courses Error" });
            }

           
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteFileConfirmation(DateTimeOffset fileUploadDate)
        {
            var model = new DeleteFileConfirmationViewModel();
            model.FileUploadedDate = fileUploadDate.ToString("dd MMM yyyy hh:mm");

            return View("../Bulkupload/DeleteFileConfirmation/Index", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PublishYourFile()
        {
            var model = new PublishYourFileViewModel();

            model.NumberOfCourses = 99;

            return View("../Bulkupload/PublishYourFile/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PublishYourFile(PublishYourFileViewModel model)
        {
            //to publish stuff
            return View("../Bulkupload/Complete/Index", new PublishCompleteViewModel() { NumberOfCoursesPublished = 99, Mode = PublishMode.BulkUpload });
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