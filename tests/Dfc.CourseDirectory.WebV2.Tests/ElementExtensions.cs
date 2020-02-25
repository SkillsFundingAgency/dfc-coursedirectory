using AngleSharp.Dom;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public static class ElementExtensions
    {
        public static T As<T>(this IElement element)
            where T : class, IElement
        {
            return element as T;
        }
    }
}
