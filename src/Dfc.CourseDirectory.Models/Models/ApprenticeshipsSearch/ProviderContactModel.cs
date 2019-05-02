using Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch
{
    public class ProviderContactModel : IProviderContactModel
    {
        public string phone { get; set; }
        public string email { get; set; }
        public string contactUsUrl { get; set; }
    }
}
