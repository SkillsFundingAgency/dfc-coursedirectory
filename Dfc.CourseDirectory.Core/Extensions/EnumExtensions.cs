using System;
using System.ComponentModel;
using System.Reflection;

namespace Dfc.CourseDirectory.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            FieldInfo field = value.GetType().GetField(value.ToString());

            if (field == null) return value.ToString();

            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute?.Description ?? value.ToString();
        }
    }
}
