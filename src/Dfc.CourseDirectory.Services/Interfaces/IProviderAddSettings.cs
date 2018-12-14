using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IProviderAddSettings
    {
        string ApiUrl { get; }
        string ApiKey { get; }
        string ApiVersion { get; set; }
    }
}
