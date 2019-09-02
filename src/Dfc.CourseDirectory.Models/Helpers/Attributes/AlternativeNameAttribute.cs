using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Helpers.Attributes
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
