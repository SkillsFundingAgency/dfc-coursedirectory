using Dfc.CourseDirectory.Common;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.Components.Interfaces
{
    public interface IViewComponentModel
    {
        bool HasErrors { get; }
        IEnumerable<string> Errors { get; }
    }
}