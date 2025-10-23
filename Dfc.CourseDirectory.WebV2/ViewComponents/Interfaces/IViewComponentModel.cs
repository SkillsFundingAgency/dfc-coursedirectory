using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Interfaces
{
    public interface IViewComponentModel
    {
        bool HasErrors { get; }
        IEnumerable<string> Errors { get; }
    }
}
