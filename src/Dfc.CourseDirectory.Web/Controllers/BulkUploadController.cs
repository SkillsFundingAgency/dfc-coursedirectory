﻿
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
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels;

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
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _bulkUploadService = bulkUploadService;
            _blobService = blobService;
            _courseService = courseService;
            _env = env;
            _courseService = courseService;
        }


        [Authorize]
        public IActionResult Index()
        {
            _session.SetString("Option", "BulkUpload");
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            var courseCounts = _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(UKPRN)).Result;
            var courseErrors = courseCounts.HasValue && courseCounts.IsSuccess ? courseCounts.Value.Where(x => (int)x.Status == (int)RecordStatus.MigrationPending  && x.Count > 0|| (int)x.Status == (int)RecordStatus.MigrationReadyToGoLive && x.Count > 0).Count() : 500;

            var model = new BulkUploadViewModel
            {
                HasMigrationErrors = courseErrors > 0 ? true : false
            };

            return View("Index", model);
        }

        [Authorize]
        public IActionResult Pending()
        {
            _session.SetString("Option", "BulkUpload");
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            return View("./Pending/Index");
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

                int csvLineCount = _bulkUploadService.CountCsvLines(ms);
                bool processInline = (csvLineCount <= _blobService.InlineProcessingThreshold);
                _logger.LogInformation($"Csv line count = {csvLineCount} threshold = {_blobService.InlineProcessingThreshold} processInline = {processInline}");

                if (processInline)
                {
                    bulkUploadFileNewName += "." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".processed"; // stops the Azure trigger from processing the file
                }

                Task task = _blobService.UploadFileAsync($"{UKPRN.ToString()}/Bulk Upload/Files/{bulkUploadFileNewName}", ms);
                task.Wait();

                var errors = _bulkUploadService.ProcessBulkUpload(ms, providerUKPRN, userId, processInline);

                if (errors.Any())
                {
                    vm.errors = errors;
                    return View(vm);                   
                }
                else
                {
                    if(processInline)
                    {
                        // All good => redirect to BulkCourses action
                        return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.BulkUpload, fromBulkUpload = true });
                    }
                    else
                    {
                        return RedirectToAction("Pending");
                    }
                }

            }
            else
            {
                vm.errors = new string[] { errorMessage };
            }

            return View(vm);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ProcessMigrationReportErrors(string UKPRN)
        {
            _session.SetInt32("UKPRN", Convert.ToInt32(UKPRN));
            return RedirectToAction("WhatDoYouWantToDoNext");
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> WhatDoYouWantToDoNext(string message)
        {
            var model = new WhatDoYouWantToDoNextViewModel();

            if (!string.IsNullOrEmpty(message))
            {
                model.Message = message;
            }
           
            return View("../BulkUpload/WhatDoYouWantToDoNext/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> WhatDoYouWantToDoNext(WhatDoYouWantToDoNextViewModel model)
        {
            var fromBulkUpload = false;
            if (!string.IsNullOrEmpty(model.Message))
            {
                fromBulkUpload = true;

            }
            switch (model.WhatDoYouWantToDoNext)
            {
                case Models.Enums.WhatDoYouWantToDoNext.OnScreen:
                    return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.BulkUpload, fromBulkUpload });
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
        public IActionResult DownloadErrorFile()
        {
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            var model = new DownloadErrorFileViewModel() { ErrorFileCreatedDate = DateTime.Now, UKPRN = UKPRN };
            IEnumerable<BlobFileInfo> list = _blobService.GetFileList(UKPRN + "/Bulk Upload/Files/").OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
            {
                model.ErrorFileCreatedDate = list.FirstOrDefault().DateUploaded.Value.DateTime;
            }

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


            var deleteBulkuploadResults = await _courseService.DeleteBulkUploadCourses(UKPRN);

            if (deleteBulkuploadResults.IsSuccess)
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

            DateTime localDateTime = DateTime.Parse(fileUploadDate.ToString());
            DateTime utcDateTime = localDateTime.ToUniversalTime();

            model.FileUploadedDate = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local).ToString("dd MMM yyyy HH:mm");

            return View("../Bulkupload/DeleteFileConfirmation/Index", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PublishYourFile(int NumberOfCourses)
        {
            var model = new PublishYourFileViewModel();

            model.NumberOfCourses = NumberOfCourses;

            return View("../Bulkupload/PublishYourFile/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PublishYourFile(PublishYourFileViewModel model)
        {
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

            var resultArchivingCourses = await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.Live, (int)RecordStatus.Archived);
            if (resultArchivingCourses.IsSuccess)
            {
                await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.BulkUploadReadyToGoLive, (int)RecordStatus.Live);
            }
            //to publish stuff
            return View("../Bulkupload/Complete/Index", new PublishCompleteViewModel() { NumberOfCoursesPublished = model.NumberOfCourses, Mode = PublishMode.BulkUpload });
        }

        [Authorize]
        [HttpGet]
        public IActionResult LandingOptions()
        {
            return View("../BulkUpload/LandingOptions/Index", new BulkuploadLandingViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> LandingOptions(BulkuploadLandingViewModel model)
        {
            switch (model.BulkUploadLandingOptions)
            { 
                case BulkUploadLandingOptions.Apprenticeship:
                    return RedirectToAction("ApprenticeshipIndex", "BulkUpload");
                case BulkUploadLandingOptions.FE:
                    return RedirectToAction("Index", "BulkUpload");
                default:
                    return RedirectToAction("LandingOptions", "BulkUpload");
            }

        }

        [Authorize]
        public IActionResult ApprenticeshipIndex()
        {

            return View("ApprenticeshipIndex");
        }

        /// <summary>
        /// Server side validation to match and extend the client-side validation
        /// </summary>
        /// <param name="bulkUploadFile"></param>
        /// <returns></returns>
        private bool ValidateFile(IFormFile bulkUploadFile, out string errorMessage)
        {
            if (bulkUploadFile.Length == 0)
            {
                errorMessage = "No file uploaded";
                return false;
            }

            if (!bulkUploadFile.FileName.EndsWith(".csv") || bulkUploadFile.FileName.Replace(".csv", string.Empty).Contains(".") || bulkUploadFile.Name != "bulkUploadFile")
            {
                errorMessage = "Invalid file name";
                return false;
            }
            if (!bulkUploadFile.ContentDisposition.Contains("filename"))
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