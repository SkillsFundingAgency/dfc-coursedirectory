using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class UpdateHidePassedNotification : ISqlQuery<OneOf<NotFound, Success>>
    {
        public int ApprenticeshipQASubmissionId { get; set; }
        public bool HidePassedNotification { get; set; }
    }
}