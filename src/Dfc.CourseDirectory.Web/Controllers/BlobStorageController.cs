using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.ApprenticeshipBulkUpload;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class BlobStorageController : Controller
    {
        private IApprenticeshipProvisionHelper _apprenticeshipProvisionHelper;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        private ISession _session => HttpContext.Session;

        public BlobStorageController(
                IApprenticeshipProvisionHelper apprenticeshipProvisionHelper,
                ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _apprenticeshipProvisionHelper = apprenticeshipProvisionHelper ?? throw new ArgumentNullException(nameof(apprenticeshipProvisionHelper));
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
        }

        [Authorize]
        public IActionResult Index()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            return View();
        }

        public FileStreamResult GetApprenticeshipBulkUploadTemplateFile()
        {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<CsvApprenticeship>();
            }

            stream.Position = 0;

            return new FileStreamResult(stream, MediaTypeNames.Text.Plain)
            {
                FileDownloadName = "apprenticeships bulk upload template.csv"
            };
        }

        public Task<FileStreamResult> GetCurrentApprenticeshipsTemplateFile()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return Task.FromResult<FileStreamResult>(null);
            }

            return _apprenticeshipProvisionHelper.DownloadCurrentApprenticeshipProvisions();
        }

        [Authorize]
        public async Task<FileStreamResult> GetApprenticeshipBulkUploadErrors(int? UKPRN)
        {
            if (!UKPRN.HasValue)
            {
                return null;
            }

            var apprenticeships = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeships
            {
                Predicate = a =>
                    a.ProviderUKPRN == UKPRN
                    && ((a.RecordStatus & (int)RecordStatus.BulkUploadPending) > 0 || (a.RecordStatus & (int)RecordStatus.BulkUploadReadyToGoLive) > 0)
            });

            var apprenticeshipBUErrors = apprenticeships.Values.Where(x => x.BulkUploadErrors != null).SelectMany(y => y.BulkUploadErrors).ToList();

            IEnumerable<string> headers = new string[] { "Row Number,Column Name,Error Description" };
            IEnumerable<string> csvlines = apprenticeshipBUErrors.Select(i => string.Join(",", new string[] { i.LineNumber.ToString(), i.Header, i.Error.Replace(',', ' ') }));
            string report = string.Join(Environment.NewLine, headers.Concat(csvlines));
            byte[] data = Encoding.ASCII.GetBytes(report);
            MemoryStream ms = new MemoryStream(data)
            {
                Position = 0
            };

            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"Bulk_upload_errors_{UKPRN}_{d.Day:D2}_{d.Month:D2}_{d.Year}_{d.Hour:D2}_{d.Minute:D2}.csv";
            return result;
        }
    }
}
