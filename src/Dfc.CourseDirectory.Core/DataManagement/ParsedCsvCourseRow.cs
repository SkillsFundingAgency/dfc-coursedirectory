using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ParsedCsvCourseRow : CsvCourseRow
    {
        private const string DateFormat = "dd/MM/yyyy";

        private ParsedCsvCourseRow()
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

        public static ParsedCsvCourseRow FromCsvCourseRow(CsvCourseRow row, IEnumerable<Region> allRegions)
        {
            var parsedRow = row.Adapt(new ParsedCsvCourseRow());
            parsedRow.ResolvedDeliveryMode = ResolveDeliveryMode(parsedRow.DeliveryMode);
            parsedRow.ResolvedStartDate = ResolveStartDate(parsedRow.StartDate);
            parsedRow.ResolvedFlexibleStartDate = ResolveFlexibleStartDate(parsedRow.FlexibleStartDate);
            parsedRow.ResolvedNationalDelivery = ResolveNationalDelivery(parsedRow.NationalDelivery);
            parsedRow.ResolvedCost = ResolveCost(parsedRow.Cost);
            parsedRow.ResolvedDuration = ResolveDuration(parsedRow.Duration);
            parsedRow.ResolvedDurationUnit = ResolveDurationUnit(parsedRow.DurationUnit);
            parsedRow.ResolvedStudyMode = ResolveStudyMode(parsedRow.StudyMode);
            parsedRow.ResolvedAttendancePattern = ResolveAttendancePattern(parsedRow.AttendancePattern);
            parsedRow.ResolvedSubRegions = ResolveSubRegions(parsedRow.SubRegions, allRegions);
            return parsedRow;
        }

        public static CourseAttendancePattern? ResolveAttendancePattern(string value) => value?.ToLower() switch
        {
            "daytime" => CourseAttendancePattern.Daytime,
            "evening" => CourseAttendancePattern.Evening,
            "weekend" => CourseAttendancePattern.Weekend,
            "day/block release" => CourseAttendancePattern.DayOrBlockRelease,
            _ => (CourseAttendancePattern?)null
        };

        public static string MapAttendancePattern(CourseAttendancePattern? value) => value switch
        {
            0 => null,
            null => null,
            CourseAttendancePattern.Daytime => "daytime",
            CourseAttendancePattern.Evening => "evening",
            CourseAttendancePattern.Weekend => "weekend",
            CourseAttendancePattern.DayOrBlockRelease => "day/block release",
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static decimal? ResolveCost(string value) =>
            decimal.TryParse(value, out var result) && GetDecimalPlaces(result) <= 2 ? result : (decimal?)null;

        public static string MapCost(decimal? value) => value?.ToString("0.0000");

        public static CourseDeliveryMode? ResolveDeliveryMode(string value) => value?.ToLower() switch
        {
            "classroom based" => CourseDeliveryMode.ClassroomBased,
            "classroombased" => CourseDeliveryMode.ClassroomBased,
            "classroom" => CourseDeliveryMode.ClassroomBased,
            "online" => CourseDeliveryMode.Online,
            "work based" => CourseDeliveryMode.WorkBased,
            "workbased" => CourseDeliveryMode.WorkBased,
            "work" => CourseDeliveryMode.WorkBased,
            _ => (CourseDeliveryMode?)null
        };

        public static string MapDeliveryMode(CourseDeliveryMode? value) => value switch
        {
            CourseDeliveryMode.ClassroomBased => "classroom based",
            CourseDeliveryMode.Online => "online",
            CourseDeliveryMode.WorkBased => "work based",
            null => null,
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };


        public static int? ResolveDuration(string value) =>
            int.TryParse(value, out var duration) ? duration : (int?)null;

        public static string MapDuration(int? value) => value?.ToString("0.##");

        public static CourseDurationUnit? ResolveDurationUnit(string value) => value?.ToLower() switch
        {
            "hours" => CourseDurationUnit.Hours,
            "days" => CourseDurationUnit.Days,
            "weeks" => CourseDurationUnit.Weeks,
            "months" => CourseDurationUnit.Months,
            "years" => CourseDurationUnit.Years,
            _ => null
        };

        public static string MapDurationUnit(CourseDurationUnit? value) => value switch
        {
            CourseDurationUnit.Hours => "hours",
            CourseDurationUnit.Days => "days",
            CourseDurationUnit.Weeks => "weeks",
            CourseDurationUnit.Months => "months",
            CourseDurationUnit.Years => "years",
            null => null,
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static bool? ResolveFlexibleStartDate(string value) => value?.ToLower() switch
        {
            "yes" => true,
            "no" => false,
            "" => false,
            _ => null
        };

        public static string MapFlexibleStartDate(bool? value) => value switch
        {
            true => "yes",
            false => "no",
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static bool? ResolveNationalDelivery(string value) => value?.ToLower() switch
        {
            "yes" => true,
            "no" => false,
            _ => null
        };

        public static string MapNationalDelivery(bool? value) => value switch
        {
            true => "yes",
            false => "no",
            null => null,
        };

        public static DateTime? ResolveStartDate(string value) =>
            DateTime.TryParseExact(value, DateFormat, null, DateTimeStyles.None, out var dt) ? dt : (DateTime?)null;

        public static CourseStudyMode? ResolveStudyMode(string value) => value?.ToLower() switch
        {
            "full time" => CourseStudyMode.FullTime,
            "part time" => CourseStudyMode.PartTime,
            "flexible" => CourseStudyMode.Flexible,
            _ => (CourseStudyMode?)null
        };

        public static string MapStudyMode(CourseStudyMode? value) => value switch
        {
            0 => null,
            null => null,
            CourseStudyMode.FullTime => "full time",
            CourseStudyMode.PartTime => "part time",
            CourseStudyMode.Flexible => "flexible",
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
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
    }
}
