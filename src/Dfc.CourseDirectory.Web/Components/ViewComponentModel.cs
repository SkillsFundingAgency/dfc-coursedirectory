using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Errors = errors;
        }
    }
}
