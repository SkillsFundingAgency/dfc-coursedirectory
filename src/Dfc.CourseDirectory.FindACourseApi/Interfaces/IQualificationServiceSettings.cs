
using System;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IQualificationServiceSettings
    {
        string SearchService { get; }
        string QueryKey { get; }
        string AdminKey { get; }
        string Index { get; }
    }
}
