using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public static class EnumExtension
    {
        public static string ToDescription(this System.Enum value)
        {
            var attributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static IEnumerable<SelectListItem> ToSelectList<T>(this System.Enum enumValue)
        {
            return
                System.Enum.GetValues(enumValue.GetType()).Cast<T>()
                    .Select(
                        x =>
                            new SelectListItem
                            {
                                Text = ((System.Enum)(object)x).ToDescription(),
                                Value = x.ToString(),
                                Selected = (enumValue.Equals(x))
                            });
        }
    }
}
