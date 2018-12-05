using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearch
{
    public class PostCodeSearchModel
    {
        [Required]
        public string PostCode { get; set; }
    }
}