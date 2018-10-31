namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchFilterBuilder
    {
        ILarsSearchFilterBuilder And(string value);
        ILarsSearchFilterBuilder Or(string value);
        ILarsSearchFilterBuilder Not(string value);
        ILarsSearchFilterBuilder StartsWith(string value);
        ILarsSearchFilterBuilder EqualTo(string value);
        ILarsSearchFilterBuilder NotEqualTo(string value);
        ILarsSearchFilterBuilder GreaterThan(string value);
        ILarsSearchFilterBuilder LessThan(string value);
        ILarsSearchFilterBuilder GreatherThanOrEqualTo(string value);
        ILarsSearchFilterBuilder LessThanOrEqualTo(string value);
        string Build();
    }
}
