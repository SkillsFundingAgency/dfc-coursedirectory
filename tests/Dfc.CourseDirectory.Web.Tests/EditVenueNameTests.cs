using Dfc.CourseDirectory.Web.ViewComponents.EditVenueName;
using Dfc.CourseDirectory.Web.ViewModels;
using System;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class EditVenueNameTests
    {
        [Fact]
        public void Model_Venue_Name_Cannot_Be_Blank()
        {
            string emptyString = "";
            Assert.Throws<ArgumentException>(() => new VenueNameModel(
                venueName: emptyString));
        }

        [Fact]
        public void Model_Max_Character_Length_Of_255()
        {
            string longString = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghi" +
                "jklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefgh" +
                "ijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefg" +
                "hijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz";
            Assert.Throws<ArgumentOutOfRangeException>(() => new VenueNameModel(
                venueName: longString));
        }
        //[Fact]
        //public void View_Venue_Name_Cannot_Be_Blank()
        //{
        //    string error = "Venue Name Cannot Be Blank";
        //    EditAddressViewModel vm = new EditAddressViewModel();
        //    vm.VenueName = "";
        //}

        //[Fact]
        //public void View_Max_Character_Length_Of_255()
        //{

        //}
        //[Fact]
        //public void Display_Error_On_Max_Character_Length_Of_255()
        //{
        //}

        [Fact]
        public void First_Character_Cannot_Be_Whitespace()
        {
            string emptyString = "";
            Assert.Throws<ArgumentException>(() => new VenueNameModel(
                venueName: emptyString));
        }

        [Fact]
        public void No_NonKeyboard_Characters_Allowed()
        {
        }
    }
}