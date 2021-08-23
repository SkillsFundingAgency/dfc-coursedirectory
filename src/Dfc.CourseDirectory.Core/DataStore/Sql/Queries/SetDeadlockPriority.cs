using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetDeadlockPriority : ISqlQuery<None>
    {
        public int Priority { get; set; }
    }
}
