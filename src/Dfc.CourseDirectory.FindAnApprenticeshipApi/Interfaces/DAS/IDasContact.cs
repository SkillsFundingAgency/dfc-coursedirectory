using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS
{
    public interface IDasContact
    {
        string Phone { get; set; }
        string Email { get; set; }
        string ContactUsUrl { get; set; }
    }
}
