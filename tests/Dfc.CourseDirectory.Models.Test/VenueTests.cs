using System;
using Dfc.CourseDirectory.Models.Models.Venues;
using Xunit;

namespace Dfc.CourseDirectory.Models.Test
{
    public class VenueTests
    {
        [Fact]
        public void Create_And_Assign_Values()
        {
            //Venue venue = new Venue("1", 2, 2, 2, "", "", "", "", "", "", "", "", (VenueStatus)99, "", DateTime.Now, DateTime.Now);
            //Assert.NotNull(venue);
        }


        [Fact]
        public void Create_And_Assign_Blank_Id()
        {
            //Exception ex = Assert.Throws<ArgumentException>(() => new Venue("", 2, 2, 2, "", "", "", "", "", "", "", "", (VenueStatus)99, "", DateTime.Now, DateTime.Now));

        }
    }
}
