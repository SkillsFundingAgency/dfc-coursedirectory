using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICosmosDbSettings
    {
        string EndpointUri { get; }
        string PrimaryKey { get; }
        string DatabaseId { get; }
    }
}
