
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using System.Collections.Generic;


namespace Dfc.CourseDirectory.Services.VenueService
{
    public class GetVenuesByPRNAndNameCriteria : ValueObject<GetVenuesByPRNAndNameCriteria>, IGetVenuesByPRNAndNameCriteria
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return PRN;
            yield return Name;
        }
    }
}
