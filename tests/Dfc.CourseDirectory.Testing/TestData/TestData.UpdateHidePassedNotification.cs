using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task UpdateHidePassedNotification(
            int apprenticeshipQASubmissionId, bool hideNotificationn) => WithSqlQueryDispatcher(
            dispatcher => dispatcher.ExecuteQuery(
            new UpdateHidePassedNotification()
            {
                ApprenticeshipQASubmissionId = apprenticeshipQASubmissionId,
                HidePassedNotification = hideNotificationn
            }));
    }
}