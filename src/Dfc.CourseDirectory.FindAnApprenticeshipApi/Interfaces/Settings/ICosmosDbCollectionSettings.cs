using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Settings
{
    public interface ICosmosDbCollectionSettings
    {
        string StandardsCollectionId { get; }
        string FrameworksCollectionId { get; }
        string ApprenticeshipCollectionId { get; }
        string ProgTypesCollectionId { get; }
    }
}
