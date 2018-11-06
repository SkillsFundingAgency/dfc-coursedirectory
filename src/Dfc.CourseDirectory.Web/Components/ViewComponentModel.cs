using Dfc.CourseDirectory.Common;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.Components
{
    public abstract class ViewComponentModel
    {
        public bool HasErrors => Errors?.Count() > 0;
        public IEnumerable<string> Errors { get; }

        public ViewComponentModel()
            : this(new string[] { })
        {
        }

        public ViewComponentModel(IEnumerable<string> errors)
        {
            Throw.IfNull(errors, nameof(errors));

            Errors = errors;
        }
    }
}