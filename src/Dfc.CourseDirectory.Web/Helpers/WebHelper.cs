using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dfc.CourseDirectory.Models.Enums;


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
                return " Course";
            }
            return " Courses";
            
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
    }
}
