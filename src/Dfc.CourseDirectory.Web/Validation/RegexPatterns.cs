namespace Dfc.CourseDirectory.Web.Validation
{
    public static class RegexPattern
    {
        public const string AllowEverything = @".*";

        public const string AllowValidCharactersRelaxed =
            @"^[a-zA-Z0-9/\n/\r/\\u/\¬\!\£\$\%\^\&\*\\é\\è\\ﬁ\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\•\·\●\\’\‘\“\”\—\-\–\‐\‐\…\:/\°\®\\â\\ç\\ñ\\ü\\ø\♦\™\\t/\s\¼\¾\½\" +"\"" + "\\\\]+$";
            
        public const string DisallowHTMLTags = @"^(\<.*?\>).*?(\<\/.*?\>)"; //Not tested.

        public const string Base64Encoded = @"^(?:[A-Za-z0-9+\/]{4})*(?:[A-Za-z0-9+\/]{2}==|[A-Za-z0-9+\/]{3}=)?$";
    }
}
