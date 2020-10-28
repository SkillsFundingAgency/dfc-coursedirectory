
using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Interfaces;


namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class FACSearchFacet : IFACSearchFacet
    {
        public int? count { get; }
        public dynamic value { get; }
    }
}
