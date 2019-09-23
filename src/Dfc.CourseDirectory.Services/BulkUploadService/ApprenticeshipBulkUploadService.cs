using CsvHelper;
using CsvHelper.Configuration;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Helpers;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;


namespace Dfc.CourseDirectory.Services.BulkUploadService
{

    public class ApprenticeshipBulkUploadService : IApprenticeshipBulkUploadService
    {

        private enum DeliveryMethod
        {
            Undefined = 0,
            Classroom = 1,
            Employer = 2,
            Both = 3
        }

        private enum DeliveryMode
        {
            Undefined = 0,
            Day = 1,
            Block = 2,
            Employer = 3
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
            public DeliveryMethod DELIVERY_METHOD { get; set; }
            public string VENUE { get; set; }
            public int? RADIUS { get; set; }
            public string DELIVERY_MODE { get; set; }
            public bool? ACROSS_ENGLAND { get; set; }
            public bool? NATIONAL_DELIVERY { get; set; }
            public string REGION { get; set; }
            public string SUB_REGION { get; set; }
            public List<string> ErrorsList { get; set; }
            [Ignore]
            public int RowNumber  { get; set; }
            [Ignore]
            public string Base64Row  { get; set; }
            [Ignore]
            public Guid VenueId  { get; set; }

        }

        private class ApprenticeshipCsvRecordMap : ClassMap<ApprenticeshipCsvRecord>
        {
            private readonly IApprenticeshipService _apprenticeshipService;
            private readonly IVenueService _venueService;
            public ApprenticeshipCsvRecordMap(IApprenticeshipService apprenticeshipService,
                IVenueService venueService)
            {
                Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
                _apprenticeshipService = apprenticeshipService;

                Map(m => m.STANDARD_CODE).ConvertUsing(Mandatory_Checks_STANDARD_CODE);
                Map(m => m.STANDARD_VERSION).ConvertUsing(Mandatory_Checks_STANDARD_VERSION);
                Map(m => m.FRAMEWORK_CODE).ConvertUsing(Mandatory_Checks_FRAMEWORK_CODE);
                Map(m => m.FRAMEWORK_PROG_TYPE).ConvertUsing(Mandatory_Checks_FRAMEWORK_PROG_TYPE);
                Map(m => m.FRAMEWORK_PATHWAY_CODE).ConvertUsing(Mandatory_Checks_FRAMEWORK_PATHWAY_CODE);
                Map(m => m.APPRENTICESHIP_INFORMATION);
                Map(m => m.APPRENTICESHIP_WEBPAGE);
                Map(m => m.CONTACT_EMAIL);
                Map(m => m.CONTACT_PHONE);
                Map(m => m.CONTACT_URL);
                Map(m => m.DELIVERY_METHOD).ConvertUsing(Mandatory_Checks_DELIVERY_METHOD);
                Map(m => m.VENUE);
                Map(m => m.RADIUS).ConvertUsing(Mandatory_Checks_RADIUS);
                Map(m => m.DELIVERY_MODE);
                Map(m => m.ACROSS_ENGLAND).ConvertUsing(row => Mandatory_Checks_Bool(row, "ACROSS_ENGLAND"));
                Map(m => m.NATIONAL_DELIVERY).ConvertUsing(row => Mandatory_Checks_Bool(row, "NATIONAL_DELIVERY"));
                Map(m => m.REGION);
                Map(m => m.SUB_REGION);
                Map(m => m.ErrorsList).ConvertUsing(ValidateData);
                Map(m => m.RowNumber).ConvertUsing(row => row.Context.RawRow);
                Map(m => m.Base64Row).ConvertUsing(Base64Encode);


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

            private DeliveryMethod Mandatory_Checks_DELIVERY_METHOD(IReaderRow row)
            {
                string fieldName = "DELIVERY_METHOD";
                row.TryGetField<string>(fieldName, out string value);

                if (String.IsNullOrWhiteSpace(value))
                {
                    return DeliveryMethod.Undefined;
                }
                var deliveryMethod = value.ToEnum(DeliveryMethod.Undefined);
                if (deliveryMethod == DeliveryMethod.Undefined)
                {
                    return DeliveryMethod.Undefined;
                }
                return deliveryMethod;
            }

            private int? Mandatory_Checks_RADIUS(IReaderRow row)
            {

                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                if (deliveryMethod != DeliveryMethod.Both)
                {
                    return null;
                }

                var isAcrossEngland = Mandatory_Checks_Bool(row, "ACROSS_ENGLAND");

                if (isAcrossEngland == true)
                {
                    return 600;
                }

                string fieldName = "RADIUS";
                return ValueMustBeNumericIfPresent(row, fieldName);
            }

            private bool? Mandatory_Checks_Bool(IReaderRow row, string fieldName)
            {
                
                row.TryGetField<string>(fieldName, out string value);

                switch (value.ToUpper())
                {
                    case "YES" :
                        return true;
                    case "NO":
                        return false;
                }

                return null;
            }
            //private Guid Mandatory_Checks_VENUE(IReaderRow row)
            //{
            //    //var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);

            //    //if (deliveryMethod == DeliveryMethod.Undefined || deliveryMethod == DeliveryMethod.Employer)
            //    //{
                    
            //    //}
            //    //string fieldName = "VENUE";
            //    //row.TryGetField<string>(fieldName, out string value);
            //    //if (String.IsNullOrWhiteSpace(value))
            //    //{
            //    //    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
            //    //    return errors;
            //    //}

            //    //return errors;
            //}
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
                errors.AddRange(Validate_RADIUS(row));
                errors.AddRange(Validate_DELIVERY_MODE(row));
                errors.AddRange(Validate_ACROSS_ENGLAND(row));
                errors.AddRange(Validate_NATIONAL_DELIVERY(row));
                

                return errors;
            }

