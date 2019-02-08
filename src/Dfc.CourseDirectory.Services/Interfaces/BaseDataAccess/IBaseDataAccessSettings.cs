using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces.BaseDataAccess
{
    public interface IBaseDataAccessSettings
    {
        string ConnectionString { get; set; }
    }
}
