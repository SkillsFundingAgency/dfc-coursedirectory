using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.BackgroundWorkers;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.Validation;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneOf.Types;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class BulkUploadController : Controller
    {
        private readonly IBulkUploadService _bulkUploadService;
        private readonly IBlobStorageService _blobService;
        private readonly ICourseService _courseService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IBackgroundWorkScheduler _backgroundWorkScheduler;
        private readonly ILogger<BulkUploadController> _logger;
        private readonly IProviderContextProvider _providerContextProvider;

        private ISession _session => HttpContext.Session;

        public BulkUploadController(
            IBulkUploadService bulkUploadService,
            IBlobStorageService blobService,
            ICourseService courseService,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IBackgroundWorkScheduler backgroundWorkScheduler,
            ILogger<BulkUploadController> logger,
            IProviderContextProvider providerContextProvider)
        {
            _bulkUploadService = bulkUploadService ?? throw new ArgumentNullException(nameof(bulkUploadService));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _backgroundWorkScheduler = backgroundWorkScheduler ?? throw new ArgumentNullException(nameof(backgroundWorkScheduler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _providerContextProvider = providerContextProvider;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Courses", "BulkUpload")
                .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true));
        }

        [Authorize]
        [HttpGet("/bulk-upload/courses/upload")]
        public async Task<IActionResult> Upload()
        {
            _session.SetString("Option", "BulkUpload");
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = UKPRN.Value });
            if (null == provider)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Failed to look up Provider details." });
            }

            var courseCounts = _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(UKPRN)).Result;
            var courseErrors = courseCounts.IsSuccess
                ? courseCounts.Value.Where(x => (int)x.Status == (int)RecordStatus.MigrationPending && x.Count > 0 || (int)x.Status == (int)RecordStatus.MigrationReadyToGoLive && x.Count > 0).Count()
                : 500;

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

            if (!deleteResult.IsSuccess)
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
                case Services.Models.WhatDoYouWantToDoNext.OnScreen:
                    return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.BulkUpload, fromBulkUpload });
                case Services.Models.WhatDoYouWantToDoNext.DownLoad:
                    return RedirectToAction("DownloadErrorFile", "BulkUpload");
                case Services.Models.WhatDoYouWantToDoNext.Delete:
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
            return View("../Bulkupload/WhatDoYouWantToDoNext/Index", new WhatDoYouWantToDoNextViewModel());
        }

        [Authorize]
        [HttpGet]
        public IActionResult DeleteFile()
        {
            return View("../Bulkupload/DeleteFile/Index", new DeleteFileViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteFile(DeleteFileViewModel model)
        {
            var UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = UKPRN.Value });

            if (provider == null)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Failed to find Provider data to delete bulk upload." });
            }

            var blobFiles = _blobService.GetFileList(UKPRN + "/Bulk Upload/Files/").OrderByDescending(x => x.DateUploaded).ToList();
            
            if (blobFiles.Any())
            {
                _blobService.ArchiveFiles($"{UKPRN}/Bulk Upload/Files/");
            }

            // COUR-1927 move the delete to a background worker because it's timing out for large files.
            static async Task DeleteBulkUploadCoursesWorker(object state, IServiceProvider services, CancellationToken ct)
            {
                if (!(state is int UKPRN))
                {
                    throw new ArgumentException($"{nameof(state)} must be of type {nameof(Int32)} and cannot be null.", nameof(state));
                }

                var courseService = services.GetRequiredService<ICourseService>();
                var logger = services.GetRequiredService<ILogger<BulkUploadController>>();

                var guid = Guid.NewGuid().ToString();
                var tag = $"delete bulk upload for provider {UKPRN}.";

                try
                {
                    logger.LogInformation($"{DateTime.UtcNow:yyyyMMddHHmmss} Starting background worker {guid} for {tag}");

                    var deleteBulkuploadResults = await courseService.DeleteBulkUploadCourses(UKPRN);

                    if (!deleteBulkuploadResults.IsSuccess)
                    {
                        throw new Exception($"{nameof(DeleteBulkUploadCoursesWorker)} failed with error {deleteBulkuploadResults.Error}");
                    }

                    logger.LogInformation($"{DateTime.UtcNow:yyyyMMddHHmmss} background worker {guid} finished successfully for {tag}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error whilst deleting bulk upload file on background worker {guid} for {tag}");
                    throw;
                }
            }

            var workerCompleted = await _backgroundWorkScheduler.ScheduleAndWait(DeleteBulkUploadCoursesWorker, TimeSpan.FromSeconds(5), UKPRN);

            if (workerCompleted)
            {
                return RedirectToAction("DeleteFileConfirmation", "Bulkupload", new
                {
                    // COUR-1972 make sure we get a date on the Delete Confirmation page even if the physical delete above didn't find any files to delete.
                    fileUploadDate = blobFiles.FirstOrDefault()?.DateUploaded
                        ?? provider.BulkUploadStatus?.StartedTimestamp?.ToLocalTime()
                        ?? new DateTimeOffset()
                });
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

            // COUR-1864
            // Offload this long running activity to a background task.
            // @See:  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2&tabs=visual-studio

            // Find the provider and flag it as "publish in progress" outside of the queue otherwise the time lag is too great and
            // the UI displays incorrect info

            // Flag the provider as "publish in progress".
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = UKPRN });

            if (provider == null)
            {
                throw new Exception($"Failed to find provider with UK PRN {UKPRN}");
            }

            var flagProviderResult = await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderBulkUploadStatus { ProviderId = provider.Id, PublishInProgress = true });

            if (!(flagProviderResult.Value is Success))
            {
                throw new Exception($"Failed to set the 'publish in progress' flag for provider with UK PRN {UKPRN}.");
            }

            static async Task PublishCoursesWorker(object state, IServiceProvider services, CancellationToken ct)
            {
                if (!(state is (Provider provider, PublishYourFileViewModel model)))
                {
                    throw new ArgumentException($"{nameof(state)} must be of type {nameof(ValueTuple<Provider, PublishYourFileViewModel>)} and cannot be null.", nameof(state));
                }

                var courseService = services.GetRequiredService<ICourseService>();
                var cosmosDbQueryDispatcher = services.GetRequiredService<ICosmosDbQueryDispatcher>();
                var logger = services.GetRequiredService<ILogger<BulkUploadController>>();

                var guid = Guid.NewGuid().ToString();
                var tag = $"bulk upload publish for provider {provider.Ukprn} for {model.NumberOfCourses} courses.";
                var startTimestamp = DateTime.UtcNow;

                try
                {
                    logger.LogInformation($"{startTimestamp.ToString("yyyyMMddHHmmss")} Starting background worker {guid} for {tag}");

                    // Publish the bulk-uploaded courses.

                    var resultArchivingCourses = await courseService.ArchiveCoursesExceptBulkUploadReadytoGoLive(provider.Ukprn, (int)RecordStatus.Archived);
                    if (resultArchivingCourses.IsSuccess)
                    {
                        var resultGoingLive = await courseService.ChangeCourseRunStatusesForUKPRNSelection(provider.Ukprn, (int)RecordStatus.BulkUploadReadyToGoLive, (int)RecordStatus.Live);
                        if (resultGoingLive.IsSuccess)
                        {
                            // Clear the provider "publish in progress" flag.
                            var unflagProviderResult = await cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderBulkUploadStatus { ProviderId = provider.Id, PublishInProgress = false });

                            if (!(unflagProviderResult.Value is Success))
                            {
                                throw new Exception($"Failed to clear the 'publish in progress' flag for provider with UK PRN {provider.Ukprn}.");
                            }
                        }
                        // @ToDo: ELSE failure here means we need a manual way to clear the flag
                    }
                    // @ToDo: ELSE failure here means we need a manual way to clear the flag

                    var finishTimestamp = DateTime.UtcNow;
                    logger.LogInformation($"{finishTimestamp.ToString("yyyyMMddHHmmss")} background worker {guid} finished successfully for {tag}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to publish courses from the background worker {guid} for {tag}");
                }

                logger.LogInformation($"Queued Background Task {guid} is complete.");
            }

            // Now queue the background work to publish the courses.
            await _backgroundWorkScheduler.Schedule(PublishCoursesWorker, (provider, model));

            // @ToDo: we'll reach here before the above background task has completed, so need some UI work
            var vm = new PublishCompleteViewModel()
            {
                NumberOfCoursesPublished = model.NumberOfCourses,
                Mode = PublishMode.BulkUpload
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
    }
}
