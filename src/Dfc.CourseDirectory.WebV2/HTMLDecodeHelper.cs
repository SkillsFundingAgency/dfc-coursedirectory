using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public static class HTMLDecodeHelper
    {
        public static string RemoveHTML(string src)
        {
            string returnstring = src;
            if (src != null)
            {
                returnstring = Regex.Replace(returnstring, "<.*?>", string.Empty);
            }
            return returnstring;
        }
    }
}
