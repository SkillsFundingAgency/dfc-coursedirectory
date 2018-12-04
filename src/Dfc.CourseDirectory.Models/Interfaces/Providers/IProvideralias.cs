using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Providers
{ 
    public interface IProvideralias
    {
        object ProviderAlias { get; set; }
        object LastUpdated { get; set; }
    }
}
