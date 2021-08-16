﻿using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ParsedCsvApprenticeshipRow : CsvApprenticeshipRow
    {
        private const string DateFormat = "dd/MM/yyyy";
        public ApprenticeshipDeliveryMode? ResolvedDeliveryMode { get; private set; }
        public IReadOnlyCollection<Region> ResolvedSubRegions { get; private set; }
        public ApprenticeshipLocationType? ResolvedDeliveryMethod { get; private set; }
        public bool? ResolvedNationalDelivery { get; private set; }
        public int? ResolvedRadius { get; private set; }

        private ParsedCsvApprenticeshipRow()
        {
        }

        public static ParsedCsvApprenticeshipRow FromCsvApprenticeshipRow(CsvApprenticeshipRow row)
        {
            var parsedRow = row.Adapt(new ParsedCsvApprenticeshipRow());
            parsedRow.ResolvedDeliveryMode = ResolveDeliveryMode(row.DeliveryMode);
            parsedRow.ResolvedDeliveryMethod = ResolveDeliveryMethod(row.DeliveryMethod);
            parsedRow.ResolvedNationalDelivery = ResolveNationalDelivery(row.NationalDelivery);
            parsedRow.ResolvedRadius = ResolveRadius(row.Radius);

            return parsedRow;
        }


        public static bool? ResolveNationalDelivery(string value) => value?.ToLower() switch
        {
            "yes" => true,
            "no" => false,
            _ => null
        };

        public static int? ResolveRadius(string value)
        {
            if (int.TryParse(value?.ToLower(), out int radius))
                return radius;
            return null;
        }

        public static ApprenticeshipDeliveryMode? ResolveDeliveryMode(string value) => value?.ToLower() switch
        {
            "block release" => ApprenticeshipDeliveryMode.BlockRelease,
            "block" => ApprenticeshipDeliveryMode.BlockRelease,
            "day release" => ApprenticeshipDeliveryMode.DayRelease,
            "day" => ApprenticeshipDeliveryMode.DayRelease,
            "employer address" => ApprenticeshipDeliveryMode.EmployerAddress,
            "employer" => ApprenticeshipDeliveryMode.EmployerAddress,
            _ => (ApprenticeshipDeliveryMode?)null
        };

        public static ApprenticeshipLocationType? ResolveDeliveryMethod(string value) => value?.ToLower() switch
        {
            "classroom based" => ApprenticeshipLocationType.ClassroomBased,
            "classroom" => ApprenticeshipLocationType.ClassroomBased,
            "employer based" => ApprenticeshipLocationType.EmployerBased,
            "emplyoer" => ApprenticeshipLocationType.EmployerBased,
            "both" => ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
            "classroom based and employer based" => ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
            _ => (ApprenticeshipLocationType?)null
        };

        public static string MapDeliveryMode(ApprenticeshipDeliveryMode? value) => value switch
        {
            ApprenticeshipDeliveryMode.BlockRelease => "Block Release",
            ApprenticeshipDeliveryMode.DayRelease => "Day Release",
            ApprenticeshipDeliveryMode.EmployerAddress => "Employer Address",
            null => null,
            _ => throw new NotSupportedException($"Unknown value: '{value}'."),
        };

        public static string MapDeliveryMethod(ApprenticeshipLocationType? value) => value switch
        {
            ApprenticeshipLocationType.ClassroomBased => "Classroom Based",
            ApprenticeshipLocationType.ClassroomBasedAndEmployerBased => "Classroom Based And Employer Based",
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
    }
}
