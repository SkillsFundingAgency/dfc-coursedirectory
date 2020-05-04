using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing
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
