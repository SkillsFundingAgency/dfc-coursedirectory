using System;
using System.Globalization;
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

        public CourseDeliveryMode? ResolvedDeliveryMode => ResolveDeliveryMode(DeliveryMode);
        public DateTime? ResolvedStartDate => ResolveStartDate(StartDate);
        public bool? ResolvedFlexibleStartDate => ResolveFlexibleStartDate(FlexibleStartDate);
        public bool? ResolvedNationalDelivery => ResolveNationalDelivery(NationalDelivery);
        public decimal? ResolvedCost => ResolveCost(Cost);
        public int? ResolvedDuration => ResolveDuration(Duration);
        public CourseDurationUnit? ResolvedDurationUnit => ResolveDurationUnit(DurationUnit);
        public CourseStudyMode? ResolvedStudyMode => ResolveStudyMode(StudyMode);
        public CourseAttendancePattern? ResolvedAttendancePattern => ResolveAttendancePattern(AttendancePattern);

        public static ParsedCsvCourseRow FromCsvCourseRow(CsvCourseRow row)
        {
            var parsedRow = new ParsedCsvCourseRow();
            return row.Adapt(parsedRow);
        }

        public static CourseAttendancePattern? ResolveAttendancePattern(string value) => value?.ToLower() switch
        {
            "daytime" => CourseAttendancePattern.Daytime,
            "evening" => CourseAttendancePattern.Evening,
            "weekend" => CourseAttendancePattern.Weekend,
            "day/block release" => CourseAttendancePattern.DayOrBlockRelease,
            _ => (CourseAttendancePattern?)null
        };

        public static decimal? ResolveCost(string value) =>
            decimal.TryParse(value, out var result) && GetDecimalPlaces(result) <= 2 ? result : (decimal?)null;

        public static CourseDeliveryMode? ResolveDeliveryMode(string value) => value?.ToLower() switch
        {
            "classroom based" => CourseDeliveryMode.ClassroomBased,
            "classroom" => CourseDeliveryMode.ClassroomBased,
            "online" => CourseDeliveryMode.Online,
            "work based" => CourseDeliveryMode.WorkBased,
            "work" => CourseDeliveryMode.WorkBased,
            _ => (CourseDeliveryMode?)null
        };

        public static int? ResolveDuration(string value) =>
            int.TryParse(value, out var duration) ? duration : (int?)null;

        public static CourseDurationUnit? ResolveDurationUnit(string value) => value?.ToLower() switch
        {
            "hours" => CourseDurationUnit.Hours,
            "days" => CourseDurationUnit.Days,
            "weeks" => CourseDurationUnit.Weeks,
            "months" => CourseDurationUnit.Months,
            "years" => CourseDurationUnit.Years,
            _ => (CourseDurationUnit?)null
        };

        public static bool? ResolveFlexibleStartDate(string value) => value?.ToLower() switch
        {
            "yes" => true,
            "no" => false,
            "" => false,
            _ => null
        };

        public static bool? ResolveNationalDelivery(string value) => value?.ToLower() switch
        {
            "yes" => true,
            "no" => false,
            _ => null
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
