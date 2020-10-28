
using System;
using System.Collections.Generic;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface IFACSearchFacet
    {
        int? count { get; }
        dynamic value { get; }
    }
}
