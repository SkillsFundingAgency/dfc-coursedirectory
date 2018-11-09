using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.Interfaces
{
    public interface IViewComponentModel
    {
        bool HasErrors { get; }
        IEnumerable<string> Errors { get; }
    }
}