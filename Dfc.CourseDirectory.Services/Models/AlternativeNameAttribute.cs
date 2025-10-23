using System;

namespace Dfc.CourseDirectory.Services.Models
{
    public class AlternativeName
    {
        public interface IAttribute<T>
        {
            T Value { get; }
        }
        public sealed class AlternativeNameAttribute : Attribute, IAttribute<string>
        {
            private readonly string value;

            public AlternativeNameAttribute(string value)
            {
                this.value = value;
            }

            public string Value
            {
                get { return this.value; }
            }
        }
    }
}
