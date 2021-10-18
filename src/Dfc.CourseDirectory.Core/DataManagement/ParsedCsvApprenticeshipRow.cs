using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ParsedCsvApprenticeshipRow : CsvApprenticeshipRow
    {
        public IReadOnlyCollection<ApprenticeshipDeliveryMode> ResolvedDeliveryModes { get; private set; }
        public IReadOnlyCollection<Region> ResolvedSubRegions { get; private set; }
        public ApprenticeshipLocationType? ResolvedDeliveryMethod { get; private set; }
        public bool? ResolvedNationalDelivery { get; private set; }
        public int? ResolvedRadius { get; private set; }

        private ParsedCsvApprenticeshipRow()
        {
        }

        public static ParsedCsvApprenticeshipRow FromCsvApprenticeshipRow(CsvApprenticeshipRow row, IEnumerable<Region> allRegions)
        {
            var parsedRow = row.Adapt(new ParsedCsvApprenticeshipRow());
            parsedRow.ResolvedDeliveryModes = ResolveDeliveryModes(row.DeliveryModes);
            parsedRow.ResolvedDeliveryMethod = ResolveDeliveryMethod(row.DeliveryMethod, parsedRow.ResolvedDeliveryModes);
            parsedRow.ResolvedNationalDelivery = ResolveNationalDelivery(row.NationalDelivery, parsedRow.SubRegion, parsedRow.ResolvedDeliveryMethod);
            parsedRow.ResolvedRadius = ResolveRadius(row.Radius);
            parsedRow.ResolvedSubRegions = ResolveSubRegions(parsedRow.SubRegion, allRegions);
            return parsedRow;
        }

        public static bool? ResolveNationalDelivery(string value, string subRegions, ApprenticeshipLocationType? deliveryMethod)
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

            // Treat an empty value as 'no' if sub regions were specified for an Employer-based delivery method
            if (string.IsNullOrWhiteSpace(value) && subRegionsWereSpecified && deliveryMethod == ApprenticeshipLocationType.EmployerBased)
            {
                return false;
            }

            return null;
        }

        public static int? ResolveRadius(string value)
        {
            if (int.TryParse(value?.ToLower(), out int radius))
                return radius;
            return null;
        }

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

        public static IReadOnlyCollection<ApprenticeshipDeliveryMode> ResolveDeliveryModes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<ApprenticeshipDeliveryMode>();
            }

            var parsed = value.ToLower()
                .Split(DeliveryModeDelimiter, StringSplitOptions.RemoveEmptyEntries)
                .Select(dm => dm.Trim() switch
                {
                    "block release" => ApprenticeshipDeliveryMode.BlockRelease,
                    "block" => ApprenticeshipDeliveryMode.BlockRelease,
                    "day release" => ApprenticeshipDeliveryMode.DayRelease,
                    "day" => ApprenticeshipDeliveryMode.DayRelease,
                    "employer address" => ApprenticeshipDeliveryMode.EmployerAddress,
                    "employer" => ApprenticeshipDeliveryMode.EmployerAddress,
                    _ => (ApprenticeshipDeliveryMode?)null
                })
                .Distinct()
                .ToArray();

            // If any element failed parsing return an empty collection
            if (parsed.Any(v => !v.HasValue))
            {
                return Array.Empty<ApprenticeshipDeliveryMode>();
            }

            return parsed.Cast<ApprenticeshipDeliveryMode>().ToArray();
        }

        public static ApprenticeshipLocationType? ResolveDeliveryMethod(string value, IReadOnlyCollection<ApprenticeshipDeliveryMode> deliveryModes) =>
            (value?.ToLower()?.Trim(), deliveryModes?.Contains(ApprenticeshipDeliveryMode.EmployerAddress) ?? false) switch
            {
                ("classroom based", false) => ApprenticeshipLocationType.ClassroomBased,
                ("classroom", false) => ApprenticeshipLocationType.ClassroomBased,
                ("classroom based", true) => ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
                ("classroom", true) => ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
                ("employer based", _) => ApprenticeshipLocationType.EmployerBased,
                ("employer", _) => ApprenticeshipLocationType.EmployerBased,
                _ => (ApprenticeshipLocationType?)null
            };

        public static string MapDeliveryModes(IEnumerable<ApprenticeshipDeliveryMode> value) =>
            string.Join(
                DeliveryModeDelimiter,
                value.Select(v => v switch
                {
                    ApprenticeshipDeliveryMode.BlockRelease => "Block Release",
                    ApprenticeshipDeliveryMode.DayRelease => "Day Release",
                    ApprenticeshipDeliveryMode.EmployerAddress => "Employer Address",
                    _ => throw new NotSupportedException($"Unknown value: '{value}'."),
                }));

        public static string MapDeliveryMethod(ApprenticeshipLocationType? value) => value switch
        {
            ApprenticeshipLocationType.ClassroomBased => "Classroom Based",
            ApprenticeshipLocationType.ClassroomBasedAndEmployerBased => "Classroom Based",
            ApprenticeshipLocationType.EmployerBased => "Employer Based",
            null => null,
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
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

        public static string MapStandardCode(int standardCode) => standardCode.ToString();

        public static string MapStandardVersion(int standardVersion) => standardVersion.ToString();
    }
}
