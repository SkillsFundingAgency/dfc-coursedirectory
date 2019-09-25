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
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Models.Models.Providers;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class BUApprenticeshipsController : Controller
    {
        private readonly ILogger<BulkUploadController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBulkUploadService _bulkUploadService;
        private readonly IBlobStorageService _blobService;
        private readonly ICourseService _courseService;
        private readonly IProviderService _providerService;
        private IHostingEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;
        
         public BUApprenticeshipsController(
                ILogger<BulkUploadController> logger,
                IHttpContextAccessor contextAccessor,
                IBulkUploadService bulkUploadService,
                IBlobStorageService blobService,
                ICourseService courseService,
                IHostingEnvironment env,
                IProviderService providerService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(bulkUploadService, nameof(bulkUploadService));
            Throw.IfNull(blobService, nameof(blobService));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(env, nameof(env));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(providerService, nameof(providerService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _bulkUploadService = bulkUploadService;
            _blobService = blobService;
            _courseService = courseService;
            _env = env;
            _courseService = courseService;
            _providerService = providerService;
        }


        [Authorize]
        public IActionResult Index()
        {
            _session.SetString("Option", "BulkUploadApprenticeships");
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            Provider provider = FindProvider(UKPRN.Value);
            if(null == provider)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Failed to look up Provider details." });
            }


            var courseCounts = _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(UKPRN)).Result;
            var courseErrors = courseCounts.HasValue && courseCounts.IsSuccess ? courseCounts.Value.Where(x => (int)x.Status == (int)RecordStatus.MigrationPending  && x.Count > 0|| (int)x.Status == (int)RecordStatus.MigrationReadyToGoLive && x.Count > 0).Count() : 500;
            
            var model = new BulkUploadViewModel
            {
                HasMigrationErrors = courseErrors > 0 ? true : false,
            };

            if(null != provider.BulkUploadStatus)
            {
                model.BulkUploadBackgroundInProgress = provider.BulkUploadStatus.InProgress;
                model.BulkUploadBackgroundRowCount = provider.BulkUploadStatus.TotalRowCount;
                model.BulkUploadBackgroundStartTimestamp = provider.BulkUploadStatus.StartedTimestamp;
            }
            return View("Index",model);
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

            if (Validate.ValidateFile(bulkUploadFile, out errorMessage))
            {
                int providerUKPRN = UKPRN.Value;
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string bulkUploadFileNewName = string.Format(@"{0}-{1}", DateTime.Now.ToString("yyMMdd-HHmmss"),
                    bulkUploadFile.FileName);

                MemoryStream ms = new MemoryStream();
                bulkUploadFile.CopyTo(ms);

                if (!Validate.isBinaryStream(ms))
                {
                    int csvLineCount = _bulkUploadService.CountCsvLines(ms);
                    bool processInline = (csvLineCount <= _blobService.InlineProcessingThreshold);
                    _logger.LogInformation(
                        $"Csv line count = {csvLineCount} threshold = {_blobService.InlineProcessingThreshold} processInline = {processInline}");

                    if (processInline)
                    {
                        bulkUploadFileNewName +=
                            "." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") +
                            ".processed"; // stops the Azure trigger from processing the file
                    }

                    Task task = _blobService.UploadFileAsync(
                        $"{UKPRN.ToString()}/Bulk Upload Apprenticeships/Files/{bulkUploadFileNewName}", ms);
                    task.Wait();

                    var errors = _bulkUploadService.ProcessApprenticeshipBulkUpload(ms, providerUKPRN, userId, processInline);

                    if (errors.Any())
                    {
                        vm.errors = errors;
                        return View(vm);
                    }
                    else
                    {
                        if (processInline)
                        {
                            //TODO:GB send to correct controller
                            // All good => redirect to BulkCourses action
                            return RedirectToAction("Index", "PublishCourses",
                                new {publishMode = PublishMode.BulkUpload, fromBulkUpload = true});
                        }
                        else
                        {
                            return RedirectToAction("Pending");
                        }
                    }

                }
                else
                {
                    vm.errors = new string[] {"Invalid file content."};
                }
            }

            else
            {
                vm.errors = new string[] {errorMessage};
            }

            return View(vm);
        }
        
        /*
        [Authorize]
        [HttpGet]
        public IActionResult ProcessMigrationReportErrors(string UKPRN)
        {
            _session.SetInt32("UKPRN", Convert.ToInt32(UKPRN));
            return RedirectToAction("WhatDoYouWantToDoNext");
        }

*/
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

            return View("../BulkUploadApprenticeships/DownloadErrorFile/Index", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DownloadErrorFile(DownloadErrorFileViewModel model)
        {
            // where to go????
            return View("../BulkUploadApprenticeships/WhatDoYouWantToDoNext/Index", new WhatDoYouWantToDoNextViewModel());
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteFile()
        {
            var model = new DeleteFileViewModel();


            return View("../BulkUploadApprenticeships/DeleteFile/Index", model);
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

            IEnumerable<Services.BlobStorageService.BlobFileInfo> list = _blobService.GetFileList(UKPRN + "/Bulk Upload Apprenticeships/Files/").OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
            {
                fileUploadDate = list.FirstOrDefault().DateUploaded.Value;
                var archiveFilesResult = _blobService.ArchiveFiles($"{UKPRN.ToString()}/Bulk Upload Apprenticeships/Files/");
            }


            var deleteBulkuploadResults = await _courseService.DeleteBulkUploadCourses(UKPRN);

            if (deleteBulkuploadResults.IsSuccess)
            {
                return RedirectToAction("DeleteFileConfirmation", "BulkuploadApprenticeships", new { fileUploadDate = fileUploadDate });
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

            return View("../BulkuploadApprenticeships/DeleteFileConfirmation/Index", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PublishYourFile(int NumberOfCourses)
        {
            var model = new PublishYourFileViewModel();

            model.NumberOfCourses = NumberOfCourses;

            return View("../BulkuploadApprenticeships/PublishYourFile/Index", model);
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
            return View("../BulkuploadApprenticeships/Complete/Index", new PublishCompleteViewModel() { NumberOfCoursesPublished = model.NumberOfCourses, Mode = PublishMode.BulkUpload });
        }

       

        
        private Provider FindProvider(int prn)
        {
            Provider provider = null;
            try
            {
                var providerSearchResult = Task.Run(async () => await _providerService.GetProviderByPRNAsync(new Services.ProviderService.ProviderSearchCriteria(prn.ToString()))).Result;
                if (providerSearchResult.IsSuccess)
                {
                    provider = providerSearchResult.Value.Value.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                // @ToDo: decide how to handle this
            }
            return provider;
        }
    }
}