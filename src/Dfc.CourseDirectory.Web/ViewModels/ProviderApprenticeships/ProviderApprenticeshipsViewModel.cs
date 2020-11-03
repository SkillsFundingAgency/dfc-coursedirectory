using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.ProviderApprenticeships
{
    public class ProviderApprenticeshipsViewModel
    {
        [RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "Search contains invalid characters")]
        public string Search { get; set; }
        public List<Apprenticeship> Apprenticeships { get; set; }
        public Guid? ApprenticeshipId { get; set; }
    }
}
