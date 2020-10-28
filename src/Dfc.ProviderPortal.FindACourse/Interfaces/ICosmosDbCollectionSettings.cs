using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface ICosmosDbCollectionSettings
    {
        string CoursesCollectionId { get; }
    }
}
