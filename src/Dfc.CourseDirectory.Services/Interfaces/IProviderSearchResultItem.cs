using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public interface IProviderSearchResultItem
    {
        string UnitedKingdomProviderReferenceNumber { get; }
        string ProviderName { get; }
        string ProviderStatus { get; }
    }
}
