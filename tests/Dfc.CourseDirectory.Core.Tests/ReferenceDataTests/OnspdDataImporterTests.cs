using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class OnspdDataImporterTests : DatabaseTestBase
    {
        public OnspdDataImporterTests(Testing.DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

    }
}
