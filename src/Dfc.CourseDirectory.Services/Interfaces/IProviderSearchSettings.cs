using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IProviderSearchSettings
    {
        string ApiUrl { get; }
        //string ApiVersion { get; }
        string ApiKey { get; }
    }
}
