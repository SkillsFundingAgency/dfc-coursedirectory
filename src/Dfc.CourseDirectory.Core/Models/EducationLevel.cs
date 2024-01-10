using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum EducationLevel
    {
        EntryLevel = 0,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven
    }

    public static class EducationLevelExtensions
    {
        public static string ToDescription(this EducationLevel? educationLevel) => GetEducationLevelDescription(educationLevel);

        public static string ToDescription(this EducationLevel educationLevel) => GetEducationLevelDescription(educationLevel);

        private static string GetEducationLevelDescription(EducationLevel? educationLevel)
        {
            return educationLevel switch
            {
                null => null,
                EducationLevel.EntryLevel => "Entry Level",
                EducationLevel.One => "1",
                EducationLevel.Two => "2",
                EducationLevel.Three => "3",               
                EducationLevel.Four => "4",
                EducationLevel.Five => "5",
                EducationLevel.Six => "6",
                EducationLevel.Seven => "7",
                _ => throw new NotImplementedException($"Unknown value: '{educationLevel}'.")
            };
        }
    }
}
