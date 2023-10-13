﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Core.Validation;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ASCIICodeHelper
    {
        public static string RemoveASCII(string src)
        {
            string returnstring = string.Empty;
            if (src != null)
            {
                returnstring = Regex.Replace(src, @"[^\u0000-\u007F]", "");
                //returnstring = HttpUtility.HtmlDecode(returnstring);
                returnstring= Regex.Replace(returnstring, "<.*?>", string.Empty);
            }
            return returnstring;
        }
    }
}
