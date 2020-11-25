using Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace Dfc.Providerportal.FindAnApprenticeship.Models.DAS
{
    public class DasLocation : IDasLocation 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IDasAddress Address { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Phone { get; set; }
    }

    public class DasLocationComparer : EqualityComparer<DasLocation>
    {
        public override bool Equals(DasLocation alpha, DasLocation beta)
        {
            if (alpha == null && beta == null)
                return true;
            else if (alpha == null || beta == null)
                return false;

            return alpha.Id == beta.Id;
        }

        public override int GetHashCode(DasLocation app)
        {
            return app.Id.GetHashCode();
        }
    }
}
