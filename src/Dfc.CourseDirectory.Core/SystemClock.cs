﻿using System;

namespace Dfc.CourseDirectory.Core
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
