using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class DeleteCourseRunHandler : ICosmosDbQueryHandler<DeleteCourseRun, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            DocumentClient client,
            Configuration configuration,
            DeleteCourseRun request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.CoursesCollectionName,
                request.CourseId.ToString());

            var partitionKey = new PartitionKey(request.ProviderUkprn);

            Course course;

            try
            {
                var query = await client.ReadDocumentAsync<Course>(
                    documentUri,
                    new RequestOptions() { PartitionKey = partitionKey });

                course = query.Document;
            }
            catch (DocumentClientException dex) when (dex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFound();
            }

            var courseRun = course.CourseRuns.SingleOrDefault(cr => cr.Id == request.CourseRunId);
            if (courseRun == null ||
                courseRun.RecordStatus == CourseStatus.Archived ||
                courseRun.RecordStatus == CourseStatus.Deleted)
            {
                return new NotFound();
            }

            courseRun.RecordStatus = CourseStatus.Archived;

            course.CourseStatus = course.CourseRuns
                .Select(cr => cr.RecordStatus)
                .Aggregate((CourseStatus)0, (l, r) => l | r);

            await client.ReplaceDocumentAsync(
                documentUri,
                course,
                new RequestOptions() { PartitionKey = partitionKey });

            return new Success();
        }
    }
}
