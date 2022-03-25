using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ExpiredCourseRuns
{
    public class ExpiredCourseRunsTests : MvcTestBase
    {
        public ExpiredCourseRunsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }
    }
}
