using Dfc.CourseDirectory.Models.Models.Venues;
using Xunit;

namespace Dfc.CourseDirectory.Models.Test
{
    class VenueTests
    {
        public void Create_And_Assign_Values()
        {
            Venue venue = new Venue("", 2, 2, 2, "", "", "", "", "", "", "", "");
            Assert.NotNull(venue);
        }
    
    }
}
