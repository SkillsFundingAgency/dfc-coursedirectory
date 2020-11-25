using System;
using System.Collections.Generic;
using Dfc.Providerportal.FindAnApprenticeship.Models;
using Dfc.Providerportal.FindAnApprenticeship.Models.DAS;
using Dfc.Providerportal.FindAnApprenticeship.Models.Providers;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Helper
{
    [Obsolete("Please try not to use this any more, and instead create Mapper classes using Automapper or similar", false)]
    public interface IDASHelper
    {
        DasProvider CreateDasProviderFromProvider(int exportKey, Provider provider, FeChoice feChoice);
        List<DasLocation> ApprenticeshipLocationsToLocations(int exportKey, Dictionary<string, ApprenticeshipLocation> locations);
        List<DasStandard> ApprenticeshipsToStandards(int exportKey, IEnumerable<Apprenticeship> apprenticeships,
            Dictionary<string, ApprenticeshipLocation> validLocations);
        List<DasFramework> ApprenticeshipsToFrameworks(int exportKey, IEnumerable<Apprenticeship> apprenticeships,
            Dictionary<string, ApprenticeshipLocation> validLocations);
    }
}