using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.RequestModels
{
    public class ProviderSearchRequestModel
    {
        [Required(ErrorMessage = "Enter Search Term")]
        public string SearchTerm { get; set; }
    }
}
