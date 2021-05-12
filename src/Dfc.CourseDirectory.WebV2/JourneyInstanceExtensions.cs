using System;
using Dfc.CourseDirectory.Core;
using FormFlow;

namespace Dfc.CourseDirectory.WebV2
{
    public static class JourneyInstanceExtensions
    {
        public static void ThrowIfCompleted(this JourneyInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (instance.Completed)
            {
                throw new StateExpiredException();
            }
        }

        public static void ThrowIfNotCompleted(this JourneyInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!instance.Completed)
            {
                throw new InvalidStateException();
            }
        }
    }
}
