using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService
{
    /// <summary>
    /// Builder Pattern implementation with fluent api for building arbitrary csv row data for bulk upload testing
    /// </summary>
    internal class ApprenticeshipCsvRowBuilder
    {
        public const string DefaultTestVenueName = "A Test Venue";

        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(
            ApprenticeshipCsvStructure.Fields.ToDictionary(fieldName => fieldName, _ => ""));

        public ApprenticeshipCsvRowBuilder()
        {
            SetValidDefaultValues();
        }

        public ApprenticeshipCsvRowBuilder With(string field, string value)
        {
            if (!_values.ContainsKey(field))
            {
                throw new ArgumentOutOfRangeException(field, $"Unknown field '{field}'.");
            }

            _values[field] = value;
            return this;
        }

        public ApprenticeshipCsvRowBuilder WithStandardCode()
        {
            return With("STANDARD_CODE", "122").With("STANDARD_VERSION", "1");
        }

        public ApprenticeshipCsvRowBuilder WithFrameworkCode()
        {
            return With("FRAMEWORK_CODE", "487").With("FRAMEWORK_PROG_TYPE", "21").With("FRAMEWORK_PATHWAY_CODE", "1");
        }

        public string Build()
        {
            return string.Join(",",
                ApprenticeshipCsvStructure.Fields.Select(
                    fieldName => _values[fieldName]));
        }

        private void SetValidDefaultValues()
        {
            With("ACROSS_ENGLAND", "No");
            With("APPRENTICESHIP_INFORMATION", "description number 1");
            With("CONTACT_EMAIL", "info@example.co.uk");
            With("CONTACT_PHONE", "1414960001");
            With("DELIVERY_METHOD", "Both");
            With("DELIVERY_MODE", "Employer");
            With("RADIUS", "100");
            With("VENUE", DefaultTestVenueName);
        }
    }
}
