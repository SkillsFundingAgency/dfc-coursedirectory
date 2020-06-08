namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ArchiveApprenticeshipsForProvider : ICosmosDbQuery<int>
    {
        public int Ukprn { get; set; }
    }
}
