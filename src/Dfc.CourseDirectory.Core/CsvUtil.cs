using System.IO;

namespace Dfc.CourseDirectory.Core
{
    public static class CsvUtil
    {
        public static int CountLines(Stream stream)
        {
            var reader = new StreamReader(stream, leaveOpen: true);

            int count = 0;
            while (reader.ReadLine() != null)
            {
                count++;
            }

            return count;
        }
    }
}
