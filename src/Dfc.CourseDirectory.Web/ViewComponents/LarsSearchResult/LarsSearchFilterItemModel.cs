namespace Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult
{
    public class LarsSearchFilterItemModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        public int Count { get; set; }

        public bool IsSelected { get; set; }

        public static string FormatAwardOrgCodeSearchFilterItemText(string value) => value.ToUpper() switch
        {
            "E" => "Entry level",
            "X" => "Unknown or not applicable",
            "H" => "Higher",
            "M" => "Mixed",
            _ => $"Level {value}"
        };
    }
}