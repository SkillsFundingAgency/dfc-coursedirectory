using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch
{
    public interface IProviderContactModel
    {
        string phone { get; set; }
        string email { get; set; }
        string contactUsUrl { get; set; }
    }
}
