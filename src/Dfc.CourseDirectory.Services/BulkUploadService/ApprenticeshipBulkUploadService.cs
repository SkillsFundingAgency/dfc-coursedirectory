using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dfc.CourseDirectory.Models.Helpers;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;


namespace Dfc.CourseDirectory.Services.BulkUploadService
{
    
    public class ApprenticeshipBulkUploadService : IApprenticeshipBulkUploadService
    {

        private enum DeliveryMode
        {
            Undefined = 0,
            Classroom = 1,
            Employer = 2,
            Both = 3
        }
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
            public string ACROSS_ENGLAND { get; set; }
            public string NATIONAL_DELIVERY { get; set; }
            public string REGION { get; set; }
            public string SUB_REGION { get; set; }
            public List<string> ErrorsList { get; set; }
        }

        private class ApprenticeshipCsvRecordMap : ClassMap<ApprenticeshipCsvRecord>
        {
            private readonly IApprenticeshipService _apprenticeshipService;
            public ApprenticeshipCsvRecordMap(IApprenticeshipService apprenticeshipService)
            {
                Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
                _apprenticeshipService = apprenticeshipService;
                Map(m => m.STANDARD_CODE).ConvertUsing((row) => { return Mandatory_Checks_STANDARD_CODE(row); });
                Map(m => m.STANDARD_VERSION).ConvertUsing((row) => { return Mandatory_Checks_STANDARD_VERSION(row); });
                Map(m => m.FRAMEWORK_CODE).ConvertUsing((row) => { return Mandatory_Checks_FRAMEWORK_CODE(row); });
                Map(m => m.FRAMEWORK_PROG_TYPE).ConvertUsing((row) => { return Mandatory_Checks_FRAMEWORK_PROG_TYPE(row); });
                Map(m => m.FRAMEWORK_PATHWAY_CODE).ConvertUsing((row) => { return Mandatory_Checks_FRAMEWORK_PATHWAY_CODE(row); });
                Map(m => m.APPRENTICESHIP_INFORMATION);
                Map(m => m.APPRENTICESHIP_WEBPAGE);
                Map(m => m.CONTACT_EMAIL);
                Map(m => m.CONTACT_PHONE);
                Map(m => m.CONTACT_URL);
                Map(m => m.DELIVERY_METHOD);
                Map(m => m.VENUE);
                Map(m => m.RADIUS);
                Map(m => m.DELIVERY_MODE);
                Map(m => m.ACROSS_ENGLAND);
                Map(m => m.NATIONAL_DELIVERY);
                Map(m => m.REGION);
                Map(m => m.SUB_REGION);
                Map(m => m.ErrorsList).ConvertUsing((row) => { return ValidateData(row); });
            }

