
using System;
using System.Collections.Generic;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface IFACSearchCriteria
    {
        string scoringProfile { get; }
        string search { get; }
        string searchMode { get; }
        int? top { get; }
        int? skip { get; }
        string filter { get; }
        IEnumerable<string> facets { get; }
        bool count { get; }
    }
}
