using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Core.Validation;

namespace Dfc.CourseDirectory.Core.Helpers
{
    public static class ASCIICodeHelper
    {
        public static string ReplaceHexCodes(string src)
        {
            string returnstring = string.Empty;
            if (src != null)
            {
                var hexCodeRegex = new Regex(@"&#x([0-9A-Fa-f]+);");

                returnstring = hexCodeRegex.Replace(src, match =>
                { 
                    string hexCode = match.Groups[1].Value;

                    int charCode = Convert.ToInt32(hexCode, 16);

                    return ((char)charCode).ToString();
                });
            }

            return returnstring;
        }
    }
}
