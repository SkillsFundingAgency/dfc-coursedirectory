using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Web.BackgroundWorkers;
using Dfc.CourseDirectory.Web.Validation;
using Dfc.CourseDirectory.Web.ViewModels;
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
        private readonly IBulkUploadService _bulkUploadService;
        private readonly IBlobStorageService _blobService;
        private readonly ICourseService _courseService;
        private readonly IProviderService _providerService;
        private IWebHostEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private IBackgroundTaskQueue _queue;

        public BulkUploadController(
                ILogger<BulkUploadController> logger,
                IHttpContextAccessor contextAccessor,
                IBulkUploadService bulkUploadService,
                IBlobStorageService blobService,
                ICourseService courseService,
                IWebHostEnvironment env,
                IProviderService providerService,
                IBackgroundTaskQueue queue)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(bulkUploadService, nameof(bulkUploadService));
            Throw.IfNull(blobService, nameof(blobService));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(env, nameof(env));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(providerService, nameof(providerService));
            Throw.IfNull(queue, nameof(queue));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _bulkUploadService = bulkUploadService;
            _blobService = blobService;
            _courseService = courseService;
            _env = env;
            _courseService = courseService;
            _providerService = providerService;
            _queue = queue;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Courses", "BulkUpload");
        }

        [Authorize]
        [HttpGet("/bulk-upload/courses/upload")]
        public IActionResult Upload()
        {
            _session.SetString("Option", "BulkUpload");
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            Provider provider = FindProvider(UKPRN.Value);
            if (null == provider)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Failed to look up Provider details." });
            }

            var courseCounts = _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(UKPRN)).Result;
            var courseErrors = courseCounts.HasValue && courseCounts.IsSuccess ? courseCounts.Value.Where(x => (int)x.Status == (int)RecordStatus.MigrationPending && x.Count > 0 || (int)x.Status == (int)RecordStatus.MigrationReadyToGoLive && x.Count > 0).Count() : 500;

            var model = new BulkUploadViewModel
            {
                HasMigrationErrors = courseErrors > 0 ? true : false,
            };

            if (null != provider.BulkUploadStatus)
            {
                model.BulkUploadBackgroundInProgress = provider.BulkUploadStatus.InProgress;
                model.BulkUploadBackgroundRowCount = provider.BulkUploadStatus.TotalRowCount;
                model.BulkUploadBackgroundStartTimestamp = provider.BulkUploadStatus.StartedTimestamp;
            }

            return View(model);
        }

        [Authorize]
        [HttpPost("/bulk-upload/courses/upload")]
        public async Task<IActionResult> Upload(IFormFile bulkUploadFile)
        {
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            BulkUploadViewModel vm = new BulkUploadViewModel();

            string errorMessage;

            // COUR-1986 restoring delete for inline processing to fix issue with course errors accruing on the dashboard DQIs
            // NB: TEST WITH VOLUME!!! - this may cause time outs again
            var deleteResult = await _courseService.DeleteBulkUploadCourses(UKPRN.Value);
            if (deleteResult.IsFailure)
            {
                vm.errors = new string[] { deleteResult.Error };
            }
            else
            {
                if (Validate.ValidateFile(bulkUploadFile, out errorMessage))
                {
                    int providerUKPRN = UKPRN.Value;
                    string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    string bulkUploadFileNewName = string.Format(@"{0}-{1}", DateTime.Now.ToString("yyMMdd-HHmmss"),
                        bulkUploadFile.FileName);

                    MemoryStream ms = new MemoryStream();
                    bulkUploadFile.CopyTo(ms);

                    if (!Validate.IsBinaryStream(ms))
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
                            $"{UKPRN.ToString()}/Courses Bulk Upload/Files/{bulkUploadFileNewName}", ms);
                        task.Wait();

                        var errors = _bulkUploadService.ProcessBulkUpload(ms, providerUKPRN, userId, processInline);

                        if (errors.Any())
                        {
                            vm.errors = errors;
                            return View(vm);
                        }
                        else
                        {
                            if (processInline)
                            {
                                // All good => redirect to BulkCourses action
                                return RedirectToAction("Index", "PublishCourses",
                                    new { publishMode = PublishMode.BulkUpload, fromBulkUpload = true });
                            }
                            else
                            {
                                return RedirectToAction("Pending");
                            }
                        }

                    }
                    else
                    {
                        vm.errors = new string[] { "Invalid file content." };
                    }
                }

                else
                {
                    vm.errors = new string[] { errorMessage };
                }
            }
            return View(vm);
        }

        [Authorize]
        [HttpGet]
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
        [HttpGet]
        public IActionResult ProcessMigrationReportErrors(string UKPRN)
        {
            _session.SetInt32("UKPRN", Convert.ToInt32(UKPRN));
            return RedirectToAction("WhatDoYouWantToDoNext");
        }

        [Authorize]
        [HttpGet]
        public IActionResult WhatDoYouWantToDoNext(string message)
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
        public IActionResult WhatDoYouWantToDoNext(WhatDoYouWantToDoNextViewModel model)
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

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                DateTime dt1 = DateTime.Parse(list.FirstOrDefault().DateUploaded.Value.DateTime.ToString());
                DateTime dt2 = TimeZoneInfo.ConvertTimeFromUtc(dt1, tzi);

                model.ErrorFileCreatedDate = Convert.ToDateTime(dt2.ToString("dd MMM yyyy HH:mm"));

            }

            return View("../Bulkupload/DownloadErrorFile/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult DownloadErrorFile(DownloadErrorFileViewModel model)
        {
            // where to go????
            return View("../Bulkupload/WhatDoYouWantToDoNext/Index", new WhatDoYouWantToDoNextViewModel());
        }

        [Authorize]
        [HttpGet]
        public IActionResult DeleteFile()
        {
            var model = new DeleteFileViewModel();


            return View("../Bulkupload/DeleteFile/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult DeleteFile(DeleteFileViewModel model)
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

            var provider = FindProvider(UKPRN);
            if (null == provider)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Failed to find Provider data to delete bulk upload." });
            }

            IEnumerable<Services.BlobStorageService.BlobFileInfo> list = _blobService.GetFileList(UKPRN + "/Bulk Upload/Files/").OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
            {
                fileUploadDate = list.FirstOrDefault().DateUploaded.Value;
                var archiveFilesResult = _blobService.ArchiveFiles($"{UKPRN.ToString()}/Bulk Upload/Files/");
            }


            // COUR-1927 move the delete to a background worker because it's timing out for large files.
            bool deleteSuccess = false;
            _queue.QueueBackgroundWorkItem(async token =>
            {
                var guid = Guid.NewGuid().ToString();
                var tag = $"delete bulk upload for provider {UKPRN}.";
                var startTimestamp = DateTime.UtcNow;

                try
                {
                    _logger.LogInformation($"{startTimestamp.ToString("yyyyMMddHHmmss")} Starting background worker {guid} for {tag}");

                    var deleteBulkuploadResults = await _courseService.DeleteBulkUploadCourses(UKPRN);
                    deleteSuccess = deleteBulkuploadResults.IsSuccess;

                    var finishTimestamp = DateTime.UtcNow;
                    _logger.LogInformation($"{finishTimestamp.ToString("yyyyMMddHHmmss")} background worker {guid} finished successfully for {tag}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error whilst deleting bulk upload file on background worker {guid} for {tag}", ex);
                }

                _logger.LogInformation($"Queued Background Task {guid} is complete.");
            });

            // COUR-1972 make sure we get a date on the Delete Confirmation page even if the physical delete above didn't find any files to delete.
            if (null != provider.BulkUploadStatus)
            {
                if (provider.BulkUploadStatus.StartedTimestamp.HasValue)
                {
                    fileUploadDate = provider.BulkUploadStatus.StartedTimestamp.Value.ToLocalTime();
                }
            }

            if (deleteSuccess)
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
        public IActionResult DeleteFileConfirmation(DateTimeOffset fileUploadDate)
        {
            var model = new DeleteFileConfirmationViewModel();

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTime dt1 = DateTime.Parse(fileUploadDate.DateTime.ToString());
            DateTime dt2 = TimeZoneInfo.ConvertTimeFromUtc(dt1, tzi);

            model.FileUploadedDate = dt2.ToString("dd MMM yyyy HH:mm");

            return View("../Bulkupload/DeleteFileConfirmation/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult PublishYourFile(PublishYourFileViewModel model)
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

            // COUR-1864
            // Offload this long running activity to a background task.
            // @See:  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2&tabs=visual-studio

            // Find the provider and flag it as "publish in progress" outside of the queue otherwise the time lag is too great and
            // the UI displays incorrect info

            // Flag the provider as "publish in progress".
            var provider = FindProvider(UKPRN);
            if (null == provider) throw new Exception($"Failed to find provider with UK PRN {UKPRN}");
            if (null == provider.BulkUploadStatus) provider.BulkUploadStatus = new BulkUploadStatus();
            provider.BulkUploadStatus.PublishInProgress = true;
            var flagProviderResult = _providerService.UpdateProviderDetails(provider).Result;
            if (flagProviderResult.IsFailure) throw new Exception($"Failed to set the 'publish in progress' flag for provider with UK PRN {UKPRN}.");

            // Now queue the background work to publish the courses.
            _queue.QueueBackgroundWorkItem(async token =>
            {
                var guid = Guid.NewGuid().ToString();
                var tag = $"bulk upload publish for provider {UKPRN} for {model.NumberOfCourses} courses.";
                var startTimestamp = DateTime.UtcNow;

                try
                {
                    _logger.LogInformation($"{startTimestamp.ToString("yyyyMMddHHmmss")} Starting background worker {guid} for {tag}");

                    // Publish the bulk-uploaded courses.

                    var resultArchivingCourses = await _courseService.ArchiveCoursesExceptBulkUploadReadytoGoLive(UKPRN, (int)RecordStatus.Archived);
                    if (resultArchivingCourses.IsSuccess)
                    {
                        var resultGoingLive = await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.BulkUploadReadyToGoLive, (int)RecordStatus.Live);
                        if (resultGoingLive.IsSuccess)
                        {
                            // Clear the provider "publish in progress" flag.
                            provider.BulkUploadStatus.PublishInProgress = false;
                            var unflagProviderResult = await _providerService.UpdateProviderDetails(provider);
                            if (unflagProviderResult.IsFailure) throw new Exception($"Failed to clear the 'publish in progress' flag for provider with UK PRN {UKPRN}.");
                        }
                        // @ToDo: ELSE failure here means we need a manual way to clear the flag
                    }
                    // @ToDo: ELSE failure here means we need a manual way to clear the flag

                    var finishTimestamp = DateTime.UtcNow;
                    _logger.LogInformation($"{finishTimestamp.ToString("yyyyMMddHHmmss")} background worker {guid} finished successfully for {tag}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to publish courses from the background worker {guid} for {tag}", ex);
                }

                _logger.LogInformation($"Queued Background Task {guid} is complete.");


            });

            // @ToDo: we'll reach here before the above background task has completed, so need some UI work
            var vm = new PublishCompleteViewModel()
            {
                NumberOfCoursesPublished = model.NumberOfCourses,
                Mode = PublishMode.BulkUpload,
                BackgroundPublishInProgress = provider.BulkUploadStatus.PublishInProgress
            };
            double totalmins = Math.Max(2, (model.NumberOfCourses * _bulkUploadService.BulkUploadSecondsPerRecord / 60));
            vm.BackgroundPublishMinutes = (int)Math.Round(totalmins, 0, MidpointRounding.AwayFromZero);
            return View("../PublishCourses/InProgress", vm);
        }

        [Authorize]
        [HttpGet]
        public IActionResult LandingOptions()
        {
            return View("../BulkUpload/LandingOptions/Index", new BulkuploadLandingViewModel());
        }

        [HttpPost]
        public IActionResult LandingOptions(BulkuploadLandingViewModel model)
        {
            switch (model.BulkUploadLandingOptions)
            {
                case BulkUploadLandingOptions.Apprenticeship:
                    return RedirectToAction("Index", "BulkUploadApprenticeships");

                case BulkUploadLandingOptions.FE:
                    return RedirectToAction("Index", "BulkUpload");
                default:
                    return RedirectToAction("LandingOptions", "BulkUpload");
            }

        }

        private Provider FindProvider(int prn)
        {
            Provider provider = null;
            try
            {
                var providerSearchResult = Task.Run(async () => await _providerService.GetProviderByPRNAsync(prn.ToString())).Result;
                if (providerSearchResult.IsSuccess)
                {
                    provider = providerSearchResult.Value.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                // @ToDo: decide how to handle this
            }
            return provider;
        }
    }
}
