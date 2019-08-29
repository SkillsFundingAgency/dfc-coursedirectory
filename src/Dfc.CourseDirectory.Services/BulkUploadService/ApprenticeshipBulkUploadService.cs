using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Dfc.CourseDirectory.Services.BulkUploadService
{
    public class ApprenticeshipBulkUploadService : IApprenticeshipBulkUploadService
    {
        private class ApprenticeshipCsvRecord
        {
            public int? STANDARD_CODE { get; set; }
            public int? STANDARD_VERSION { get; set; }
            public int? FRAMEWORK_CODE { get; set; }
            public int? FRAMEWORK_PROG_TYPE { get; set; }
            public int? FRAMEWORK_PATHWAY_CODE { get; set; }
            public string APPRENTICESHIP_INFORMATION { get; set; }
            public string APPRENTICESHIP_WEBPAGE { get; set; }
            public string CONTACT_EMAIL { get; set; }
            public string CONTACT_PHONE { get; set; }
            public string CONTACT_URL { get; set; }
            public string DELIVERY_METHOD { get; set; }
            public string VENUE { get; set; }
            public string RADIUS { get; set; }
            public string DELIVERY_MODE { get; set; }

            [Name("ACROSS ENGLAND")]  // Is this genuine or just a mistake in the test file?
            public string ACROSS_ENGLAND { get; set; }

            public string NATIONAL_DELIVERY { get; set; }
            public string REGION { get; set; }
            public string SUB_REGION { get; set; }
        }

        private class ApprenticeshipCsvRecordMap : ClassMap<ApprenticeshipCsvRecord>
        {
            public ApprenticeshipCsvRecordMap()
            {
                Map(m => m.STANDARD_CODE).ConvertUsing((IReaderRow row) => { return Validate_STANDARD_CODE(row); });
                Map(m => m.STANDARD_VERSION).ConvertUsing((IReaderRow row) => { return Validate_STANDARD_VERSION(row); });
                Map(m => m.FRAMEWORK_CODE).ConvertUsing((IReaderRow row) => { return Validate_FRAMEWORK_CODE(row); });
                Map(m => m.FRAMEWORK_PROG_TYPE).ConvertUsing((IReaderRow row) => { return Validate_FRAMEWORK_PROG_TYPE(row); });
                Map(m => m.FRAMEWORK_PATHWAY_CODE).ConvertUsing((IReaderRow row) => { return Validate_FRAMEWORK_PATHWAY_CODE(row); });
                Map(m => m.APPRENTICESHIP_INFORMATION).ConvertUsing((IReaderRow row) => { return Validate_APPRENTICESHIP_INFORMATION(row); });
                Map(m => m.APPRENTICESHIP_WEBPAGE).ConvertUsing((IReaderRow row) => { return Validate_APPRENTICESHIP_WEBPAGE(row); }) ;
                Map(m => m.CONTACT_EMAIL).ConvertUsing((IReaderRow row) => { return Validate_CONTACT_EMAIL(row); }) ;
                Map(m => m.CONTACT_PHONE).ConvertUsing((IReaderRow row) => { return Validate_CONTACT_PHONE(row); });
                Map(m => m.CONTACT_URL);
                Map(m => m.DELIVERY_METHOD);
                Map(m => m.VENUE);
                Map(m => m.RADIUS);
                Map(m => m.DELIVERY_MODE);
                Map(m => m.ACROSS_ENGLAND).Optional();
                Map(m => m.NATIONAL_DELIVERY);
                Map(m => m.REGION);
                Map(m => m.SUB_REGION);
            }

            private int? Validate_STANDARD_CODE(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "STANDARD_CODE");
                if(value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                }
                return value;
            }

            private int? Validate_STANDARD_VERSION(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "STANDARD_VERSION");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                }
                return value;
            }

            private int? Validate_FRAMEWORK_CODE(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "FRAMEWORK_CODE");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                }
                return value;
            }

            private int? Validate_FRAMEWORK_PROG_TYPE(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "FRAMEWORK_PROG_TYPE");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                }
                return value;
            }

            private int? Validate_FRAMEWORK_PATHWAY_CODE(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "FRAMEWORK_PATHWAY_CODE");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                }
                return value;
            }

            private int? ValueMustBeNumericIfPresent(IReaderRow row, string fieldName)
            {
                if (!row.TryGetField<int?>(fieldName, out int? value))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} must be numeric if present.");
                }
                return value;
            }

            private void ValuesForBothStandardAndFrameworkCannotBePresent(IReaderRow row)
            {
                row.TryGetField<int?>("STANDARD_CODE", out int? STANDARD_CODE);
                row.TryGetField<int?>("STANDARD_VERSION", out int? STANDARD_VERSION);
                row.TryGetField<int?>("FRAMEWORK_CODE", out int? FRAMEWORK_CODE);
                row.TryGetField<int?>("FRAMEWORK_PROG_TYPE", out int? FRAMEWORK_PROG_TYPE);
                row.TryGetField<int?>("FRAMEWORK_PATHWAY_CODE", out int? FRAMEWORK_PATHWAY_CODE);

                if(STANDARD_CODE.HasValue || STANDARD_VERSION.HasValue)
                {
                    if(FRAMEWORK_CODE.HasValue || FRAMEWORK_PROG_TYPE.HasValue || FRAMEWORK_PATHWAY_CODE.HasValue)
                    {
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Values for Both Standard AND Framework cannot be present in the same row.");
                    }
                }
            }

            private string Validate_APPRENTICESHIP_INFORMATION(IReaderRow row)
            {
                string fieldName = "APPRENTICESHIP_INFORMATION";
                if (!row.TryGetField<string>(fieldName, out string value))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
                if(string.IsNullOrWhiteSpace(value))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
                if(value.Length > 750)
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 750 characters.");
                }
                return value;
            }
            private string Validate_APPRENTICESHIP_WEBPAGE(IReaderRow row)
            {
                string fieldName = "APPRENTICESHIP_WEBPAGE";
                if (!row.TryGetField<string>(fieldName, out string value))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
                if(!string.IsNullOrWhiteSpace(value))
                {
                    var regex = @"^([-a-zA-Z0-9]{2,256}\.)+[a-z]{2,10}(\/.*)?";
                    if (Regex.IsMatch(value, regex))
                    {
                        throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} format of URL is incorrect.");
                    }
                    if (value.Length > 255)
                    {
                        throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 255 characters.");
                    }
                }

                return value;
            }
            private string Validate_CONTACT_EMAIL(IReaderRow row)
            {
                string fieldName = "CONTACT_EMAIL";
                if (!row.TryGetField<string>(fieldName, out string value))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
                if(string.IsNullOrWhiteSpace(value))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
                if (value.Length > 255)
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 255 characters.");
                }
                var emailRegEx = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                if (!Regex.IsMatch(value, emailRegEx))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} needs a valid email.");
                }
                return value;
            }
            private string Validate_CONTACT_PHONE(IReaderRow row)
            {
                string fieldName = "CONTACT_PHONE";
                if (!row.TryGetField<string>(fieldName, out string value))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
                value = RemoveWhiteSpace(value);
                int? isNumeric = ValueMustBeNumericIfPresent(row, fieldName);

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
               

                if(value.Length > 30)
                {
                    throw new FieldValidationException(row.Context, fieldName, $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 30 characters.");
                }
                return value;
            }
        }

        private readonly ILogger<ApprenticeshipBulkUploadService> _logger;

        public ApprenticeshipBulkUploadService(
            ILogger<ApprenticeshipBulkUploadService> logger
            )
        {
            Throw.IfNull(logger, nameof(logger));

            _logger = logger;
        }

        public int CountCsvLines(Stream stream)
        {
            Throw.IfNull(stream, nameof(stream));

            int count = 0;
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);  // don't dispose the stream we might need it later.
            while (sr.ReadLine() != null)
            {
                ++count;
            }

            return count;
        }

        public List<string> ValidateCSVFormat(Stream stream)
        {
            Throw.IfNull(stream, nameof(stream));

            List<string> errors = new List<string>();
            int processedRowCount = 0;
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    if (0 == stream.Length) throw new Exception("File is empty.");
                    stream.Seek(0,SeekOrigin.Begin);
                    using (var csv = new CsvReader(reader))
                    {
                        // Validate the header row.
                        ValidateHeader(csv);

                        // Now parse the data in the remaining rows. 
                        csv.Configuration.RegisterClassMap<ApprenticeshipCsvRecordMap>();
                        while (csv.Read())
                        {
                            var record = csv.GetRecord<ApprenticeshipCsvRecord>();
                            processedRowCount++;
                        }
                    }

                    if(0 == processedRowCount)
                    {
                        throw new Exception("No apprenticeship data present in the file.");
                    }
                }
            }
            catch (ReaderException ex)
            {
                Exception rootex = ex;
                while (null != rootex.InnerException)
                {
                    rootex = rootex.InnerException;
                }
                errors.Add($"Invalid file format. {rootex.Message}");
            }
            catch (HeaderValidationException ex)
            {
                string errmsg = $"Invalid header row. {ex.Message.FirstSentence()}";
                errors.Add(errmsg);
            }
            catch (FieldValidationException ex)
            {
                errors.Add($"{ex.Message}");
            }
            catch (BadDataException ex)
            {
                errors.Add($"{ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add($"{ex.Message}");
            }

            return errors;
        }

        private void ValidateHeader(CsvReader csv)
        {
            // Ignore whitespace in the headers.
            csv.Configuration.PrepareHeaderForMatch = (string header, int index) => Regex.Replace(header, @"\s", string.Empty);
            
            // Validate the header.
            csv.Read();
            csv.ReadHeader();
            csv.ValidateHeader<ApprenticeshipCsvRecord>();
        }
        private static string RemoveWhiteSpace(string value)
        {
            return Regex.Replace(value, @"\s+", "");
        }
    }
}
