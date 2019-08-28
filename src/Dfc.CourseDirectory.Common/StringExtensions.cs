namespace Dfc.CourseDirectory.Common
{
    public static class StringExtensions
    {
        public static string FirstSentence(this string str)
        {
            string firstSentence = str;
            int pos = str.IndexOf(".") + 1;
            if(pos > 0 )
            {
                firstSentence = str.Substring(0, pos);
            }
            return firstSentence;
        }
    }
}
