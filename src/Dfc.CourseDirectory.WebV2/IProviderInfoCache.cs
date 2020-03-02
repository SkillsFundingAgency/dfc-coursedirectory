﻿using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderInfoCache
    {
        Task<ProviderInfo> GetProviderInfo(Guid providerId);
    }
}
