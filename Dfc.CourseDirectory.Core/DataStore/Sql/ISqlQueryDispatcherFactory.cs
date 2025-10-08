using System.Data;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public interface ISqlQueryDispatcherFactory
    {
        ISqlQueryDispatcher CreateDispatcher(IsolationLevel isolationLevel = IsolationLevel.Snapshot);
    }
}
