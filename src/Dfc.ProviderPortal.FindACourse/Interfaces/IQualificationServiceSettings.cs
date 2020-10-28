
using System;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface IQualificationServiceSettings
    {
        string SearchService { get; }
        string QueryKey { get; }
        string AdminKey { get; }
        string Index { get; }
    }
}
