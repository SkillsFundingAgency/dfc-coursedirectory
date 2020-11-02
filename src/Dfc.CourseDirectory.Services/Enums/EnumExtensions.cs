using System;

namespace Dfc.CourseDirectory.Services.Enums
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            return Enum.TryParse<T>(value, true, out T result) ? result : defaultValue;
        }
    }
}