            private List<string> Validate_APPRENTICESHIP_INFORMATION(IReaderRow row)
            {
                List<string> errors = new List<string>();
                string fieldName = "APPRENTICESHIP_INFORMATION";
                row.TryGetField<string>(fieldName, out string value);

                if (String.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }
                if (!String.IsNullOrEmpty(value) && value.Length > 750)
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
                if (!String.IsNullOrWhiteSpace(value))
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

                if (String.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                    return errors;
                }
                if (!String.IsNullOrEmpty(value) && value.Length > 255)
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

                if (String.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                    return errors;
                }
                if (!String.IsNullOrEmpty(value) && value.Length > 30)
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 30 characters.");
                    return errors;
                }
                if (!Int32.TryParse(value, out int numericalValue))
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
                if (String.IsNullOrEmpty(value))
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

                if (String.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                    return errors;
                }

                var deliveryMethod = value.ToEnum(DeliveryMethod.Undefined);
                if (deliveryMethod == DeliveryMethod.Undefined)
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is invalid.");
                }
                return errors;
            }
            private List<string> Validate_VENUE(IReaderRow row)
            {
                List<string> errors = new List<string>();

                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);

                if (deliveryMethod == DeliveryMethod.Undefined || deliveryMethod == DeliveryMethod.Employer)
                {
                    return errors;
                }
                string fieldName = "VENUE";
                row.TryGetField(fieldName, out string value);
                if (String.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                    return errors;
                }

