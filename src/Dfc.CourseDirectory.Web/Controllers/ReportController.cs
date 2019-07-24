using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Admin")]
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;
        private readonly ICourseService _courseService;
        private ICSVHelper _CSVHelper;
        public ReportController(ILogger<ReportController> logger, ICourseService courseService, ICSVHelper csvHelper)
        {
            _logger = logger;
            _courseService = courseService;
            _CSVHelper = csvHelper;
        }

        
        public async Task<IActionResult> Index()
        {
            var reportResults = await _courseService.GetAllDfcReports();

            if (reportResults.IsFailure) throw new Exception("Unable to generate migration reports");

            var csvReports = reportResults.Value.Select(x => new CsvDfcMigrationReport
            {
                FailedMigrationCount = x.FailedMigrationCount,
                LiveCount = x.LiveCount,
                MigratedCount = x.MigratedCount,
                MigrationDate = x.MigrationDate,
                MigrationPendingCount = x.MigrationPendingCount,
                MigrationRate = x.MigrationRate,
                ProviderName = x.ProviderName,
                ProviderType = x.ProviderType
            });

            List<string> csvLines = new List<string>();
            foreach (var line in _CSVHelper.ToCsv(csvReports))
            {
                csvLines.Add(line);
            }

            string report = string.Join(Environment.NewLine, csvLines);
            byte[] data = Encoding.ASCII.GetBytes(report);
            MemoryStream ms = new MemoryStream(data)
            {
                Position = 0
            };
            FileStreamResult result = new FileStreamResult(ms, MediaTypeNames.Text.Plain);
            DateTime d = DateTime.Now;
            result.FileDownloadName = $"DFc_Migration_Report_{d.Day.TwoChars()}_{d.Month.TwoChars()}_{d.Year}_{d.Hour.TwoChars()}_{d.Minute.TwoChars()}.csv";
            return result;

        }
    }
}