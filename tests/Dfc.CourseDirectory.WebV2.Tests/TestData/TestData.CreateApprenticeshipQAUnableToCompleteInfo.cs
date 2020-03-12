using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public Task CreateApprenticeshipQAUnableToCompleteInfo(
            Guid providerId,
            ApprenticeshipQAUnableToCompleteReasons unableToCompleteReasons,
            string comments,
            string addedByUserId,
            DateTime addedOn) => WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new CreateApprenticeshipQAUnableToCompleteInfo()
                {
                    ProviderId = providerId,
                    UnableToCompleteReasons = unableToCompleteReasons,
                    Comments = comments,
                    AddedByUserId = addedByUserId,
                    AddedOn = addedOn
                }));
    }
}