            #region Basic Checks
            private int? Mandatory_Checks_STANDARD_CODE(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "STANDARD_CODE");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                }
                return value;
            }

            private int? Mandatory_Checks_STANDARD_VERSION(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "STANDARD_VERSION");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                    row.TryGetField<int?>("STANDARD_CODE", out int? STANDARD_CODE);
                    var result = DoesStandardExist(STANDARD_CODE, value);
                    if (result == false)
                    {
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Invalid Standard Code or Version Number. Standard not found.");
                    }
                }
                return value;
            }

            private int? Mandatory_Checks_FRAMEWORK_CODE(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "FRAMEWORK_CODE");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                }
                return value;
            }

            private int? Mandatory_Checks_FRAMEWORK_PROG_TYPE(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "FRAMEWORK_PROG_TYPE");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                }
                return value;
            }

            private int? Mandatory_Checks_FRAMEWORK_PATHWAY_CODE(IReaderRow row)
            {
                int? value = ValueMustBeNumericIfPresent(row, "FRAMEWORK_PATHWAY_CODE");
                if (value.HasValue)
                {
                    ValuesForBothStandardAndFrameworkCannotBePresent(row);
                    row.TryGetField<int?>("FRAMEWORK_CODE", out int? FRAMEWORK_CODE);
                    row.TryGetField<int?>("FRAMEWORK_PROG_TYPE", out int? FRAMEWORK_PROG_TYPE);
                    var result = DoesFrameworkExist(FRAMEWORK_CODE, FRAMEWORK_PROG_TYPE, value);
                    if (result == false)
                    {
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Invalid Framework Code, Prog Type, or Pathway Code. Framework not found.");
                    }
                }
                return value;
            }
            #endregion

            #region Field Validation

            private List<string> ValidateData(IReaderRow row)
            {
                List<string> errors = new List<string>();
                errors.AddRange(Validate_APPRENTICESHIP_INFORMATION(row));
                errors.AddRange(Validate_APPRENTICESHIP_WEBPAGE(row));
                errors.AddRange(Validate_CONTACT_EMAIL(row));
                errors.AddRange(Validate_CONTACT_PHONE(row));
                errors.AddRange(Validate_CONTACT_URL(row));
                errors.AddRange(Validate_DELIVERY_METHOD(row));
                errors.AddRange(Validate_VENUE(row));

                return errors;
            }

            private List<string> Validate_APPRENTICESHIP_INFORMATION(IReaderRow row)
            {
                List<string> errors = new List<string>();
                string fieldName = "APPRENTICESHIP_INFORMATION";
                row.TryGetField<string>(fieldName, out string value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
                if (!string.IsNullOrEmpty(value) && value.Length > 750)
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 750 characters.");
                }
                return errors;
            }
            private List<string> Validate_APPRENTICESHIP_WEBPAGE(IReaderRow row)
            {
                List<string> errors = new List<string>();
                string fieldName = "APPRENTICESHIP_WEBPAGE";
                row.TryGetField<string>(fieldName, out string value);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var regex = @"^([-a-zA-Z0-9]{2,256}\.)+[a-z]{2,10}(\/.*)?";
                    if (Regex.IsMatch(value, regex))
                    {
                        errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} format of URL is incorrect.");
                    }
                    if (value.Length > 255)
                    {
                        errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 255 characters.");
                    }
                }

                return errors;
            }
            private List<string> Validate_CONTACT_EMAIL(IReaderRow row)
            {
                List<string> errors = new List<string>();
                string fieldName = "CONTACT_EMAIL";
                row.TryGetField<string>(fieldName, out string value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                    return errors;
                }
                if (!string.IsNullOrEmpty(value) && value.Length > 255)
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 255 characters.");
                    return errors;
                }

                var emailRegEx = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                if (!Regex.IsMatch(value, emailRegEx))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} needs a valid email.");
                    
                }
                return errors;
            }
            private List<string> Validate_CONTACT_PHONE(IReaderRow row)
            {
                List<string> errors = new List<string>();

                string fieldName = "CONTACT_PHONE";
                row.TryGetField<string>(fieldName, out string value); 
                value = RemoveWhiteSpace(value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                    return errors;
                }
                if (!string.IsNullOrEmpty(value) && value.Length > 30)
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 30 characters.");
                    return errors;
                }
                if (!int.TryParse(value, out int numericalValue))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} must be numeric if present.");
                    return errors;
                }
                return errors;
            }
            private List<string> Validate_CONTACT_URL(IReaderRow row)
            {
                
                List<string> errors = new List<string>();
                string fieldName = "CONTACT_URL";
                row.TryGetField(fieldName, out string value);
                if (string.IsNullOrEmpty(value))
                {
                    return errors;
                }
                if (value.Length > 255)
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 255 characters.");
                }
                var urlRegex = @"^([-a-zA-Z0-9]{2,256}\.)+[a-z]{2,10}(\/.*)?";
                if (Regex.IsMatch(value, urlRegex))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} format of URL is incorrect.");
                }
                return errors;
            }
            private List<string> Validate_DELIVERY_METHOD(IReaderRow row)
            {
                List<string> errors = new List<string>();
                string fieldName = "DELIVERY_METHOD";
                row.TryGetField<string>(fieldName, out string value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                    return errors;
                }

                var deliveryMethod = value.ToEnum(DeliveryMode.Undefined);
                if (deliveryMethod == DeliveryMode.Undefined)
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is invalid.");
                }
                return errors;
            }
            private List<string> Validate_VENUE(IReaderRow row)
            {
                List<string> errors = new List<string>();
                string fieldName = "VENUE";
                

                return errors;
            }
            #endregion


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

            private bool DoesStandardExist(int? standardCode, int? version)
            {
                var result = _apprenticeshipService.GetStandardByCode(new StandardSearchCriteria
                    {StandardCode = standardCode, Version = version}).Result;

                if (result.IsSuccess && result.HasValue && result.Value.Any())
                {
                    var standard = result.Value.FirstOrDefault();
                    if(standard != null && 
                       (standard.StandardCode == standardCode && standard.Version == version))
                        return true;
                }

                return false;
            }
            private bool DoesFrameworkExist(int? frameworkCode, int? progType, int? pathwayCode)
            {
                var result = _apprenticeshipService.GetFrameworkByCode(new FrameworkSearchCriteria
                    {FrameworkCode = frameworkCode, ProgType = progType, PathwayCode = pathwayCode}).Result;
                if (result.IsSuccess && result.HasValue && result.Value.Any())
                {
                    var framework = result.Value.FirstOrDefault();
                    if(framework != null &&(framework.FrameworkCode == frameworkCode 
                                            && framework.ProgType == progType
                                            && framework.PathwayCode == pathwayCode))
                        return true;
                }
                return false;
            }
           
        }

        private readonly ILogger<ApprenticeshipBulkUploadService> _logger;
        private readonly IApprenticeshipService _apprenticeshipService;
        public ApprenticeshipBulkUploadService(
            ILogger<ApprenticeshipBulkUploadService> logger,
            IApprenticeshipService apprenticeshipService
            )
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            _logger = logger;
            _apprenticeshipService = apprenticeshipService;
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
            List<ApprenticeshipCsvRecord> records = new List<ApprenticeshipCsvRecord>();
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
                        var classMap = new ApprenticeshipCsvRecordMap(_apprenticeshipService);
                        csv.Configuration.RegisterClassMap(classMap);
                        while (csv.Read())
                        {
                            var record = csv.GetRecord<ApprenticeshipCsvRecord>();
                            records.Add(record);
                            CheckForDuplicates(records);
                            errors.AddRange(record.ErrorsList);

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

        private void CheckForDuplicates(List<ApprenticeshipCsvRecord> records)
        {
            CheckForStandardDuplicates(records);
        }

        private void CheckForStandardDuplicates(List<ApprenticeshipCsvRecord> records)
        {
            var duplicates = records.GroupBy(x => x.STANDARD_CODE.HasValue).Any(y => y.Count( ) > 1);
        }
    }
}
