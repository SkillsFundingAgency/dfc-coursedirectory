using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CsvProvider : ICsvProvider
    {
        [Display(Name = "Ukprn")]
        public string ProviderUKPRN { get; set; }
        [Display(Name = "ProviderName")]
        public string ProviderName { get; set; }
    }
}
