
using System;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IProviderServiceSettings
    {
        string ApiUrl { get; }
        string ApiKey { get; }
    }
}
