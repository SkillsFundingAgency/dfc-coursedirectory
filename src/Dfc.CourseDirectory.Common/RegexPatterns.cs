using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Dfc.CourseDirectory.Common
{
    public static class RegexPattern
    {
        public const string AllowEverything = @".*";

        public const string AllowValidCharactersRelaxed =
            @"^[a-zA-Z0-9/\n/\r/\\u/\¬\!\£\$\%\^\&\*\\é\\è\\ﬁ\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\•\·\●\\’\‘\“\”\—\-\–\‐\‐\…\:/\°\®\\â\\ç\\ñ\\ü\\ø\♦\™\\t/\s\¼\¾\½\" +"\"" + "\\\\]+$";
            
        public const string DisallowHTMLTags = @"^(\<.*?\>).*?(\<\/.*?\>)"; //Not tested.
    }
}
