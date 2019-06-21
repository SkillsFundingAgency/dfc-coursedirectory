using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class UnRegulatedSearchViewModel
    {
       
        public string Search { get; set; }

        public string NotificationTitle { get; set; }

        public string NotificationMessage { get; set; }
    }
}
