﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IUserHelper
    {
        bool CheckUserLoggedIn();
        Task<bool> IsUserAuthorised(string policy);
    }
}
