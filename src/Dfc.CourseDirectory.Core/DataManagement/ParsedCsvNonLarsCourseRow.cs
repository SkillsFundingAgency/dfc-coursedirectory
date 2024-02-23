﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ParsedCsvNonLarsCourseRow : CsvNonLarsCourseRow
    {
        private const string DateFormat = "dd/MM/yyyy";

        private ParsedCsvNonLarsCourseRow()
        {
        }

        public CourseDeliveryMode? ResolvedDeliveryMode { get; private set; }
        public DateTime? ResolvedStartDate { get; private set; }
        public bool? ResolvedFlexibleStartDate { get; private set; }
        public bool? ResolvedNationalDelivery { get; private set; }
        public decimal? ResolvedCost { get; private set; }
        public int? ResolvedDuration { get; private set; }
        public CourseDurationUnit? ResolvedDurationUnit { get; private set; }
        public CourseStudyMode? ResolvedStudyMode { get; private set; }
        public CourseAttendancePattern? ResolvedAttendancePattern { get; private set; }
        public IReadOnlyCollection<Region> ResolvedSubRegions { get; private set; }
        public CourseType? ResolvedCourseType { get; set; }
        public EducationLevel? ResolvedEducationLevel { get; set; }
        public int? ResolvedSector { get; set; }

        public static ParsedCsvNonLarsCourseRow FromCsvCourseRow(CsvNonLarsCourseRow row, IEnumerable<Region> allRegions, List<Sector> sectors)
        {
            var parsedRow = row.Adapt(new ParsedCsvNonLarsCourseRow());
            parsedRow.ResolvedEducationLevel = ResolveEducationLevel(parsedRow.EducationLevel);
            parsedRow.ResolvedSector = ResolveSector(parsedRow.Sector, sectors);
            parsedRow.ResolvedCourseType = ResolveCourseType(parsedRow.CourseType);
            parsedRow.ResolvedDeliveryMode = ResolveDeliveryMode(parsedRow.DeliveryMode);
            parsedRow.ResolvedStartDate = ResolveStartDate(parsedRow.StartDate);
            parsedRow.ResolvedFlexibleStartDate = ResolveFlexibleStartDate(parsedRow.FlexibleStartDate);
            parsedRow.ResolvedNationalDelivery = ResolveNationalDelivery(parsedRow.NationalDelivery, parsedRow.SubRegions);
            parsedRow.ResolvedCost = ResolveCost(parsedRow.Cost);
            parsedRow.ResolvedDuration = ResolveDuration(parsedRow.Duration);
            parsedRow.ResolvedDurationUnit = ResolveDurationUnit(parsedRow.DurationUnit);
            parsedRow.ResolvedStudyMode = ResolveStudyMode(parsedRow.StudyMode);
            parsedRow.ResolvedAttendancePattern = ResolveAttendancePattern(parsedRow.AttendancePattern);
            parsedRow.ResolvedSubRegions = ResolveSubRegions(parsedRow.SubRegions, allRegions);
            return parsedRow;
        }

        public static string MapAttendancePattern(CourseAttendancePattern? value) => value switch
        {
            0 => null,  // TODO - remove when NormalizeClassroomBasedCourseRunFields function has been run
            null => null,
            CourseAttendancePattern.Daytime => "Daytime",
            CourseAttendancePattern.Evening => "Evening",
            CourseAttendancePattern.Weekend => "Weekend",
            CourseAttendancePattern.DayOrBlockRelease => "Day/Block release",
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static string MapCost(decimal? value) => value?.ToString("0.00");

        public static string MapDeliveryMode(CourseDeliveryMode? value) => value switch
        {
            CourseDeliveryMode.BlendedLearning => "Blended learning",
            CourseDeliveryMode.ClassroomBased => "Classroom",
            CourseDeliveryMode.Online => "Online",
            CourseDeliveryMode.WorkBased => "Work based",
            null => null,
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static string MapCourseType(CourseType? value) => value switch
        {
            Models.CourseType.SkillsBootcamp => "Skills Bootcamp",
            null => null,
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static string MapEducationLevel(EducationLevel? value) => value switch
        {
            Models.EducationLevel.EntryLevel => "Entry Level",
            Models.EducationLevel.One => "1",
            Models.EducationLevel.Two => "2",
            Models.EducationLevel.Three => "3",
            Models.EducationLevel.Four => "4",
            Models.EducationLevel.Five => "5",
            Models.EducationLevel.Six => "6",
            Models.EducationLevel.Seven => "7",
            null => null,
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static string MapDuration(int? value) => value?.ToString("0.##");

        public static string MapDurationUnit(CourseDurationUnit? value) => value switch
        {
            CourseDurationUnit.Minutes => "Minutes",
            CourseDurationUnit.Hours => "Hours",
            CourseDurationUnit.Days => "Days",
            CourseDurationUnit.Weeks => "Weeks",
            CourseDurationUnit.Months => "Months",
            CourseDurationUnit.Years => "Years",
            null => null,
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static string MapFlexibleStartDate(bool? value) => value switch
        {
            true => "Yes",
            false => "No",
            null => null
        };

        public static string MapNationalDelivery(bool? value) => value switch
        {
            true => "Yes",
            false => "No",
            null => null
        };

        public static string MapSubRegions(IReadOnlyCollection<string> value, IReadOnlyCollection<Region> allRegions) =>
            value == null ?
                string.Empty :
                string.Join(
                    SubRegionDelimiter + " ",
                    allRegions
                        .SelectMany(r => r.SubRegions)
                        .Where(r => value.Contains(r.Id))
                        .Select(r => r.Name));

        public static string MapStartDate(DateTime? value) => value?.ToString("dd/MM/yyyy");

        public static string MapStudyMode(CourseStudyMode? value) => value switch
        {
            0 => null,  // TODO - remove when NormalizeClassroomBasedCourseRunFields function has been run
            null => null,
            CourseStudyMode.FullTime => "Full time",
            CourseStudyMode.PartTime => "Part time",
            CourseStudyMode.Flexible => "Flexible",
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static CourseAttendancePattern? ResolveAttendancePattern(string value) => value?.ToLower()?.Trim() switch
        {
            "daytime" => CourseAttendancePattern.Daytime,
            "evening" => CourseAttendancePattern.Evening,
            "weekend" => CourseAttendancePattern.Weekend,
            "day/block release" => CourseAttendancePattern.DayOrBlockRelease,
            _ => (CourseAttendancePattern?)null
        };

        public static decimal? ResolveCost(string value) =>
            decimal.TryParse(value, out var result) && GetDecimalPlaces(result) <= 2 ? result : (decimal?)null;

        public static CourseDeliveryMode? ResolveDeliveryMode(string value) => value?.ToLower()?.Trim() switch
        {
            "blended" => CourseDeliveryMode.BlendedLearning,
            "blendedlearning" => CourseDeliveryMode.BlendedLearning,
            "blended learning" => CourseDeliveryMode.BlendedLearning,
            "classroom based" => CourseDeliveryMode.ClassroomBased,
            "classroombased" => CourseDeliveryMode.ClassroomBased,
            "classroom" => CourseDeliveryMode.ClassroomBased,
            "online" => CourseDeliveryMode.Online,
            "work based" => CourseDeliveryMode.WorkBased,
            "workbased" => CourseDeliveryMode.WorkBased,
            "work" => CourseDeliveryMode.WorkBased,
            _ => (CourseDeliveryMode?)null
        };

        public static CourseType? ResolveCourseType(string value) => value?.ToLower()?.Trim() switch
        {
            "skills bootcamp" => Models.CourseType.SkillsBootcamp,
            "Skills bootcamp" => Models.CourseType.SkillsBootcamp,
            "Skillsbootcamp" => Models.CourseType.SkillsBootcamp,
            _ => (CourseType?)null
        };
        public static EducationLevel? ResolveEducationLevel(string value) => value?.ToLower()?.Trim() switch
        {
            "entry level" => Models.EducationLevel.EntryLevel,            
            "1" => Models.EducationLevel.One,
            "one" => Models.EducationLevel.One,
            "2" => Models.EducationLevel.Two,
            "two" => Models.EducationLevel.Two,
            "3" => Models.EducationLevel.Three,
            "three" => Models.EducationLevel.Three,
            "4" => Models.EducationLevel.Four,
            "four" => Models.EducationLevel.Four,
            "5" => Models.EducationLevel.Five,
            "five" => Models.EducationLevel.Five,
            "6" => Models.EducationLevel.Six,
            "six" => Models.EducationLevel.Six,
            "7" => Models.EducationLevel.Seven,
            "seven" => Models.EducationLevel.Seven,
            _ => (EducationLevel?)null
        };        

        public static int? ResolveDuration(string value) =>
            int.TryParse(value, out var duration) ? duration : (int?)null;

        public static CourseDurationUnit? ResolveDurationUnit(string value) => value?.ToLower()?.Trim() switch
        {
            "minutes" => CourseDurationUnit.Minutes,
            "hours" => CourseDurationUnit.Hours,
            "days" => CourseDurationUnit.Days,
            "weeks" => CourseDurationUnit.Weeks,
            "months" => CourseDurationUnit.Months,
            "years" => CourseDurationUnit.Years,
            _ => null
        };

        public static bool? ResolveFlexibleStartDate(string value) => value?.ToLower()?.Trim() switch
        {
            "yes" => true,
            "no" => false,
            "" => false,
            _ => null
        };

        public static bool? ResolveNationalDelivery(string value, string subRegions)
        {
            var normalized = value?.ToLower();
            var subRegionsWereSpecified = !string.IsNullOrWhiteSpace(subRegions);

            if (normalized == "yes")
            {
                return true;
            }

            if (normalized == "no")
            {
                return false;
            }

            // Treat an empty value as 'no' if sub regions were specified
            if (string.IsNullOrWhiteSpace(value) && subRegionsWereSpecified)
            {
                return false;
            }

            return null;
        }

        public static DateTime? ResolveStartDate(string value) =>
            DateTime.TryParseExact(value, DateFormat, null, DateTimeStyles.None, out var dt) ? dt : (DateTime?)null;

        public static CourseStudyMode? ResolveStudyMode(string value) => value?.ToLower()?.Trim() switch
        {
            "full time" => CourseStudyMode.FullTime,
            "full-time" => CourseStudyMode.FullTime,
            "part time" => CourseStudyMode.PartTime,
            "part-time" => CourseStudyMode.PartTime,
            "flexible" => CourseStudyMode.Flexible,
            _ => (CourseStudyMode?)null
        };

        public static IReadOnlyCollection<Region> ResolveSubRegions(string value, IEnumerable<Region> allRegions)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var comparer = StringComparer.OrdinalIgnoreCase;

            var allSubRegions = allRegions
                .SelectMany(sr => sr.SubRegions)
                .ToDictionary(sr => sr.Name, sr => sr, comparer);

            var subRegionNames = value
                .Split(SubRegionDelimiter, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .ToArray();

            var matchedRegions = subRegionNames
                .SelectMany(v => allSubRegions.TryGetValue(v, out var sr) ? new[] { sr } : Array.Empty<Region>())
                .ToArray();

            if (subRegionNames.Length != matchedRegions.Length)
            {
                return null;
            }

            return matchedRegions;
        }

        private static int GetDecimalPlaces(decimal n)
        {
            n = Math.Abs(n);
            n -= (int)n;

            var decimalPlaces = 0;
            while (n > 0)
            {
                decimalPlaces++;
                n *= 10;
                n -= (int)n;
            }

            return decimalPlaces;
        }

        public static string MapSectorIdToCode(int? sectorId, List<Sector> sectors)
        {
            return sectors.FirstOrDefault(s => s.Id.Equals(sectorId))?.Code ?? null;
        }

        public static int? ResolveSector(string code, List<Sector> sectors)
        {
            return sectors.FirstOrDefault(s => s.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase))?.Id ?? null;
        }
    }
}
