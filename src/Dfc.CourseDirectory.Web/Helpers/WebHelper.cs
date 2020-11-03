using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using Dfc.CourseDirectory.Services.Enums;
using Microsoft.Extensions.WebEncoders.Testing;


namespace Dfc.CourseDirectory.Web.Helpers
{



    public static class WebHelper
    {
        public static string GetErrorTextValueToUse(int val)
        {
            if (val.Equals(1))
            {
                return " error";
            }
            return " errors";

        }
        public static string GetCourseTextToUse(int val)
        {
            if (val.Equals(1))
            {
                return " course";
            }
            return " courses";
            
        }

        public static string GetLocationsTextToUse(int val)
        {
            if (val.Equals(1))
            {
                return " location";
            }
            return " locations";

        }

        public static string GetApprenticeshipsTextToUse(int val)
        {
            if (val.Equals(1))
            {
                return " apprenticeship";
            }
            return " apprenticeships";

        }

        public static string GetEnumDescription(Enum value)
        {
            return
                value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description;
        }

        public static string GetEnumSubtext(Enum value)
        {
            return
                value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<HintAttribute>()
                    ?.Hint;
        }

        public static string Encode(string value)
        {
            HtmlEncoder htmlEncoder = new HtmlTestEncoder();

            if (!string.IsNullOrEmpty(value))
            {
                return htmlEncoder.Encode(value);
            }

            return null;
        }
    }
}
