using Dfc.CourseDirectory.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.EditVenueName
{
    public class VenueNameModel : ValueObject<VenueNameModel>
    {
        public string VenueName { get; }

        public VenueNameModel(
            string venueName)
        {
            Throw.IfNullOrWhiteSpace(venueName, nameof(venueName));
            Throw.IfGreaterThan(255, venueName.Length, nameof(venueName));

            VenueName = venueName;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return VenueName;
        }
    }
}
