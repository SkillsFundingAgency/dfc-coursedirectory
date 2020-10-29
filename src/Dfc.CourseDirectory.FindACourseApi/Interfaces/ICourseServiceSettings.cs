
using System;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICourseServiceSettings
    {
        string ApiUrl { get; }
        string ApiKey { get; }
    }
}
