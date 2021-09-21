using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Models
{
    public class ApprenticeshipLocationSameAddress : EqualityComparer<ApprenticeshipLocation>
    {
        public override bool Equals(ApprenticeshipLocation alpha, ApprenticeshipLocation beta)
        {
            if (alpha == null && beta == null)
                return true;
            else if (alpha == null || beta == null)
                return false;

            return alpha.ToAddressHash() == beta.ToAddressHash();
        }

        public override int GetHashCode(ApprenticeshipLocation app)
        {
            var stringToHash = app.ToAddressHash();
            return stringToHash;
        }
    }
}