                return errors;
            }
            private List<string> Validate_RADIUS(IReaderRow row)
            {
                List<string> errors = new List<string>();

                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                if (deliveryMethod != DeliveryMethod.Both)
                {
                    return errors;
                }

                string fieldName = "RADIUS";
                var value = Mandatory_Checks_RADIUS(row);
                if (value.HasValue)
                {
                    if (value <= 0)
                    {
                        errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} must be a valid number");
                        return errors;
                    }

                    if (value > 874)
                    {
                        errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} must be between 1 and 874");
                        return errors;
                    }
                }
                return errors;
            }
            private List<string> Validate_DELIVERY_MODE(IReaderRow row)
            {
                List<string> errors = new List<string>();

                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                if (deliveryMethod == DeliveryMethod.Undefined || deliveryMethod == DeliveryMethod.Employer)
                {
                    return errors;
                }
                string fieldName = "DELIVERY_MODE";
                row.TryGetField(fieldName, out string value);

                
                Dictionary<DeliveryMode, string> modes = new Dictionary<DeliveryMode, string>();

                var modeArray = value.Split(";");

                foreach (var mode in modeArray)
                {
                    var deliveryMode = mode.ToEnum(DeliveryMode.Undefined);
                    if (deliveryMode == DeliveryMode.Undefined)
                    {
                        errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} must be a valid Delivery Mode");
                        return errors;
                    }

                    if (!modes.TryAdd(deliveryMode, deliveryMode.ToString()))
                    {
                        errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} must contain unique Delivery Modes");
                        return errors;
                    }
                }
                return errors;
            }
            private List<string> Validate_ACROSS_ENGLAND(IReaderRow row)
            {
                List<string> errors = new List<string>();
                string fieldName = "ACROSS_ENGLAND";
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                var isAcrossEngland = Mandatory_Checks_Bool(row, fieldName);
                if (deliveryMethod == DeliveryMethod.Both)
                {
                    if (!isAcrossEngland.HasValue)
                    {
                        errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} must contain a value when Delivery Method is 'Both'");
                        return errors;
                    }
                }


                return errors;
            }
            private List<string> Validate_NATIONAL_DELIVERY(IReaderRow row)
            {
                List<string> errors = new List<string>();
                string fieldName = "NATIONAL_DELIVERY";
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                var isNational = Mandatory_Checks_Bool(row, fieldName);
                if (deliveryMethod == DeliveryMethod.Employer)
                {
                    if (!isNational.HasValue)
                    {
                        errors.Add($"Validation error on row {row.Context.Row}. Field {fieldName} must contain a value when Delivery Method is 'Employer'");
                        return errors;
                    }
                }


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
        private readonly IVenueService _venueService;
        public ApprenticeshipBulkUploadService(
            ILogger<ApprenticeshipBulkUploadService> logger,
            IApprenticeshipService apprenticeshipService,
            IVenueService venueService
            )
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            _logger = logger;
            _apprenticeshipService = apprenticeshipService;
            _venueService = venueService;
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
            Dictionary<string, string> duplicateCheck = new Dictionary<string, string>();
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

                        var classMap = new ApprenticeshipCsvRecordMap(_apprenticeshipService, _venueService);
                        csv.Configuration.RegisterClassMap(classMap);



                        while (csv.Read())
                        {
                            var record = csv.GetRecord<ApprenticeshipCsvRecord>();
                            records.Add(record);
                            if (!duplicateCheck.TryAdd(record.Base64Row, record.RowNumber.ToString()))
                            {
                                var duplicateRow = duplicateCheck[record.Base64Row];
                                throw new BadDataException(csv.Context,
                                    $"Duplicate entries detected on rows {duplicateRow}, and {record.RowNumber}.");
                            }
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
            csv.Configuration.PrepareHeaderForMatch = (string header, int index) => Regex.Replace(header, @"\s", String.Empty);
            // Validate the header.
            csv.Read();
            csv.ReadHeader();
            csv.ValidateHeader<ApprenticeshipCsvRecord>();
        }
        private static string RemoveWhiteSpace(string value)
        {
            return Regex.Replace(value, @"\s+", "");
        }

        private static string Base64Encode(IReaderRow row)
        {
            row.TryGetField<string>("STANDARD_CODE", out string STANDARD_CODE);
            row.TryGetField<string>("STANDARD_VERSION", out string STANDARD_VERSION);
            row.TryGetField<string>("FRAMEWORK_CODE", out string FRAMEWORK_CODE);
            row.TryGetField<string>("FRAMEWORK_PROG_TYPE", out string FRAMEWORK_PROG_TYPE);
            row.TryGetField<string>("FRAMEWORK_PATHWAY_CODE", out string FRAMEWORK_PATHWAY_CODE);
            row.TryGetField<string>("DELIVERY_METHOD", out string DELIVERY_METHOD);
            row.TryGetField<string>("VENUE", out string VENUE);

            string[] line = new string[]
            {
                STANDARD_CODE,
                STANDARD_VERSION,
                FRAMEWORK_CODE,
                FRAMEWORK_PROG_TYPE,
                FRAMEWORK_PATHWAY_CODE,
                DELIVERY_METHOD,
                VENUE
            };
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(String.Join(",", line));
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
