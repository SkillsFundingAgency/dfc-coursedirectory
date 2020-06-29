using Microsoft.AspNetCore.Html;

namespace Dfc.CourseDirectory.Web.ViewComponents.CourseName
{
    public class CourseNameModel
    {
        public string CourseName { get; set; }
        public string LabelText { get; set; }
        public HtmlString HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
