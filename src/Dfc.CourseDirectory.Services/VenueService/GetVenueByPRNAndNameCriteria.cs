using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class GetVenuesByPRNAndNameCriteria : IGetVenuesByPRNAndNameCriteria
    {
        public string PRN { get; }
        public string Name { get; }

        public GetVenuesByPRNAndNameCriteria(
            string prn,
            string name)
        {
            Throw.IfNullOrWhiteSpace(prn, nameof(prn));
            Throw.IfNullOrWhiteSpace(name, nameof(name));

            PRN = prn;
            Name = name;
        }
    }
}
