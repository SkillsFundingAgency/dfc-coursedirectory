using Dfc.CourseDirectory.Models.Models.Courses;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.StudyMode
{
    public class StudyModeModel
    {
        public Models.Models.Courses.StudyMode StudyMode { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
    }
}
