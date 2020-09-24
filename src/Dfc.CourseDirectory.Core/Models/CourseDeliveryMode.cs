using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum CourseDeliveryMode
    {
        ClassroomBased = 1,
        Online = 2,
        WorkBased = 3
    }

    public static class CourseDeliveryModeExtensions
    {
        public static string ToDescription(this CourseDeliveryMode deliveryMode) => deliveryMode switch
        {
            CourseDeliveryMode.ClassroomBased => "Classroom based",
            CourseDeliveryMode.Online => "Online",
            CourseDeliveryMode.WorkBased => "Work based",
            _ => throw new NotSupportedException($"Unknown delivery mode: '{deliveryMode}'.")
        };
    }
}
