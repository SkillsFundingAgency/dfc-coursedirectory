using Dfc.CourseDirectory.Services.Interfaces;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchFilterBuilder : ILarsSearchFilterBuilder
    {
        private static StringBuilder _sb;

        public LarsSearchFilterBuilder() : this(new StringBuilder())
        {
        }

        public LarsSearchFilterBuilder(StringBuilder stringBuilder)
        {
            _sb = stringBuilder;
        }

        public ILarsSearchFilterBuilder And()
        {
            _sb.Append(" and");
            return this;
        }

        public string Build()
        {
            return _sb.ToString().Trim();
        }

        public ILarsSearchFilterBuilder EqualTo(string value)
        {
            if (value != null)
            {
                _sb.Append($" eq '{value}'");
            }

            return this;
        }

        public ILarsSearchFilterBuilder Field(string value)
        {
            if (value != null)
            {
                _sb.Append($" {value}");
            }

            return this;
        }

        public ILarsSearchFilterBuilder GreaterThan(string value)
        {
            if (value != null)
            {
                _sb.Append($" gt '{value}'");
            }

            return this;
        }

        public ILarsSearchFilterBuilder GreaterThanOrEqualTo(string value)
        {
            if (value != null)
            {
                _sb.Append($" ge {value}");
            }

            return this;
        }

        public ILarsSearchFilterBuilder LessThan(string value)
        {
            if (value != null)
            {
                _sb.Append($" lt '{value}'");
            }

            return this;
        }

        public ILarsSearchFilterBuilder LessThanOrEqualTo(string value)
        {
            if (value != null)
            {
                _sb.Append($" le '{value}'");
            }

            return this;
        }

        public ILarsSearchFilterBuilder Not()
        {
            _sb.Append($" not");
            return this;
        }

        public ILarsSearchFilterBuilder NotEqualTo(string value)
        {
            if (value != null)
            {
                _sb.Append($" ne '{value}'");
            }

            return this;
        }

        public ILarsSearchFilterBuilder Or()
        {
            _sb.Append($" or");
            return this;
        }

        public ILarsSearchFilterBuilder StartsWith(string value)
        {
            if (value != null)
            {
                _sb.Append($" {value}*");
            }

            return this;
        }

        public ILarsSearchFilterBuilder PrependOpeningBracket()
        {
            _sb.Insert(0, "(");

            return this;
        }

        public ILarsSearchFilterBuilder AppendClosingBracket()
        {
            _sb.Append(")");

            return this;
        }

        public ILarsSearchFilterBuilder AppendOpeningBracket()
        {
            _sb.Append("(");

            return this;
        }
    }
}