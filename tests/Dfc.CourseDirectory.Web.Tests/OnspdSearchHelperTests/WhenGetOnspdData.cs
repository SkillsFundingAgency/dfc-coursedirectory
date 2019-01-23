using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Models.Onspd;
using Dfc.CourseDirectory.Services.Interfaces.OnspdService;
using Dfc.CourseDirectory.Services.OnspdService;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.Azure.Search;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests.OnspdSearchHelperTests
{
    public class WhenGetOnspdData
    {
        private const string ValidPostcode = "CV1 2QT";
        private const string InvalidPostcode = "B1 0LJ";

        [Fact]
        public void WithValidPostcodeThenOneOnspdObjectReturned()
        {
            // Arrange
            var criteria = new OnspdSearchCriteria(ValidPostcode);
            var settings = GetOnspdSearchSettings();
            var result = OnspdResultFound();

            var service = new Mock<IOnspdService>();
            service.Setup(x => x.GetOnspdData(criteria)).Returns(result);

            var onspdData = OnspdFound();
            service.Setup(x => x.RunQuery(new SearchIndexClient(settings.SearchServiceName, settings.IndexName, new SearchCredentials(settings.SearchServiceQueryApiKey)), "CV1 2QT")).Returns(onspdData);

            // Act
            var helper = new OnspdSearchHelper(service.Object);
            var onspdResult = helper.GetOnsPostcodeData(ValidPostcode);

            // Assert
            Assert.NotNull(onspdResult);
        }

        [Fact]
        public void WithValidPostcodeThenValidLatLongReturned()
        {
            // Arrange
            var criteria = new OnspdSearchCriteria(ValidPostcode);
            var settings = GetOnspdSearchSettings();
            var result = OnspdResultFound();

            var service = new Mock<IOnspdService>();
            service.Setup(x => x.GetOnspdData(criteria)).Returns(result);

            var onspdData = OnspdFound();
            service.Setup(x => x.RunQuery(new SearchIndexClient(settings.SearchServiceName, settings.IndexName, new SearchCredentials(settings.SearchServiceQueryApiKey)), "CV1 2QT")).Returns(onspdData);

            // Act
            var helper = new OnspdSearchHelper(service.Object);
            var onspdResult = helper.GetOnsPostcodeData(ValidPostcode);

            // Assert
            Assert.NotEqual(0, onspdResult.lat);
            Assert.NotEqual(0, onspdResult.@long);
        }

        [Fact]
        public void WithInvalidPostcodeThenZeroLatLongReturned()
        {
            // Arrange
            var criteria = new OnspdSearchCriteria(InvalidPostcode);
            var settings = GetOnspdSearchSettings();
            var result = OnspdResultNotFound();

            var service = new Mock<IOnspdService>();
            service.Setup(x => x.GetOnspdData(criteria)).Returns(result);

            var onspdData = OnspdNotFound();
            service.Setup(x => x.RunQuery(new SearchIndexClient(settings.SearchServiceName, settings.IndexName, new SearchCredentials(settings.SearchServiceQueryApiKey)), "CV1 2QT")).Returns(onspdData);

            // Act
            var helper = new OnspdSearchHelper(service.Object);
            var onspdResult = helper.GetOnsPostcodeData(InvalidPostcode);

            // Assert
            Assert.Equal(0, onspdResult.lat);
            Assert.Equal(0, onspdResult.@long);
        }

        private IResult<IOnspdSearchResult> OnspdResultFound()
        {
            var onspdData = OnspdFound();
            var searchResult = new OnspdSearchResult(onspdData)
            {
                Value = onspdData
            };
            return Result.Ok<IOnspdSearchResult>(searchResult);
        }

        private Onspd OnspdFound()
        {
            return new Onspd { lat = 12345.88M, @long = 8974.23M };
        }

        private Onspd OnspdNotFound()
        {
            return new Onspd { lat = 0, @long = 0 };
        }

        private IResult<IOnspdSearchResult> OnspdResultNotFound()
        {
            var onspdData = OnspdNotFound();
            var searchResult = new OnspdSearchResult(onspdData)
            {
                Value = onspdData
            };
            return Result.Ok<IOnspdSearchResult>(searchResult);
        }

        private OnspdSearchSettings GetOnspdSearchSettings()
        {
            return new OnspdSearchSettings
            {
                SearchServiceName = "abc-def-ghij-klm",
                SearchServiceAdminApiKey = "Pretend Admin Key",
                SearchServiceQueryApiKey = "Pretend Query Key",
                IndexName = "abcd"
            };
        }
    }
}
