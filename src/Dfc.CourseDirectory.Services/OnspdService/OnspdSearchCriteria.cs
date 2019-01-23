using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.OnspdService;

namespace Dfc.CourseDirectory.Services.OnspdService
{
    public class OnspdSearchCriteria : ValueObject<OnspdSearchCriteria>, IOnspdSearchCriteria
    {
        public string Postcode { get; }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Postcode;
        }

        public OnspdSearchCriteria(string postcode)
        {
            Throw.IfNullOrWhiteSpace(postcode, nameof(postcode));

            Postcode = postcode;
        }
    }
}
