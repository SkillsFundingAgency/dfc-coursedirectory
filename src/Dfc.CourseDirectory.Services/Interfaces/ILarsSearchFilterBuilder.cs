namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchFilterBuilder
    {
        ILarsSearchFilterBuilder Field(string value);

        ILarsSearchFilterBuilder And();

        ILarsSearchFilterBuilder Or();

        ILarsSearchFilterBuilder Not();

        ILarsSearchFilterBuilder StartsWith(string value);

        ILarsSearchFilterBuilder EqualTo(string value);

        ILarsSearchFilterBuilder NotEqualTo(string value);

        ILarsSearchFilterBuilder GreaterThan(string value);

        ILarsSearchFilterBuilder LessThan(string value);

        ILarsSearchFilterBuilder GreaterThanOrEqualTo(string value);

        ILarsSearchFilterBuilder LessThanOrEqualTo(string value);

        ILarsSearchFilterBuilder PrependOpeningBracket();

        ILarsSearchFilterBuilder AppendClosingBracket();

        ILarsSearchFilterBuilder AppendOpeningBracket();

        string Build();
    }
}