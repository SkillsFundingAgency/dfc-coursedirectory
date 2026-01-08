namespace Dfc.CourseDirectory.WebV2.Validation
{
    public static class RegexPattern
    {
        public const string AllowEverything = @".*";

        public const string AllowValidCharactersRelaxed =
            @"^[a-zA-Z0-9/\n/\r/\\u/\¬\!\£\$\%\^\&\*\\é\\è\\ﬁ\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\•\·\●\\’\‘\“\”\—\-\–\‐\‐\…\:/\°\®\\â\\ç\\ñ\\ü\\ø\♦\™\\t/\s\¼\¾\½\" +"\"" + "\\\\]+$";
            
        public const string DisallowHTMLTags = @"^(?!.*(?:</?\s*[A-Za-z][A-Za-z0-9-]*(?:\s+[^<>]*?)?\s*>|&lt;/?\s*[A-Za-z][A-Za-z0-9-]*(?:\s+[^&]*?)?\s*&gt;)).+$";

        public const string Base64Encoded = @"^(?:[A-Za-z0-9+\/]{4})*(?:[A-Za-z0-9+\/]{2}==|[A-Za-z0-9+\/]{3}=)?$";

        //public const string BlackListHTMLTags = @"^(?!.*<(\/?\s*(script|style|iframe|embed|object|applet|link|meta|base|form|input|label|select|option|textarea|img|a)\b[^>]*>)).*$";
    }
}
