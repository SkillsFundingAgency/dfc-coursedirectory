
using System;


namespace Dfc.CourseDirectory.Services.Interfaces.CourseService
{
    public interface IFindACourseServiceSettings
    {
        string ApiUrl { get; set; }
        string ApiKey { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
    }
}
