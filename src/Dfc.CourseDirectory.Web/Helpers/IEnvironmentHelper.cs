﻿using Dfc.CourseDirectory.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IEnvironmentHelper
    {
        EnvironmentType GetEnvironmentType();
    }
}
