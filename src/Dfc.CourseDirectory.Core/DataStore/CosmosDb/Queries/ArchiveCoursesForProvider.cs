namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ArchiveCoursesForProvider : ICosmosDbQuery<int>
    {
        public int Ukprn { get; set; }
    }
}
