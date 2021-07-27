using System;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewComponents.Extensions
{
    public static class ViewComponentExtensions
    {
        public static string ThenCheck(this bool extendee)
        {
            return extendee ? "checked=\"checked\"" : string.Empty;
        }

        public static string ThenNullableCheck(this bool? extendee)
        {
            if (extendee.HasValue && extendee.Value)
                return "checked=\"checked\"";
            return string.Empty;
        }

        public static string ThenAriaCurrent(this bool extendee)
        {
            return extendee ? "aria-current=\"true\"" : string.Empty;
        }

        public static string ThenCssClasses(this bool extendee, string cssClasses)
        {
            return extendee ? cssClasses : string.Empty;
        }

        public static string IfNotWorkBasedDisplayNone(this CourseDeliveryMode deliveryMode)
        {
            var noDisplay = "display: none;";
            var disp = string.Empty;

            switch (deliveryMode)
            {
                case CourseDeliveryMode.ClassroomBased:
                case CourseDeliveryMode.Online:
                    return noDisplay;
                case CourseDeliveryMode.WorkBased:
                    return disp;
                default:
                    return noDisplay;
            }
        }

        public static string IfNotClassroomBasedDisplayNone(this CourseDeliveryMode deliveryMode)
        {
            var noDisplay = "display: none;";
            var disp = string.Empty;

            switch (deliveryMode)
            {
                case CourseDeliveryMode.WorkBased:
                case CourseDeliveryMode.Online:
                    return noDisplay;
                case CourseDeliveryMode.ClassroomBased:
                    return disp;
                default:
                    return noDisplay;
            }
        }
    }
}
