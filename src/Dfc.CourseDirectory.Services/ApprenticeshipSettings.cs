using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services
{
    public class ApprenticeshipSettings : IApprenticeshipSettings
    {
        public int NationalRadius { get; set; }
        public int SubRegionRadius { get; set; }

    }
}