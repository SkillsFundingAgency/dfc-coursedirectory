using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Settings
{
    public interface ICosmosDbSettings
    {
        string EndpointUri { get; set; }
        string PrimaryKey { get; set; }
        string DatabaseId { get; set; }
    }
}
