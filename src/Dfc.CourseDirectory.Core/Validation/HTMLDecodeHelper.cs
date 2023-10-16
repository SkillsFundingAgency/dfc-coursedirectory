using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class HTMLDecodeHelper
    {
        public static string RemoveHTML(string src)
        {
            string returnstring = string.Empty;
            if (src != null)
            {
                returnstring = Regex.Replace(returnstring, "<.*?>", string.Empty);
            }
            return returnstring;
        }
    }
}
