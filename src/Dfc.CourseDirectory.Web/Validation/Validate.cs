using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.Web.Validation
{
    public static class Validate
    {
        /// <summary>
        /// Server side validation to match and extend the client-side validation
        /// </summary>
        /// <param name="bulkUploadFile"></param>
        /// <returns></returns>
        public static bool ValidateFile(IFormFile bulkUploadFile, out string errorMessage)
        {
            if (bulkUploadFile.Length == 0)
            {
                errorMessage = "No file uploaded";
                return false;
            }

            if (!bulkUploadFile.FileName.EndsWith(".csv") || bulkUploadFile.FileName.Replace(".csv", string.Empty).Contains(".") || bulkUploadFile.Name != "bulkUploadFile")
            {
                errorMessage = "Invalid file name";
                return false;
            }

            if (!bulkUploadFile.ContentDisposition.Contains("filename"))
            {
                errorMessage = "Invalid upload";
                return false;
            }
            if (bulkUploadFile.Length > 209715200)
            {
                errorMessage = "File too large";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public static bool IsBase64EncodedStream(MemoryStream ms)
        {
            string msData = Encoding.UTF8.GetString((ms as MemoryStream).ToArray());

            msData = msData.Trim();
            return (msData.Length % 4 == 0) && Regex.IsMatch(msData, RegexPattern.Base64Encoded, RegexOptions.None);
        }

        public static bool IsBinaryStream(MemoryStream ms)
        {
            var msArray = ms.ToArray();
            return msArray.FirstOrDefault(x => IsControlChar(x)) != 0;
        }

        private static bool IsControlChar(int ch)
        {
            return (ch > Chars.NUL && ch < Chars.BS)
                   || (ch > Chars.CR && ch < Chars.SUB);
        }

        private static class Chars
        {
            public static char NUL = (char)0; // Null char
            public static char BS = (char)8; // Back Space
            public static char CR = (char)13; // Carriage Return
            public static char SUB = (char)26; // Substitute
        }
    }
}
