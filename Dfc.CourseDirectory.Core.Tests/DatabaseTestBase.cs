using Dfc.CourseDirectory.Testing;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests
{
    [CollectionDefinition("Database")]
    public class DatabaseTestCollection : ICollectionFixture<DatabaseTestBaseFixture>
    {
    }

    [Collection("Database")]
    public abstract class DatabaseTestBase : Testing.DatabaseTestBase
    {
        public DatabaseTestBase(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }
    }
}
