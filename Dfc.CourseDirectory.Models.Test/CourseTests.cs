using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using Dfc.CourseDirectory.Models.Models.Venues;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dfc.CourseDirectory.Models.Test
{
    public class CourseTests
    {
        [Fact]
        public void Create_And_Assign_Values()
        {
            Contactaddress address = new Contactaddress();
            Contactpersonaldetails details = new Contactpersonaldetails();
            Provideralias[] alias = new Provideralias[1];
            Verificationdetail[] verDetail = new Verificationdetail[1];
            Providercontact[] contact = new Providercontact[1];
            Provider prov = new Provider(contact, alias, verDetail);

            Qualification qual = new Qualification("ss", "ss", "ss", "ss", "ss");
            Venue venue = new Venue("ss", 2, 2, 2, "xx", "xx", "xx", "xx", "aa", "\\", "ss", "ss");
            Course course = new Course(
                provider: prov,
                venue: venue,
                qualification: qual
                //courseData,
                //courseText,
                //courseInformation
                );
        }
    }
}
