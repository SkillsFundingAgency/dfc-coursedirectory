
using System;
using System.Collections.Generic;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface ISearchCriteria // IProviderSearchCriteria
    {
        string search { get; }
        string searchMode { get; }
        int? top { get; }
        string filter { get; }
        IEnumerable<string> facets { get; }
        bool count { get; }
    }
}
