using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class DeleteCourseRunHandler : ICosmosDbQueryHandler<DeleteCourseRun, OneOf<NotFound, Success>>
    {
        public OneOf<NotFound, Success> Execute(InMemoryDocumentStore inMemoryDocumentStore, DeleteCourseRun request)
        {
            var course = inMemoryDocumentStore.Courses.All.SingleOrDefault(c => c.Id == request.CourseId);
            if (course == null)
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

            inMemoryDocumentStore.Courses.Save(course);

            return new Success();
        }
    }
}
