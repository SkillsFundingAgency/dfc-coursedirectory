using Dfc.CourseDirectory.Services.Interfaces.BaseDataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.BaseDataAccess
{
    public class BaseDataAccessSettings : IBaseDataAccessSettings
    {
        public string ConnectionString { get; set; }
    }
}
