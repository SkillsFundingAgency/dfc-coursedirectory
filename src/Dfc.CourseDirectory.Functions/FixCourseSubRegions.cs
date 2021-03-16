using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class FixCourseSubRegions
    {
        private readonly DocumentClient _documentClient;
        private readonly IRegionCache _regionCache;

        public FixCourseSubRegions(DocumentClient documentClient, IRegionCache regionCache)
        {
            _documentClient = documentClient;
            _regionCache = regionCache;
        }

        [FunctionName(nameof(FixCourseSubRegions))]
        [NoAutomaticTrigger]
        public async Task Run(string input)
        {
            const string databaseId = "providerportal";
            const string collectionId = "courses";

            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                databaseId,
                collectionId);

            var allRegions = await _regionCache.GetAllRegions();

            var query = _documentClient.CreateDocumentQuery<Course>(
                    collectionUri,
                    new FeedOptions() { EnableCrossPartitionQuery = true, MaxItemCount = -1 })
                .Where(c => c.CourseRuns.Any(r => r.Regions.Any() && !r.SubRegions.Any()))
                .AsDocumentQuery();

            await query.ProcessAll(async chunk =>
            {
                foreach (var course in chunk)
                {
                    foreach (var courseRun in course.CourseRuns.Where(r => r.Regions?.Any() == true && r.SubRegions?.Any() != true))
                    {
                        // Bulk upload historically hasn't populated `SubRegions` - fix that here.
                        // `Regions` can contain both regions and sub-regions. `SubRegions` should contain regions
                        // where every sub-region in that region is applicable and sub-regions otherwise.

                        var newSubRegions = Region.Reduce(allRegions, courseRun.Regions);

                        courseRun.SubRegions = newSubRegions.Select(r => new CourseRunSubRegion()
                        {
                            Id = r.Id,
                            Latitude = r.Latitude,
                            Longitude = r.Longitude,
                            SubRegionName = r.Name
                        }).ToList();
                    }

                    var documentLink = UriFactory.CreateDocumentUri(databaseId, collectionId, course.Id.ToString());

                    await _documentClient.ReplaceDocumentAsync(
                        documentLink,
                        course,
                        new RequestOptions()
                        {
                            PartitionKey = new Microsoft.Azure.Documents.PartitionKey(course.ProviderUKPRN)
                        });
                }
            });
        }
    }
}
