using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using Dfc.CourseDirectory.Models.Models.Venues;
using System;
using Xunit;

namespace Dfc.CourseDirectory.Models.Test
{
    public class CourseTests
    {
        [Fact]
        public void Create_Course()
        {
            Contactaddress address = new Contactaddress();
            Contactpersonaldetails details = new Contactpersonaldetails();
            Provideralias[] alias = new Provideralias[1];
            Verificationdetail[] verDetail = new Verificationdetail[1];
            Providercontact[] contact = new Providercontact[1];
            Provider prov = new Provider(contact, alias, verDetail);

            Venue venue = new Venue("s", 2, 2, 2, "ss", "ss", "ss", "ss", "ss", "ss", "ss", "ss");
            DateTime[] times = new DateTime[20180411];
            CourseInformation info = new CourseInformation(
                times,
                "StudyMode",
                "Attendance",
                "CourseID",
                "CourseURL",
                "Pattern",
                "Requirements");
            CourseRun data = new CourseRun(venue, info);

            Qualification qual = new Qualification("ss", "ss", "ss", "ss", "ss");

            CourseText text = new CourseText(
                "CourseTitle",
                "Learn",
                "How",
                "Why"
                );
            Course course = new Course(
                provider: prov,
                qualification: qual,
                data: data,
                text: text
                );
        }

        [Fact]
        public void Create_Course_Text()
        {
            CourseText text = new CourseText(
                "CourseTitle",
                "Learn",
                "How",
                "Why"
                );
        }

        [Fact]
        public void Create_Course_Information()
        {
            DateTime[] times = new DateTime[20180411];
            CourseInformation info = new CourseInformation(
                times,
                "StudyMode",
                "Attendance",
                "CourseID",
                "CourseURL",
                "Pattern",
                "Requirements");
        }

        [Fact]
        public void Create_Course_Data()
        {
            Venue venue = new Venue("s", 2, 2, 2, "ss", "ss", "ss", "ss", "ss", "ss", "ss", "ss");
            DateTime[] times = new DateTime[20180411];
            CourseInformation info = new CourseInformation(
                times,
                "StudyMode",
                "Attendance",
                "CourseID",
                "CourseURL",
                "Pattern",
                "Requirements");
            CourseRun data = new CourseRun(venue, info);
        }
    }
}