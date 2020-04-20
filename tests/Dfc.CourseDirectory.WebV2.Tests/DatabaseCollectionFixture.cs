using Dfc.CourseDirectory.Testing;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    [CollectionDefinition("Database")]
    public class DatabaseCollectionFixture : ICollectionFixture<DatabaseTestBaseFixture>
    {
    }

    [Collection("Database")]
    public abstract class DatabaseTestBase : Testing.DatabaseTestBase
    {
        protected DatabaseTestBase(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }
    }
}
