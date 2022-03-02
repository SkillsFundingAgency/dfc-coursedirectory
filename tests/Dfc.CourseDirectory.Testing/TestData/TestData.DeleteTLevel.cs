using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task DeleteTLevel(Guid tLevelId, UserInfo deletedBy)
        {
            return WithSqlQueryDispatcher(dispatcher =>
            {
                return dispatcher.ExecuteQuery(new DeleteTLevel
                {
                    TLevelId = tLevelId,
                    DeletedBy = deletedBy,
                    DeletedOn = _clock.UtcNow
                });
            });
        }       
    }
}
