using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IProviderAdd
    {
        Guid id { get; set; }
        int Status { get; set; }
        string UpdatedBy { get; set; }
    }
}
