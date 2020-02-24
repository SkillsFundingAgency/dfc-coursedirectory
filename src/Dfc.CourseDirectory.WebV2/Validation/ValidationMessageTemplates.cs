namespace Dfc.CourseDirectory.WebV2.Validation
{
    public static class ValidationMessageTemplates
    {
        public static string MessageForMaxStringLength(string label, int maxLength) =>
            $"{label} must be {maxLength} characters or less";
    }
}
