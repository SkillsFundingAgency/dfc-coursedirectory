using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class BulkUploadApprenticeshipsController : Controller
    {
        private readonly IApprenticeshipBulkUploadService _apprenticeshipBulkUploadService;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IBlobStorageService _blobService;
        private readonly ICourseService _courseService;
        private readonly IProviderService _providerService;
        private readonly IUserHelper _userHelper;
        private const string _blobContainerPath = "/Apprenticeship Bulk Upload/Files/";

        public BulkUploadApprenticeshipsController(
            IApprenticeshipBulkUploadService apprenticeshipBulkUploadService,
            IApprenticeshipService apprenticeshipService,
            IBlobStorageService blobService,
            ICourseService courseService,
            IProviderService providerService,
            IUserHelper userHelper)
        {
            Throw.IfNull(apprenticeshipBulkUploadService, nameof(apprenticeshipBulkUploadService));
            Throw.IfNull(blobService, nameof(blobService));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(providerService, nameof(providerService));
            Throw.IfNull(userHelper, nameof(userHelper));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));

            _apprenticeshipBulkUploadService = apprenticeshipBulkUploadService;
            _blobService = blobService;
            _courseService = courseService;
            _courseService = courseService;
            _providerService = providerService;
            _userHelper = userHelper;
            _apprenticeshipService = apprenticeshipService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Apprenticeships", "BulkUpload");
        }

        [Authorize]
        [HttpGet("/bulk-upload/apprenticeships/upload")]
        public async Task<IActionResult> Upload()
        {
            var session = HttpContext.Session;

            session.SetString("Option", "BulkUploadApprenticeships");

            int? UKPRN;
            if (session.GetInt32("UKPRN") != null)
            {
                UKPRN = session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            Provider provider = await FindProvider(UKPRN.Value);
            
            if (provider == null)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Failed to look up Provider details." });
            }

            var courseCounts = await _courseService.GetCourseCountsByStatusForUKPRN(new CourseSearchCriteria(UKPRN));

            var courseErrors = courseCounts.HasValue && courseCounts.IsSuccess ? courseCounts.Value.Where(x => (int)x.Status == (int)RecordStatus.MigrationPending  && x.Count > 0|| (int)x.Status == (int)RecordStatus.MigrationReadyToGoLive && x.Count > 0).Count() : 500;

            var model = new BulkUploadViewModel
            {
                HasMigrationErrors = courseErrors > 0 ? true : false,
            };

            if (provider.ApprenticeshipBulkUploadStatus != null)
            {
                model.BulkUploadBackgroundInProgress = provider.ApprenticeshipBulkUploadStatus.InProgress;
                model.BulkUploadBackgroundRowCount = provider.ApprenticeshipBulkUploadStatus.TotalRowCount;
                model.BulkUploadBackgroundStartTimestamp = provider.ApprenticeshipBulkUploadStatus.StartedTimestamp;
            }

            return View(model);
        }

        [Authorize]
        [HttpPost("/bulk-upload/apprenticeships/upload")]
        public async Task<IActionResult> Upload(IFormFile bulkUploadFile)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var UKPRN = HttpContext.Session.GetInt32("UKPRN");
                if (UKPRN == null)
                {
                    return RedirectToAction("Index", "Home", new {errmsg = "Please select a Provider."});
                }
                if (!Validate.ValidateFile(bulkUploadFile, out var errorMessage))
                {
                    return View(new BulkUploadViewModel {errors = new[] {errorMessage}});
                }

                await using var ms = new MemoryStream();
                bulkUploadFile.CopyTo(ms);

                ms.Position = 0;
                if (Validate.isBinaryStream(ms))
                {
                    return View(new BulkUploadViewModel {errors = new[] {"Invalid file content."}});
                }

                ApprenticeshipBulkUploadResult result = default;
                try
                {
                    ms.Position = 0;

                    result = await _apprenticeshipBulkUploadService.ValidateAndUploadCSV(
                        bulkUploadFile.FileName,
                        ms,
                        _userHelper.GetUserDetailsFromClaims(HttpContext.User.Claims, UKPRN));
                }
                catch (HeaderValidationException he)
                {
                    var errors = new[] { he.Message.FirstSentence() };
                    return View(new BulkUploadViewModel {errors = errors});
                }
                catch (BadDataException be)
                {
                    var errors = be.Message.Split(';');
                    return View(new BulkUploadViewModel {errors = errors});
                }
                catch (Exception e)
                {
                    var errors = new[] { e.Message };
                    return View(new BulkUploadViewModel {errors = errors});
                }

                if (result.Errors.Count != 0)
                {
                    var errors = result.Errors;
                    return RedirectToAction("WhatDoYouWantToDoNext", "BulkUploadApprenticeships", new
                        {
                            message = $"Your file contained {errors.Count} error{(errors.Count > 1 ? "s" : "")}. You must resolve all errors before your apprenticeship training information can be published.",
                            errorCount = errors.Count
                        }
                    );
                }

                if (result.ProcessedSynchronously)
                {
                    // All good => redirect to BulkCourses action
                    return RedirectToAction("PublishYourFile", "BulkUploadApprenticeships");
                }
                else
                {
                    return RedirectToAction("Pending");
                }
            }
            finally
            {
                sw.Stop();
            }
        }

        [Authorize]
        public IActionResult Pending()
        {
            var session = HttpContext.Session;

            session.SetString("Option", "ApprenticeshipBulkUpload");
            int? UKPRN;
            if (session.GetInt32("UKPRN") != null)
                UKPRN = session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            return View("./Pending/Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult WhatDoYouWantToDoNext(string message, int errorCount)
        {
            var model = new WhatDoYouWantToDoNextViewModel();

            if (!string.IsNullOrEmpty(message))
            {
                model.Message = message;
                model.ErrorCount = errorCount;
            }

            return View("../BulkUploadApprenticeships/WhatDoYouWantToDoNext/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult WhatDoYouWantToDoNext(WhatDoYouWantToDoNextViewModel model)
        {
            switch (model.WhatDoYouWantToDoNext)
            {
                case Models.Enums.WhatDoYouWantToDoNext.OnScreen:
                    return RedirectToAction("Index", "PublishApprenticeships");
                case Models.Enums.WhatDoYouWantToDoNext.DownLoad:
                    return RedirectToAction("DownloadErrorFile", "BulkUploadApprenticeships");
                case Models.Enums.WhatDoYouWantToDoNext.Delete:
                    return RedirectToAction("DeleteFile", "BulkUploadApprenticeships");
                default:
                    return RedirectToAction("WhatDoYouWantToDoNext", "BulkUploadApprenticeships");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult DownloadErrorFile()
        {
            var session = HttpContext.Session;

            int? UKPRN;
            if (session.GetInt32("UKPRN") != null)
                UKPRN = session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            var model = new DownloadErrorFileViewModel() { ErrorFileCreatedDate = DateTime.Now, UKPRN = UKPRN };
            IEnumerable<BlobFileInfo> list = _blobService.GetFileList(UKPRN + _blobContainerPath).OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
            {

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                DateTime dt1 = DateTime.Parse(list.FirstOrDefault().DateUploaded.Value.DateTime.ToString());
                DateTime dt2 = TimeZoneInfo.ConvertTimeFromUtc(dt1, tzi);

                model.ErrorFileCreatedDate = Convert.ToDateTime(dt2.ToString("dd MMM yyyy HH:mm"));
            }

            return View("../BulkUploadApprenticeships/DownloadErrorFile/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult DownloadErrorFile(DownloadErrorFileViewModel model)
        {
            return View("../BulkUploadApprenticeships/WhatDoYouWantToDoNext/Index", new WhatDoYouWantToDoNextViewModel());
        }

        [Authorize]
        [HttpGet]
        public IActionResult DeleteFile()
        {
            return View("../BulkUploadApprenticeships/DeleteFile/Index", new DeleteFileViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteFile(DeleteFileViewModel model)
        {
            DateTimeOffset fileUploadDate = new DateTimeOffset();
            int? sUKPRN = HttpContext.Session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
            else
            {
                UKPRN = sUKPRN ?? 0;
            }

            IEnumerable<Services.BlobStorageService.BlobFileInfo> list = _blobService.GetFileList(UKPRN + _blobContainerPath).OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
            {
                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                DateTime dt1 = DateTime.Parse(list.FirstOrDefault().DateUploaded.Value.DateTime.ToString());
                DateTime dt2 = TimeZoneInfo.ConvertTimeFromUtc(dt1, tzi);

                fileUploadDate = Convert.ToDateTime(dt2.ToString("dd MMM yyyy HH:mm"));;
                var archiveFilesResult = _blobService.ArchiveFiles($"{UKPRN.ToString()}{_blobContainerPath}");
            }

            var deleteBulkuploadResults = await _apprenticeshipService.DeleteBulkUploadApprenticeships(UKPRN);

            if (deleteBulkuploadResults.IsSuccess)
            {
                return RedirectToAction("DeleteFileConfirmation", "BulkUploadApprenticeships", new { fileUploadDate = fileUploadDate });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Delete All Bulk Uploaded Apprenticeships Error" });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult DeleteFileConfirmation(DateTimeOffset fileUploadDate)
        {
            var model = new DeleteFileConfirmationViewModel
            {
                FileUploadedDate = fileUploadDate.ToString("dd MMM yyyy HH:mm")
            };

            return View("../BulkUploadApprenticeships/DeleteFileConfirmation/Index", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PublishYourFile()
        {
            int? sUKPRN = HttpContext.Session.GetInt32("UKPRN");
            if (!sUKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var numberOfApprenticeships = 0;

            var model = new ApprenticeshipsPublishYourFileViewModel();

            var result = await _apprenticeshipService.GetApprenticeshipByUKPRN(sUKPRN.ToString());
            if (result.IsSuccess && result.HasValue)
            {
                numberOfApprenticeships =
                    result.Value.Where(x => x.RecordStatus == RecordStatus.BulkUploadReadyToGoLive).Count();
            }
            model.NumberOfApprenticeships = numberOfApprenticeships;

            return View("../BulkUploadApprenticeships/PublishYourFile/Index", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PublishYourFile(ApprenticeshipsPublishYourFileViewModel model)
        {
            int? sUKPRN = HttpContext.Session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            UKPRN = sUKPRN ?? 0;


            var resultArchivingApprenticeships = await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(
                UKPRN,
                (int)(RecordStatus.Live | RecordStatus.MigrationPending | RecordStatus.Pending | RecordStatus.MigrationReadyToGoLive),
                (int)RecordStatus.Archived);
            if (resultArchivingApprenticeships.IsFailure)
            {
                throw new Exception(resultArchivingApprenticeships.Error);
            }

            await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.BulkUploadReadyToGoLive, (int)RecordStatus.Live);

            //to publish stuff
            return View("../BulkUploadApprenticeships/Complete/Index", new ApprenticeshipsPublishCompleteViewModel() { NumberOfApprenticeshipsPublished = model.NumberOfApprenticeships, Mode = PublishMode.ApprenticeshipBulkUpload });
        }

        private async Task<Provider> FindProvider(int prn)
        {
            Provider provider = null;
            try
            {
                var providerSearchResult = await _providerService.GetProviderByPRNAsync(prn.ToString());
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