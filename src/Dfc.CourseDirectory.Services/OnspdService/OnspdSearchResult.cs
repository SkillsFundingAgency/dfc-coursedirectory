using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Onspd;
using Dfc.CourseDirectory.Services.Interfaces.OnspdService;

namespace Dfc.CourseDirectory.Services.OnspdService
{
    public class OnspdSearchResult : ValueObject<OnspdSearchResult>, IOnspdSearchResult
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public Onspd Value { get; set; }

        public OnspdSearchResult(Onspd value)
        {
            Throw.IfNull(value, nameof(value));
        }
    }
}
