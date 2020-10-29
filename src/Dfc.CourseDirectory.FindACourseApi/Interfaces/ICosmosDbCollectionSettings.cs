using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICosmosDbCollectionSettings
    {
        string CoursesCollectionId { get; }
    }
}
