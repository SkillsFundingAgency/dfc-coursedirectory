using System;
using System.IO;

namespace Dfc.CourseDirectory.Core
{
    public static class FileNameHelper
    {
        public static string SanitizeFileName(string fileName, string replacement = "")
        {
            if (fileName is null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var invalids = Path.GetInvalidFileNameChars();
            return string.Join(replacement, fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
    }
}
