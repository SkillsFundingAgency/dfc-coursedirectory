
using System;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Services.CourseService
{
    public class FindACourseServiceSettings : IFindACourseServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
