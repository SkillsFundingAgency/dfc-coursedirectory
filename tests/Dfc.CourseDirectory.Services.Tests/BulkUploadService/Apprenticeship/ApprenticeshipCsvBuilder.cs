using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService.Apprenticeship
{
    /// <summary>
    /// Builder Pattern implementation with fluent api for building arbitrary variations of csv data for testing bulk upload.
    /// </summary>
    internal class ApprenticeshipCsvBuilder
    {
        private bool _emptyFile;
        private bool _generateInvalidHeader;
        private bool _trailingNewline = true;
        private string _newLine = "\r\n";
        private readonly List<string> _rows = new List<string>();

        /// <summary>
        /// Lambda based API for adding rows.
        /// To add a default valid row:
        ///   `.WithRow()`
        /// To add a customised row:
        ///   `.WithRow(row => row.WithStandardCode("this-is-not-an-integer"))`
        /// </summary>
        public ApprenticeshipCsvBuilder WithRow(Action<ApprenticeshipCsvRowBuilder> configureRow = null)
        {
            var csvRowBuilder = new ApprenticeshipCsvRowBuilder();
            configureRow?.Invoke(csvRowBuilder);
            _rows.Add(csvRowBuilder.Build());
            return this;
        }

        public ApprenticeshipCsvBuilder WithoutRows()
        {
            // This method isn't strictly necessary, you can just add no rows,
            // but it makes the intent of the caller more explicit.

            _rows.Clear();
            return this;
        }

        public ApprenticeshipCsvBuilder WithUnixLineEndings()
        {
            _newLine = "\n";
            return this;
        }

        public ApprenticeshipCsvBuilder WithoutTrailingNewline()
        {
            _trailingNewline = false;
            return this;
        }

        public ApprenticeshipCsvBuilder WithInvalidHeader()
        {
            _generateInvalidHeader = true;
            return this;
        }

        public ApprenticeshipCsvBuilder EmptyFile()
        {
            _emptyFile = true;
            return this;
        }

        public Stream BuildStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(BuildString()));
        }

        private string BuildString()
        {
            if (_emptyFile)
            {
                return "";
            }

            var sb = new StringBuilder();
            sb.Append(CreateCsvHeader());
            sb.Append(_newLine);
            sb.Append(string.Join(_newLine, _rows));
            if (_trailingNewline)
            {
                sb.Append(_newLine);
            }

            return sb.ToString();
        }

        private string CreateCsvHeader()
        {
            var csvHeader = string.Join(",", ApprenticeshipCsvStructure.Fields);
            if (_generateInvalidHeader)
            {
                return "modified_first_header_name_" + csvHeader;
            }
            return csvHeader;
        }
    }
}
