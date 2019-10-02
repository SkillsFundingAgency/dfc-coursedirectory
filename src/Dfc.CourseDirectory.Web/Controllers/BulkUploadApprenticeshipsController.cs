using System;
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
using Dfc.CourseDirectory.Models.Models.Auth;
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
    public class BulkUploadApprenticeshipsController : Controller
    {
        private readonly ILogger<BulkUploadApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IApprenticeshipBulkUploadService _apprenticeshipBulkUploadService;
        private readonly IBlobStorageService _blobService;
        private readonly ICourseService _courseService;
        private readonly IProviderService _providerService;
        private readonly IUserHelper _userHelper;
        private IHostingEnvironment _env;
        private const string _blobContainerPath = "/Apprenticeships Bulk Upload/Files/";
        private ISession _session => _contextAccessor.HttpContext.Session;

        public BulkUploadApprenticeshipsController(
            ILogger<BulkUploadApprenticeshipsController> logger,
            IHttpContextAccessor contextAccessor,
            IApprenticeshipBulkUploadService apprenticeshipBulkUploadService,
            IBlobStorageService blobService,
            ICourseService courseService,
            IHostingEnvironment env,
            IProviderService providerService,
            IUserHelper userHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(apprenticeshipBulkUploadService, nameof(apprenticeshipBulkUploadService));
            Throw.IfNull(blobService, nameof(blobService));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(env, nameof(env));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(providerService, nameof(providerService));
            Throw.IfNull(userHelper, nameof(userHelper));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _apprenticeshipBulkUploadService = apprenticeshipBulkUploadService;
            _blobService = blobService;
            _courseService = courseService;
            _env = env;
            _courseService = courseService;
            _providerService = providerService;
            _userHelper = userHelper;
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

            if(null != provider.ApprenticeshipBulkUploadStatus)
            {
                model.BulkUploadBackgroundInProgress = provider.ApprenticeshipBulkUploadStatus.InProgress;
                model.BulkUploadBackgroundRowCount = provider.ApprenticeshipBulkUploadStatus.TotalRowCount;
                model.BulkUploadBackgroundStartTimestamp = provider.ApprenticeshipBulkUploadStatus.StartedTimestamp;
            }
            return View("Index",model);
        }
         
        [Authorize]
        public IActionResult Pending()
        {
            _session.SetString("Option", "ApprenticeshipBulkUpload");
            int? UKPRN;
            if (_session.GetInt32("UKPRN") != null)
                UKPRN = _session.GetInt32("UKPRN").Value;
            else
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            return View("./Pending/Index");
        }

        [Authorize] 
        [HttpPost]
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
                     Path.GetFileName(bulkUploadFile.FileName));

                MemoryStream ms = new MemoryStream();
                bulkUploadFile.CopyTo(ms);

                if (!Validate.isBinaryStream(ms))
                {
                    int csvLineCount = _apprenticeshipBulkUploadService.CountCsvLines(ms);
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
                        $"{UKPRN.ToString()}/Apprenticeship Bulk Upload/Files/{bulkUploadFileNewName}", ms);
                    task.Wait();

                    List<string> errors = new List<string>();
                    try
                    {
                        errors = _apprenticeshipBulkUploadService.ValidateAndUploadCSV(ms, _userHelper.GetUserDetailsFromClaims(this.HttpContext.User.Claims, UKPRN));
                    }
                    catch (Exception e)
                    {
                        errors.Add(e.Message.FirstSentence());
                        vm.errors = errors;
                        return View(vm);
                    }

                    
                    if (processInline)
                    {
                        
                        if (errors.Any())
                        {
                            return RedirectToAction("WhatDoYouWantToDoNext", "BulkUploadApprenticeships",
                                new { publishMode = PublishMode.BulkUpload, fromBulkUpload = true });
                        }
                        // All good => redirect to BulkApprenticeship action
                        return RedirectToAction("Index", "PublishApprenticeships",
                            new PublishViewModel
                            {
                                NumberOfCoursesInFiles = csvLineCount - 1
                            });

                    }

                    return RedirectToAction("Pending");
                    
                    

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

        
       

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> WhatDoYouWantToDoNext(string message)
        {
            var model = new WhatDoYouWantToDoNextViewModel();

            if (!string.IsNullOrEmpty(message))
            {
                model.Message = message;
            }
           
            return View("../BulkUploadApprenticeships/WhatDoYouWantToDoNext/Index", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> WhatDoYouWantToDoNext(WhatDoYouWantToDoNextViewModel model)
        {
            bool fromBulkUpload = !string.IsNullOrEmpty(model.Message);

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
            IEnumerable<BlobFileInfo> list = _blobService.GetFileList(UKPRN + _blobContainerPath).OrderByDescending(x => x.DateUploaded).ToList();
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

            IEnumerable<Services.BlobStorageService.BlobFileInfo> list = _blobService.GetFileList(UKPRN + _blobContainerPath).OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
            {
                fileUploadDate = list.FirstOrDefault().DateUploaded.Value;
                var archiveFilesResult = _blobService.ArchiveFiles($"{UKPRN.ToString()}{_blobContainerPath}");
            }

            //TODO: GB DeleteBulkUploadCourses modify for Apprenticeships
            var deleteBulkuploadResults = await _courseService.DeleteBulkUploadCourses(UKPRN);

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
        public async Task<IActionResult> DeleteFileConfirmation(DateTimeOffset fileUploadDate)
        {
            var model = new DeleteFileConfirmationViewModel();

            DateTime localDateTime = DateTime.Parse(fileUploadDate.ToString());
            DateTime utcDateTime = localDateTime.ToUniversalTime();

            model.FileUploadedDate = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local).ToString("dd MMM yyyy HH:mm");

            return View("../BulkUploadApprenticeships/DeleteFileConfirmation/Index", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PublishYourFile(int NumberOfApprenticships)
        {
            var model = new ApprenticeshipsPublishYourFileViewModel();

            model.NumberOfApprenticeships = NumberOfApprenticships;

            return View("../BulkUploadApprenticeships/PublishYourFile/Index", model);
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