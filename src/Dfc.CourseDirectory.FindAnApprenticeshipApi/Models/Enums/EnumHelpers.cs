using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Dfc.Providerportal.FindAnApprenticeship.Models.Enums
{
    public static class EnumHelpers
    {
        public static string ToDescription(this System.Enum value)
        {
            var attributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}
