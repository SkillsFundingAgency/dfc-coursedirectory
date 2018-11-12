using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class PaginationItemModel
    {
        public string Url { get; set; }
        public string Text { get; set; }
        public string CssClass { get; set; }
        public bool IsEnabled { get; set; }
    }
}
