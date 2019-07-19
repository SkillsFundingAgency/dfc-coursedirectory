using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ProviderSearchRequestModel
    {
        [Required(ErrorMessage = "Enter Search Term")]
        public string SearchTerm { get; set; }

        public ProviderSearchRequestModel()
        {

        }
    }
}
