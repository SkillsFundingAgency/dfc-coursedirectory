using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS
{
    public interface IDasLocation
    {
        int Id { get; set; }
        string Name { get; set; }
        IDasAddress Address { get; set; }
        string Email { get; set; }
        string Website { get; set; }
        string Phone { get; set; }
    }
}
