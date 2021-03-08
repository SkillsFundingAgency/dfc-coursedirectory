using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Providers;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Helper
{
    [Obsolete("Please try not to use this any more, and instead create Mapper classes using Automapper or similar", false)]
    public interface IDASHelper
    {
        DasProvider CreateDasProviderFromProvider(int exportKey, Provider provider, Core.DataStore.CosmosDb.Models.FeChoice feChoice);
        List<DasLocation> ApprenticeshipLocationsToLocations(int exportKey, Dictionary<string, ApprenticeshipLocation> locations);
        List<DasStandard> ApprenticeshipsToStandards(int exportKey, IEnumerable<Apprenticeship> apprenticeships, Dictionary<string, ApprenticeshipLocation> validLocations);
        List<DasFramework> ApprenticeshipsToFrameworks(int exportKey, IEnumerable<Apprenticeship> apprenticeships, Dictionary<string, ApprenticeshipLocation> validLocations);
    }
}
