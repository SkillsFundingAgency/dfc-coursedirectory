using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CsvProvider
    {
        [Display(Name = "Ukprn")]
        public string ProviderUKPRN { get; set; }
        [Display(Name = "ProviderName")]
        public string ProviderName { get; set; }
    }
}
