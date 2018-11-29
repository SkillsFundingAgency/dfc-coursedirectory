using Dfc.CourseDirectory.Web.ViewComponents.VenueSearch;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class VenueListUnitTests
    {
        [Fact]
        public void VenueList_Returns_A_List_Of_Venues()
        {
            var viewComponent = new VenueSearch();

            //Act
            var result = viewComponent.Invoke() as ViewViewComponentResult;

            //Assert
            Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(result);
        }
    }
}