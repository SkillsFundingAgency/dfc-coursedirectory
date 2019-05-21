using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService
{
    public interface IApprenticeshipServiceSettings
    {
        string ApiUrl { get; set; }
        string ApiKey { get; set; }
    }
}
