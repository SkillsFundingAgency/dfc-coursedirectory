using System;

namespace Dfc.CourseDirectory.WebV2
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class LocalUrlAttribute : Attribute
    {
        public LocalUrlAttribute()
        {
            AddToViewData = false;
        }

        public LocalUrlAttribute(string viewDataKey)
        {
            if (viewDataKey == null)
            {
                throw new ArgumentNullException(nameof(viewDataKey));
            }

            AddToViewData = true;
            ViewDataKey = viewDataKey;
        }

        public bool AddToViewData { get; }

        public string ViewDataKey { get; }
    }
}
