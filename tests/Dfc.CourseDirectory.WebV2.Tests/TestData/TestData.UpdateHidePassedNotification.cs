using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Tests
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