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
        //Please add all newly discovered unprocessable hex codes as a 'key' in this dictionary and their replacement char as the 'value'
        private static Dictionary<string, string> _unprocessableHexCodesMapping = new Dictionary<string, string>()
        {
            { "&#x202F;", " " },
            { "&#xF020;", "*" },
            { "&#xF06E;", "*" },
            { "&#xF0A7;", "*" },
            { "&#xF0B7;", "*" },
        };

        public static string ReplaceHexCodes(string src)
        {
            string returnstring = string.Empty;
            if (src != null)
            {
                //Search for hex codes known to not process correctly in the reports and replace them with the key specific in _uprocessableHexCodesMapping
                foreach (var key in _unprocessableHexCodesMapping.Keys)
                {
                    src = src.Replace(key, _unprocessableHexCodesMapping.GetValueOrDefault(key));
                }

                //Now check for any other hex codes and replace them with their corresponding value
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
