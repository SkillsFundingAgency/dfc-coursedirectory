
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;


namespace Dfc.CourseDirectory.Web.Controllers
{

    public class BlobStorageController : Controller
    {
        private readonly ILogger<BlobStorageController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICourseService _courseService;
        private readonly IBlobStorageService _blobService;

        //private IHostingEnvironment _env;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public BlobStorageController(
                ILogger<BlobStorageController> logger,
                IHttpContextAccessor contextAccessor,
                ICourseService courseService,
                IBlobStorageService blobService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(blobService, nameof(blobService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _blobService = blobService;
        }

        [Authorize]
        public IActionResult Index()
        {
            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            //var vm = GetBlobStorageViewModel(_courseService, UKPRN, "");
            return View(); //vm);
        }

        [Authorize]
        public FileStreamResult GetErrors(int? UKPRN)
        {
            //string connstring = "DefaultEndpointsProtocol=https;AccountName=dfcdevprovstr;AccountKey=AXw/m1t6k0r0sbwIiZQUn7OjbCOrycxJwZjsXf27Az5+RjGHkovPSZ+MwBH//PPxCNLWbNXT38xVQh1WDKLYnw==";
            if (!UKPRN.HasValue)
                return null;

            //StringBuilder sb = new StringBuilder();
            //sb.Append($"ID,Row Number,Column Name,Error Description{Environment.NewLine}");
            //sb.Append($"1,abc,def{Environment.NewLine}");
            //sb.Append($"2,ghi,jkl{Environment.NewLine}");
            //sb.Append($"3,uvw,xyz{Environment.NewLine}");

            IEnumerable<Course> courses = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                                        .Result
                                                        .Value
                                                        .Value
                                                        .SelectMany(o => o.Value)
                                                        .SelectMany(i => i.Value);

            //int[] pendingStatuses = new int[] { (int)RecordStatus.Pending, (int)RecordStatus.BulkUploadPending, (int)RecordStatus.APIPending, (int)RecordStatus.MigrationPending, (int)RecordStatus.MigrationReadyToGoLive, (int)RecordStatus.BulkUploadReadyToGoLive };
            //int[] bulkStatuses = new int[] { (int)RecordStatus.BulkUploadPending };
            //IEnumerable<Course> validCourses = courses.Where(c => c.IsValid);

            IEnumerable<CourseValidationResult> results = _courseService.CourseValidationMessages(courses, ValidationMode.EditCourseBU).Value;

            int counter = 1;
            IEnumerable<string> headers = new string[] { "ID,Row Number,Column Name,Error Description" };
            IEnumerable<string> csvlines = results.SelectMany(r => r.Issues
                                                                    .Select(i => string.Join(",", new string[] { counter.ToString(), counter++.ToString(), i, i } ))
                                                             );
            string report = string.Join(Environment.NewLine, headers.Concat(csvlines));



            byte[] data = Encoding.ASCII.GetBytes(report); // sb.ToString());
            MemoryStream ms = new MemoryStream(data);
            Task task = _blobService.UploadFileAsync($"{UKPRN.ToString()}/Bulk Upload/Files/uploadtest.csv", ms);
            task.Wait();

            ms = new MemoryStream(data);
            task = _blobService.DownloadFileAsync($"{UKPRN.ToString()}/Bulk Upload/Files/test.csv", ms);
            task.Wait();
            ms.Position = 0;
            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            result.FileDownloadName = "This is a file downloaded from memory.csv";
            return result;
        }
    }
}
