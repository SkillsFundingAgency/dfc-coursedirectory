namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetExclusiveLock : ISqlQuery<bool>
    {
        public string Name { get; set; }
        public int TimeoutMilliseconds { get; set; }
    }
}
