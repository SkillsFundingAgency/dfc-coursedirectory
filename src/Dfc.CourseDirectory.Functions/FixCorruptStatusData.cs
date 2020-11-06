using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    /// <summary>
    /// In prod CourseStatus has got out of sync with the CourseRuns
    /// </summary>
    public class FixCorruptStatusData
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly DocumentClient _documentClient;

        public FixCorruptStatusData(
            Configuration configuration,
            ILoggerFactory loggerFactory,
            DocumentClient documentClient)
        {
            _configuration = configuration;
            _documentClient = documentClient;
            _logger = loggerFactory.CreateLogger<FixCorruptStatusData>();
        }

        [FunctionName(nameof(FixCorruptStatusData))]
        [NoAutomaticTrigger]
        public async Task Run(string input)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                _configuration.DatabaseId,
                _configuration.CoursesCollectionName);

            var query = _documentClient
                .CreateDocumentQuery<Course>(collectionUri,
                    new FeedOptions() {EnableCrossPartitionQuery = true})
                .Where(c => c.CourseStatus == CourseStatus.BulkUploadReadyToGoLive)
                .AsDocumentQuery();

            int fixCount = 0;
            await query.ProcessAll(async courses =>
            {
                foreach (var course in courses.Where(c => c.CourseRuns.All(r => r.RecordStatus == CourseStatus.Live)))
                {
                    course.CourseStatus = CourseStatus.Live;

                    var documentUri = UriFactory.CreateDocumentUri(
                        _configuration.DatabaseId,
                        _configuration.CoursesCollectionName,
                        course.Id.ToString());

                    _logger.LogDebug($"Updating course with correct status {documentUri}");
                    await _documentClient.ReplaceDocumentAsync(documentUri, course,
                        new RequestOptions
                            {AccessCondition = new AccessCondition {Type = AccessConditionType.IfMatch, Condition = course.ETag}});
                    fixCount++;
                }
            });
            _logger.LogInformation($"{nameof(FixCorruptStatusData)}: Fixed {fixCount} corrupt course statuses.");
        }
    }
}
