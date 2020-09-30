using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ProviderSearchRequestModel
    {
        [Required(ErrorMessage = "Enter Search Term")]
        public string SearchTerm { get; set; }
    }
}