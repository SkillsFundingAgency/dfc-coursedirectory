using Dfc.CourseDirectory.Web.ViewComponents.VenueList;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class VenueListUnitTests
    {
        [Fact]
        public void VenueList_Returns_A_List_Of_Venues()
        {
            var viewComponent = new VenueList();

            //Act
            var result = viewComponent.Invoke(new VenueListModel()) as ViewViewComponentResult;

            //Assert
            Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(result);
        }
    }
}