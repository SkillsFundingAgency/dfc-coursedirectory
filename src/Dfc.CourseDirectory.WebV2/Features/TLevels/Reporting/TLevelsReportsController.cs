using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.Reporting
{
    [RequireFeatureFlag(FeatureFlags.TLevels)]
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    [Route("t-levels/reports")]
    public class TLevelsReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IClock _clock;

        public TLevelsReportsController(IMediator mediator, IClock clock)
        {
            _mediator = mediator;
            _clock = clock;
        }

        [HttpGet("live-t-levels")]
        public async Task LiveTLevelsReport()
        {
            var results = await _mediator.Send(new LiveTLevelsReport.Query());

            Response.Headers.Add(HeaderNames.ContentType, "text/csv");
            Response.Headers.Add(
                HeaderNames.ContentDisposition,
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"{nameof(LiveTLevelsReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv"
                }.ToString());

            await using (var stream = Response.Body)
            await using (var writer = new StreamWriter(stream))
            await using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteHeader<LiveTLevelsReport.Csv>();
                await csvWriter.NextRecordAsync();
                await writer.FlushAsync();

                await foreach (var result in results)
                {
                    csvWriter.WriteRecord(result);
                    await csvWriter.NextRecordAsync();
                    
                    await writer.FlushAsync();
                }
            }
        }
    }
}
