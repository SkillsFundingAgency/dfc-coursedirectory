using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Helpers;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Interfaces.Auth;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.WebV2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.BulkUploadService
{
    public class ApprenticeshipBulkUploadService : IApprenticeshipBulkUploadService
    {
        internal enum DeliveryMethod
        {
            Undefined = 0,
            Classroom = 1,
            Employer = 2,
            Both = 3
        }

        internal enum DeliveryMode
        {
            Undefined = 0,
            Employer = 1,
            Day = 2,
            Block = 3
        }

        internal class ApprenticeshipCsvRecord
        {
            public ApprenticeshipCsvRecord()
            {
                this.ApprenticeshipLocations = new List<ApprenticeshipLocation>();
            }

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
            public List<Venue> VENUE { get; set; }
            public int? RADIUS { get; set; }
            public List<int> DELIVERY_MODE { get; set; }
            public bool? ACROSS_ENGLAND { get; set; }
            public bool? NATIONAL_DELIVERY { get; set; }
            public string REGION { get; set; }
            public string SUB_REGION { get; set; }

            [Ignore]
            public ApprenticeshipType ApprenticeshipType { get; set; }

            [Ignore]
            public List<BulkUploadError> ErrorsList { get; set; }

            [Ignore]
            public List<string> RegionsList { get; set; }

            [Ignore]
            public IStandardsAndFrameworks Standard { get; set; }

            [Ignore]
            public IStandardsAndFrameworks Framework { get; set; }

            public List<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }

            [Ignore]
            public int RowNumber { get; set; }

            [Ignore]
            public string Base64Row { get; set; }
        }

        private class ApprenticeshipCsvRecordMap : ClassMap<ApprenticeshipCsvRecord>
        {
            private readonly IApprenticeshipService _apprenticeshipService;
            private readonly IVenueService _venueService;
            private readonly IAuthUserDetails _authUserDetails;
            private readonly IStandardsAndFrameworksCache _standardsAndFrameworksCache;
            private List<Venue> _cachedVenues;

            public ApprenticeshipCsvRecordMap(IApprenticeshipService apprenticeshipService,
                IVenueService venueService,
                IAuthUserDetails userDetails,
                IStandardsAndFrameworksCache standardsAndFrameworksCache)
            {
                Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
                Throw.IfNull(venueService, nameof(venueService));
                Throw.IfNull(userDetails, nameof(userDetails));
                _apprenticeshipService = apprenticeshipService;
                _venueService = venueService;
                _authUserDetails = userDetails;
                _standardsAndFrameworksCache = standardsAndFrameworksCache;
                Map(m => m.STANDARD_CODE).ConvertUsing(Mandatory_Checks_STANDARD_CODE);
                Map(m => m.STANDARD_VERSION).ConvertUsing(Mandatory_Checks_STANDARD_VERSION);
                Map(m => m.Standard).ConvertUsing(Mandatory_Checks_GetStandard);
                Map(m => m.FRAMEWORK_CODE).ConvertUsing(Mandatory_Checks_FRAMEWORK_CODE);
                Map(m => m.FRAMEWORK_PROG_TYPE).ConvertUsing(Mandatory_Checks_FRAMEWORK_PROG_TYPE);
                Map(m => m.FRAMEWORK_PATHWAY_CODE).ConvertUsing(Mandatory_Checks_FRAMEWORK_PATHWAY_CODE);
                Map(m => m.Framework).ConvertUsing(Mandatory_Checks_GetFramework);
                Map(m => m.APPRENTICESHIP_INFORMATION);
                Map(m => m.APPRENTICESHIP_WEBPAGE);
                Map(m => m.CONTACT_EMAIL);
                Map(m => m.CONTACT_PHONE);
                Map(m => m.CONTACT_URL);
                Map(m => m.DELIVERY_METHOD).ConvertUsing(Mandatory_Checks_DELIVERY_METHOD);
                Map(m => m.VENUE).ConvertUsing(Mandatory_Checks_VENUE); ;
                Map(m => m.RADIUS).ConvertUsing(Mandatory_Checks_RADIUS);
                Map(m => m.DELIVERY_MODE).ConvertUsing(Mandatory_Checks_DELIVERY_MODE);
                Map(m => m.ACROSS_ENGLAND).ConvertUsing(Mandatory_Checks_ACROSS_ENGLAND);
                Map(m => m.NATIONAL_DELIVERY).ConvertUsing(Mandatory_Checks_NATIONAL_DELIVERY);
                Map(m => m.REGION);
                Map(m => m.SUB_REGION);
                Map(m => m.ApprenticeshipType).ConvertUsing((row) =>
                {
                    row.TryGetField("STANDARD_CODE", out string standardCode);
                    if (!string.IsNullOrWhiteSpace(standardCode))
                    {
                        return ApprenticeshipType.StandardCode;
                    }

                    return ApprenticeshipType.FrameworkCode;
                });
                Map(m => m.RegionsList).ConvertUsing(GetRegionList);
                Map(m => m.ErrorsList).ConvertUsing(ValidateData);
                Map(m => m.RowNumber).ConvertUsing(row => row.Context.RawRow);
                Map(m => m.Base64Row).ConvertUsing(Base64Encode);
            }

            #region Mandatory Checks

            private IStandardsAndFrameworks Mandatory_Checks_GetStandard(IReaderRow row)
            {
                var standardCode = Mandatory_Checks_STANDARD_CODE(row);
                var standardVersion = Mandatory_Checks_STANDARD_VERSION(row);

                if (standardCode.HasValue)
                {
                    if (standardVersion.HasValue)
                    {
                        var result = GetStandard(standardCode.Value, standardVersion.Value).GetAwaiter().GetResult();
                        if (result == null)
                        {
                            throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Invalid Standard Code or Version Number. Standard not found.");
                        }

                        return result;
                    }
                    else
                    {
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Missing Standard Version.");
                    }
                }
                if (!standardCode.HasValue && standardVersion.HasValue)
                {
                    throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Missing Standard Code.");
                }

                return null;
            }

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
                }
                return value;
            }

            private IStandardsAndFrameworks Mandatory_Checks_GetFramework(IReaderRow row)
            {
                var frameworkCode = Mandatory_Checks_FRAMEWORK_CODE(row);
                var progType = Mandatory_Checks_FRAMEWORK_PROG_TYPE(row);
                var pathwayCode = Mandatory_Checks_FRAMEWORK_PATHWAY_CODE(row);
                if (frameworkCode.HasValue)
                {
                    if (progType.HasValue)
                    {
                        if (pathwayCode.HasValue)
                        {
                            var result = GetFramework(frameworkCode.Value, progType.Value, pathwayCode.Value).GetAwaiter().GetResult();
                            if (result == null)
                            {
                                throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Invalid Framework Code, Prog Type, or Pathway Code. Framework not found.");
                            }

                            return result;
                        }
                        else
                        {
                            throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Missing Pathway Type.");
                        }
                    }
                    else
                    {
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Missing Prog Type.");
                    }
                }

                if (!frameworkCode.HasValue && (pathwayCode.HasValue || progType.HasValue))
                {
                    throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Missing Framework Code.");
                }

                return null;
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
                }
                return value;
            }

            private DeliveryMethod Mandatory_Checks_DELIVERY_METHOD(IReaderRow row)
            {
                string fieldName = "DELIVERY_METHOD";
                row.TryGetField<string>(fieldName, out string value);

                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. DELIVERY_METHOD is required.");
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

            private List<int> Mandatory_Checks_DELIVERY_MODE(IReaderRow row)
            {
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);

                if (deliveryMethod == DeliveryMethod.Classroom || deliveryMethod == DeliveryMethod.Both)
                {
                    string fieldName = "DELIVERY_MODE";
                    row.TryGetField<string>(fieldName, out string value);

                    var modes = value.Split(";");
                    Dictionary<DeliveryMode, int> deliveryModes = new Dictionary<DeliveryMode, int>();
                    foreach (var mode in modes)
                    {
                        var deliveryMode = mode.ToEnum(DeliveryMode.Undefined);
                        if (deliveryMode == DeliveryMode.Undefined)
                        {
                            return new List<int>();
                        }

                        if (!deliveryModes.TryAdd(deliveryMode, (int)deliveryMode))
                        {
                            return new List<int>();
                        }
                    }

                    return deliveryModes.Values.ToList();
                }

                return new List<int>();
            }

            private bool? Mandatory_Checks_ACROSS_ENGLAND(IReaderRow row)
            {
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);

                if (deliveryMethod == DeliveryMethod.Both)
                {
                    string fieldName = "ACROSS_ENGLAND";
                    row.TryGetField<string>(fieldName, out string value);
                    return Mandatory_Checks_Bool(row, fieldName);
                }

                return null;
            }

            private bool? Mandatory_Checks_NATIONAL_DELIVERY(IReaderRow row)
            {
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);

                if (deliveryMethod == DeliveryMethod.Employer)
                {
                    string fieldName = "NATIONAL_DELIVERY";
                    row.TryGetField<string>(fieldName, out string value);
                    return Mandatory_Checks_Bool(row, fieldName);
                }

                return null;
            }

            private List<string> GetRegionList(IReaderRow row)
            {
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                var isNational = Mandatory_Checks_NATIONAL_DELIVERY(row);

                List<string> regions = new List<string>();
                if (deliveryMethod == DeliveryMethod.Employer)
                {
                    string regionFieldName = "REGION";
                    string subRegionFieldName = "SUB_REGION";
                    row.TryGetField(regionFieldName, out string regionList);
                    row.TryGetField(subRegionFieldName, out string subRegionList);

                    if (isNational == null)
                        return new List<string>();

                    if (string.IsNullOrEmpty(regionList) && string.IsNullOrEmpty(subRegionList))
                    {
                        return new List<string>();
                    }

                    if (isNational == false)
                    {
                        var regionCodes = ParseRegionData(regionList);
                        var subregionCodes = ParseSubRegionData(subRegionList);
                        regions.AddRange(regionCodes.Distinct());

                        foreach (var regionCode in regionCodes)
                        {
                            subregionCodes = RemoveSubregionsOfExistingRegions(subregionCodes, regionCode);
                        }
                        regions.AddRange(subregionCodes);
                    }
                }

                return new List<string>(regions.Distinct());
            }

            private bool? Mandatory_Checks_Bool(IReaderRow row, string fieldName)
            {
                row.TryGetField<string>(fieldName, out string value);

                switch (value.ToUpper())
                {
                    case "YES":
                        return true;

                    case "NO":
                        return false;
                }

                return null;
            }

            private List<Venue> Mandatory_Checks_VENUE(IReaderRow row)
            {
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                if (deliveryMethod == DeliveryMethod.Classroom || deliveryMethod == DeliveryMethod.Both)
                {
                    if (_cachedVenues == null)
                    {
                        _cachedVenues = Task.Run(async () => await _venueService.SearchAsync(new VenueSearchCriteria(_authUserDetails.UKPRN, string.Empty)))
                            .Result
                            .Value
                            .Value
                            .ToList();
                        if (!_cachedVenues.Any())
                        {
                            return null;
                        }
                    }
                    string fieldName = "VENUE";
                    row.TryGetField<string>(fieldName, out string value);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return null;
                    }

                    var venues = _cachedVenues
                        .Where(x => x.VenueName.ToUpper() == value.Trim().ToUpper() && x.Status == VenueStatus.Live).ToList();

                    if (venues.Any())
                    {
                        return venues;
                    }
                }

                return null;
            }

            #endregion Mandatory Checks

            #region Field Validation

            private List<BulkUploadError> ValidateData(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                errors.AddRange(Validate_APPRENTICESHIP_INFORMATION(row));
                errors.AddRange(Validate_APPRENTICESHIP_WEBPAGE(row));
                errors.AddRange(Validate_CONTACT_EMAIL(row));
                errors.AddRange(Validate_CONTACT_PHONE(row));
                errors.AddRange(Validate_CONTACT_URL(row));
                errors.AddRange(Validate_DELIVERY_METHOD(row));

                switch (Mandatory_Checks_DELIVERY_METHOD(row))
                {
                    case DeliveryMethod.Classroom:
                        {
                            errors.AddRange(Validate_VENUE(row));
                            errors.AddRange(Validate_DELIVERY_MODE(row));
                            break;
                        }
                    case DeliveryMethod.Employer:
                        {
                            errors.AddRange(Validate_NATIONAL_DELIVERY(row));
                            errors.AddRange(Validate_REGION(row));
                            errors.AddRange(Validate_SUB_REGION(row));
                            break;
                        }
                    case DeliveryMethod.Both:
                        {
                            errors.AddRange(Validate_VENUE(row));
                            errors.AddRange(Validate_RADIUS(row));
                            errors.AddRange(Validate_DELIVERY_MODE(row));
                            errors.AddRange(Validate_ACROSS_ENGLAND(row));
                            break;
                        }
                }
                return errors;
            }

            private List<BulkUploadError> Validate_APPRENTICESHIP_INFORMATION(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "APPRENTICESHIP_INFORMATION";
                row.TryGetField<string>(fieldName, out string value);

                if (String.IsNullOrWhiteSpace(value))
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error = $"Validation error on row {row.Context.Row}. Field {fieldName} is required."
                    });
                }
                if (!String.IsNullOrEmpty(value) && value.Length > 750)
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error = $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 750 characters."
                    });
                }
                return errors;
            }

            private List<BulkUploadError> Validate_APPRENTICESHIP_WEBPAGE(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "APPRENTICESHIP_WEBPAGE";
                row.TryGetField<string>(fieldName, out string value);
                if (!String.IsNullOrWhiteSpace(value))
                {
                    value = HTTPCheck(value).Trim();
                    var regex = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
                    if (!Regex.IsMatch(value, regex))
                    {
                        errors.Add(new BulkUploadError
                        {
                            LineNumber = row.Context.Row,
                            Header = fieldName,
                            Error =
                                $"Validation error on row {row.Context.Row}. Field {fieldName} format of URL is incorrect."
                        });
                    }

                    if (value.Length > 255)
                    {
                        errors.Add(new BulkUploadError
                        {
                            LineNumber = row.Context.Row,
                            Header = fieldName,
                            Error =
                                $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 255 characters."
                        });
                    }
                }

                return errors;
            }

            private List<BulkUploadError> Validate_CONTACT_EMAIL(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "CONTACT_EMAIL";
                row.TryGetField<string>(fieldName, out string value);

                if (String.IsNullOrWhiteSpace(value))
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error =
                            $"Validation error on row {row.Context.Row}. Field {fieldName} is required."
                    });
                    return errors;
                }
                if (!String.IsNullOrEmpty(value) && value.Length > 255)
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error =
                            $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 255 characters."
                    });
                    return errors;
                }

                var emailRegEx = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                if (!Regex.IsMatch(value, emailRegEx))
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error =
                            $"Validation error on row {row.Context.Row}. Field {fieldName} needs a valid email."
                    });
                }
                return errors;
            }

            private List<BulkUploadError> Validate_CONTACT_PHONE(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();

                string fieldName = "CONTACT_PHONE";
                row.TryGetField<string>(fieldName, out string value);
                value = RemoveWhiteSpace(value);

                if (String.IsNullOrWhiteSpace(value))
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error = $"Validation error on row {row.Context.Row}. Field {fieldName} is required."
                    });
                    return errors;
                }
                if (!String.IsNullOrEmpty(value) && value.Length > 30)
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error = $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 30 characters."
                    });
                    return errors;
                }
                if (!long.TryParse(value, out long numericalValue))
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error = $"Validation error on row {row.Context.Row}. Field {fieldName} must be numeric if present."
                    });
                    return errors;
                }
                return errors;
            }

            private List<BulkUploadError> Validate_CONTACT_URL(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "CONTACT_URL";
                row.TryGetField(fieldName, out string value);
                if (String.IsNullOrEmpty(value))
                {
                    return errors;
                }
                if (value.Length > 255)
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error = $"Validation error on row {row.Context.Row}. Field {fieldName} maximum length is 255 characters."
                    });
                }

                value = HTTPCheck(value).Trim();
                var urlRegex = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
                if (!Regex.IsMatch(value, urlRegex))
                {
                    errors.Add(new BulkUploadError
                    {
                        LineNumber = row.Context.Row,
                        Header = fieldName,
                        Error = $"Validation error on row {row.Context.Row}. Field {fieldName} format of URL is incorrect."
                    });
                }
                return errors;
            }

            private List<BulkUploadError> Validate_DELIVERY_METHOD(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "DELIVERY_METHOD";
                row.TryGetField<string>(fieldName, out string value);

                if (String.IsNullOrWhiteSpace(value))
                {                  

                    throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} is required.");
                }

                var deliveryMethod = value.ToEnum(DeliveryMethod.Undefined);
                if (deliveryMethod == DeliveryMethod.Undefined)
                {
                    throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} is invalid.");
                }
                return errors;
            }

            private List<BulkUploadError> Validate_VENUE(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();

                string fieldName = "VENUE";
                row.TryGetField(fieldName, out string value);
                if (String.IsNullOrWhiteSpace(value))
                {                   
                   throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Venue is missing.");                  
                   
                }

                var venues = Mandatory_Checks_VENUE(row);

                if (venues == null)
                {
                    throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} is invalid. Provide a Valid {fieldName}.");
                    
                }

                if (venues.Count > 1)
                {                   
                    throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} is invalid. Multiple venues identified with value entered.");
                }
                return errors;
            }

            private List<BulkUploadError> Validate_RADIUS(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();

                string fieldName = "RADIUS";
                var value = Mandatory_Checks_RADIUS(row);
                if (value.HasValue)
                {
                    if (value <= 0)
                    {
                       
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} must be a valid number.");
                    }

                    if (value > 874)
                    {                     
                       
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} must be between 1 and 874");
                       
                    }
                }
                if (Mandatory_Checks_DELIVERY_METHOD(row) == DeliveryMethod.Both)
                {
                    var acrossEngland = Mandatory_Checks_ACROSS_ENGLAND(row);
                    if (acrossEngland == false)
                    {
                        if (!value.HasValue)
                        {
                          
                            throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} must have a value if not ACROSS_ENGLAND.");
                        }
                    }
                }
                return errors;
            }

            private List<BulkUploadError> Validate_DELIVERY_MODE(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();

                string fieldName = "DELIVERY_MODE";
                row.TryGetField(fieldName, out string value);

                Dictionary<DeliveryMode, string> modes = new Dictionary<DeliveryMode, string>();

                var modeArray = value.Split(";");
                if (Mandatory_Checks_DELIVERY_METHOD(row) == DeliveryMethod.Both)
                {
                    if (modeArray.Length == 0)
                    {
                      
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} contain delivery modes if Delivery Method is BOTH");
                    }
                }
                foreach (var mode in modeArray)
                {
                    var deliveryMode = mode.ToEnum(DeliveryMode.Undefined);
                    if (deliveryMode == DeliveryMode.Undefined)
                    {                        

                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} must be a valid Delivery Mode");
                    }

                    if (!modes.TryAdd(deliveryMode, deliveryMode.ToString()))
                    {                      

                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} must contain unique Delivery Modes");
                    }
                }
                return errors;
            }

            private List<BulkUploadError> Validate_ACROSS_ENGLAND(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "ACROSS_ENGLAND";

                if (Mandatory_Checks_DELIVERY_METHOD(row) == DeliveryMethod.Both)
                {
                    var isAcrossEngland = Mandatory_Checks_Bool(row, fieldName);
                    if (!isAcrossEngland.HasValue)
                    {                        
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} must contain a value when Delivery Method is 'Both");
                        
                    }
                }

                return errors;
            }

            private List<BulkUploadError> Validate_NATIONAL_DELIVERY(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "NATIONAL_DELIVERY";
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                var isNational = Mandatory_Checks_Bool(row, fieldName);
                if (deliveryMethod == DeliveryMethod.Employer)
                {
                    if (!isNational.HasValue)
                    {
                      
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName} must contain a value when Delivery Method is 'Employer'");
                    }
                }

                return errors;
            }

            private List<BulkUploadError> Validate_REGION(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "REGION";
                row.TryGetField(fieldName, out string value);
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                var isNational = Mandatory_Checks_NATIONAL_DELIVERY(row);
                if (deliveryMethod == DeliveryMethod.Employer)
                {
                    if (isNational == false)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            var results = ParseRegionData(value);
                            if (!results.Any())
                            {                              
                                throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName}  contains invalid Region names");
                            }
                        }
                    }
                }

                return errors;
            }

            private List<BulkUploadError> Validate_SUB_REGION(IReaderRow row)
            {
                List<BulkUploadError> errors = new List<BulkUploadError>();
                string fieldName = "SUB_REGION";
                row.TryGetField(fieldName, out string value);
                var deliveryMethod = Mandatory_Checks_DELIVERY_METHOD(row);
                var isNational = Mandatory_Checks_NATIONAL_DELIVERY(row);
                if (deliveryMethod == DeliveryMethod.Employer)
                {
                    if (isNational == false)
                    {
                        var results = ParseSubRegionData(value);
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (!results.Any())
                            {
                               
                                throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Field {fieldName}  contains invalid SubRegion names");
                            }
                        }

                        var anyRegions = GetRegionList(row);
                        if (!anyRegions.Any() && !results.Any())
                        {
                          
                            throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Fields REGION/SUB_REGION are mandatory");
                        }
                    }
                }

                return errors;
            }

            #endregion Field Validation

            private string HTTPCheck(string value)
            {
                value = value.ToLower();
                if (value.Contains("http://") || value.Contains("https://"))
                {
                    return value;
                }

                return "https://" + value;
            }

            private int? ValueMustBeNumericIfPresent(IReaderRow row, string fieldName)
            {
                if (!row.TryGetField<int?>(fieldName, out var value))
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

                if (STANDARD_CODE.HasValue || STANDARD_VERSION.HasValue)
                {
                    if (FRAMEWORK_CODE.HasValue || FRAMEWORK_PROG_TYPE.HasValue || FRAMEWORK_PATHWAY_CODE.HasValue)
                    {
                        throw new BadDataException(row.Context, $"Validation error on row {row.Context.Row}. Values for Both Standard AND Framework cannot be present in the same row.");
                    }
                }
            }

            private async Task<IStandardsAndFrameworks> GetStandard(int standardCode, int version)
            {
                var standard = await _standardsAndFrameworksCache.GetStandard(standardCode, version);

                if (standard != null)
                {
                    return new StandardsAndFrameworks()
                    {
                        id = standard.CosmosId,
                        StandardCode = standard.StandardCode,
                        Version = standard.Version,
                        StandardName = standard.StandardName,
                        NotionalEndLevel = standard.NotionalNVQLevelv2,
                        OtherBodyApprovalRequired = standard.OtherBodyApprovalRequired ? "Y" : "N"
                    };
                }
                else
                {
                    return null;
                }
            }

            private async Task<IStandardsAndFrameworks> GetFramework(int frameworkCode, int progType, int pathwayCode)
            {
                var framework = await _standardsAndFrameworksCache.GetFramework(
                        frameworkCode,
                        progType,
                        pathwayCode);

                if (framework != null)
                {
                    return new StandardsAndFrameworks()
                    {
                        id = framework.CosmosId,
                        FrameworkCode = framework.FrameworkCode,
                        ProgType = framework.ProgType,
                        PathwayCode = framework.PathwayCode,
                        NasTitle = framework.NasTitle
                    };
                }
                else
                {
                    return null;
                }
            }

            private IEnumerable<string> ParseRegionData(string regions)
            {
                var availableRegions = new SelectRegionModel();
                var regionCodes = new List<string>();

                if (!string.IsNullOrEmpty(regions))
                {
                    foreach (var region in regions.Trim().Split(";"))

                    {
                        var regionCode =
                            availableRegions.RegionItems.Where(x =>
                                string.Equals(x.RegionName, region, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Id);

                        if (!regionCode.Any())
                        {
                            return new List<string>();
                        }

                        regionCodes.AddRange(regionCode);
                    }
                }

                return regionCodes.Distinct();
            }

            private IEnumerable<string> ParseSubRegionData(string subRegionList)
            {
                var availableSubRegions = new SelectRegionModel().RegionItems.SelectMany(x => x.SubRegion);
                var subRegionCodeList = new List<string>();

                if (!string.IsNullOrEmpty(subRegionList))
                {
                    foreach (var subRegion in subRegionList.Trim().Split(";"))

                    {
                        var subRegions =
                            availableSubRegions.Where(x => string.Equals(x.SubRegionName, subRegion,
                                StringComparison.InvariantCultureIgnoreCase));

                        if (!subRegions.Any())
                        {
                            return new List<string>();
                        }

                        var listOfSubRegionCodes = subRegions.Select(x => x.Id).ToList();
                        if (listOfSubRegionCodes.Any())
                        {
                            subRegionCodeList.AddRange(listOfSubRegionCodes);
                        }
                    }
                }

                return subRegionCodeList.Distinct();
            }

            private List<string> RemoveSubregionsOfExistingRegions(IEnumerable<string> subregionCodes, string regionCode)
            {
                var totalList = new List<string>();
                totalList.AddRange(subregionCodes);

                var selectRegionModel = new SelectRegionModel();

                var subregionsForRegion =
                    selectRegionModel.RegionItems.Where(x =>
                            string.Equals(x.Id, regionCode, StringComparison.CurrentCultureIgnoreCase))
                        .Select(y => y.SubRegion).FirstOrDefault();
                var subregionIds = subregionsForRegion.Select(x => x.Id);

                var matchingSubregionIds = subregionIds.Intersect(subregionCodes);

                foreach (var item in matchingSubregionIds)
                {
                    totalList.Remove(item);
                }

                return totalList;
            }
        }

        private readonly ILogger<ApprenticeshipBulkUploadService> _logger;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IVenueService _venueService;
        private readonly IStandardsAndFrameworksCache _standardsAndFrameworksCache;

        public ApprenticeshipBulkUploadService(
            ILogger<ApprenticeshipBulkUploadService> logger,
            IApprenticeshipService apprenticeshipService,
            IVenueService venueService,
            IStandardsAndFrameworksCache standardsAndFrameworksCache)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            _logger = logger;
            _apprenticeshipService = apprenticeshipService;
            _venueService = venueService;
            _standardsAndFrameworksCache = standardsAndFrameworksCache;
        }

        public int CountCsvLines(Stream stream)
        {
            Throw.IfNull(stream, nameof(stream));

            int count = 0;
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream, true);  // don't dispose the stream we might need it later.
            while (sr.ReadLine() != null)
            {
                ++count;
            }
            return count;
        }

        public async Task<List<string>> ValidateAndUploadCSV(Stream stream, AuthUserDetails userDetails, bool updateApprenticeships)
        {
            Throw.IfNull(stream, nameof(stream));
            Throw.IfNull(userDetails, nameof(userDetails));
            List<string> errors = new List<string>();
            List<ApprenticeshipCsvRecord> records = new List<ApprenticeshipCsvRecord>();
            Dictionary<string, string> duplicateCheck = new Dictionary<string, string>();
            int processedRowCount = 0;
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    if (0 == stream.Length) throw new Exception("File is empty.");
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        // Validate the header row.
                        ValidateHeader(csv);

                        var classMap = new ApprenticeshipCsvRecordMap(
                            _apprenticeshipService,
                            _venueService,
                            userDetails,
                            _standardsAndFrameworksCache);

                        csv.Configuration.RegisterClassMap(classMap);
                        bool containsDuplicates = false;
                        while (csv.Read())
                        {
                            var record = csv.GetRecord<ApprenticeshipCsvRecord>();

                            if (!duplicateCheck.TryAdd(record.Base64Row, record.RowNumber.ToString()))
                            {
                                if (containsDuplicates == false)
                                {
                                    containsDuplicates = true;
                                    errors = new List<string>();
                                }

                                var duplicateRow = duplicateCheck[record.Base64Row];
                                errors.Add(
                                    $"Duplicate entries detected on rows {duplicateRow}, and {record.RowNumber}.");
                            }

                            if (containsDuplicates == false)
                            {
                                record.ApprenticeshipLocations.Add(CreateApprenticeshipLocation(record, userDetails));
                                errors.AddRange(record.ErrorsList.Select(x => x.Error));
                            }

                            records.Add(record);
                            processedRowCount++;
                        }

                        if (containsDuplicates)
                        {
                            throw new BadDataException(csv.Context, string.Join(";", errors));
                        }
                    }

                    if (0 == processedRowCount)
                    {
                        throw new Exception("The selected file is empty");
                    }
                }

                var archivingApprenticeships = await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(int.Parse(userDetails.UKPRN), (int)RecordStatus.Live, (int)RecordStatus.Archived);
                if (updateApprenticeships)
                {
                    var apprenticeships = ApprenticeshipCsvRecordToApprenticeship(records, userDetails);
                    errors = ValidateApprenticeships(apprenticeships);


                    if (apprenticeships.Any())
                    {
                        errors.AddRange(await UploadApprenticeships(apprenticeships));
                    }
                    else
                    {
                        throw new Exception($"Unable to archive apprenticeships for {int.Parse(userDetails.UKPRN)}");
                    }
                }
            }
            catch (HeaderValidationException ex)
            {
                string errmsg = $"Invalid header row. {ex.Message.FirstSentence()}";

                errors.Add(errmsg);
                throw;
            }
            catch (FieldValidationException ex)
            {
                errors.Add($"{ex.Message}");
                throw;
            }
            catch (BadDataException ex)
            {
                errors.Add($"{ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                errors.Add($"{ex.Message}");
                throw;
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

        private async Task<List<string>> UploadApprenticeships(List<Apprenticeship> apprenticeships)
        {
            List<string> errors = new List<string>();
            foreach (var apprenticeship in apprenticeships)
            {
                var result = await _apprenticeshipService.AddApprenticeship(apprenticeship);

                if (result.IsFailure)
                {
                    throw new Exception($"Unable to add Apprenticeship {apprenticeship.ApprenticeshipTitle}");
                }
            }

            return errors;
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

        private List<Apprenticeship> ApprenticeshipCsvRecordToApprenticeship(
            List<ApprenticeshipCsvRecord> records, AuthUserDetails userDetails)
        {
            List<Apprenticeship> apprenticeships = new List<Apprenticeship>();

            foreach (var record in records)
            {
                            
                var alreadyExists = DoesApprenticeshipExist(apprenticeships, record);
            
                if (alreadyExists != null)
                {
                    var apprenticeship = apprenticeships.FirstOrDefault(x => x == alreadyExists);
                    apprenticeship?.ApprenticeshipLocations.AddRange(record.ApprenticeshipLocations);
                    apprenticeship?.BulkUploadErrors.AddRange(record.ErrorsList);
                }
                else
                {
                    apprenticeships.Add(
                        new Apprenticeship
                        {
                            id = Guid.NewGuid(),
                            ApprenticeshipTitle = record.Framework == null
                                ? record.Standard.StandardName
                                : record.Framework.NasTitle,
                            ProviderId = userDetails.ProviderID ?? Guid.Empty,
                            ProviderUKPRN = int.Parse(userDetails.UKPRN),
                            ApprenticeshipLocations = record.ApprenticeshipLocations,
                            ApprenticeshipType = record.ApprenticeshipType,
                            FrameworkId = record.Framework?.id,
                            StandardId = record.Standard?.id,
                            FrameworkCode = record.Framework?.FrameworkCode,
                            ProgType = record.Framework?.ProgType,
                            PathwayCode = record.Framework?.PathwayCode,
                            StandardCode = record.Standard?.StandardCode,
                            Version = record.Standard?.Version,
                            NotionalNVQLevelv2 = record.Framework == null
                                ? record.Standard.NotionalEndLevel
                                : record.Framework.NotionalEndLevel,
                            MarketingInformation = record.APPRENTICESHIP_INFORMATION,
                            Url = record.APPRENTICESHIP_WEBPAGE,
                            ContactTelephone = record.CONTACT_PHONE,
                            ContactEmail = record.CONTACT_EMAIL,
                            ContactWebsite = record.CONTACT_URL,                            
                            RecordStatus = record.ErrorsList.Any()? RecordStatus.BulkUploadPending : RecordStatus.BulkUploadReadyToGoLive,
                            CreatedDate = DateTime.Now,
                            CreatedBy = userDetails.UserId.ToString(),
                            BulkUploadErrors = record.ErrorsList
                        });
                }
            }
            return apprenticeships;
        }

        private Apprenticeship DoesApprenticeshipExist(List<Apprenticeship> apprenticeships, ApprenticeshipCsvRecord record)
        {
            if (record.ApprenticeshipType == ApprenticeshipType.StandardCode)
            {
                var existingApprenticeships = apprenticeships.Where(x =>
                    x.StandardCode == record.STANDARD_CODE && x.Version == record.STANDARD_VERSION);
                return existingApprenticeships.FirstOrDefault(x => x.ApprenticeshipLocations.Any(y => y.ApprenticeshipLocationType == (ApprenticeshipLocationType)record.DELIVERY_METHOD));
            }
            else
            {
                var existingApprenticeships = apprenticeships.Where(x =>
                    x.FrameworkCode == record.FRAMEWORK_CODE &&
                    x.ProgType == record.FRAMEWORK_PROG_TYPE &&
                    x.PathwayCode == record.FRAMEWORK_PATHWAY_CODE);

                return existingApprenticeships.FirstOrDefault(x => x.ApprenticeshipLocations.Any(y => y.ApprenticeshipLocationType == (ApprenticeshipLocationType)record.DELIVERY_METHOD));
            }
        }

        private ApprenticeshipLocation CreateApprenticeshipLocation(ApprenticeshipCsvRecord record, AuthUserDetails authUserDetails)
        {
            Venue venue = null;
            if (record.VENUE?.Count == 1)
            {
                venue = record.VENUE.FirstOrDefault();
            }
            return new ApprenticeshipLocation
            {
                Id = Guid.NewGuid(),
                Name = venue?.VenueName,
                CreatedDate = DateTime.Now,
                CreatedBy = authUserDetails.Email,
                ApprenticeshipLocationType = (ApprenticeshipLocationType)record.DELIVERY_METHOD,
                LocationType = LocationType.Venue,
                RecordStatus = record.ErrorsList.Any() ? RecordStatus.BulkUploadPending : RecordStatus.BulkUploadReadyToGoLive,
                Regions = record.RegionsList.ToArray(),
                National = NationalOrAcrossEngland(record.NATIONAL_DELIVERY, record.ACROSS_ENGLAND),
                TribalId = venue?.TribalLocationId,
                ProviderId = venue?.ProviderID,
                LocationId = venue?.LocationId,
                VenueId = Guid.TryParse(venue?.ID, out var venueId) ? venueId : Guid.Empty,
                Address = venue != null
                    ? new Address
                    {
                        Address1 = venue.Address1,
                        Address2 = venue.Address2,
                        Town = venue.Town,
                        County = venue.County,
                        Postcode = venue.PostCode,
                        Email = venue.Email,
                        Phone = venue.Telephone,
                        Website = venue.Website,
                        Latitude = (double)venue.Latitude,
                        Longitude = (double)venue.Longitude
                    }
                    : null,
                LocationGuidId = Guid.TryParse(venue?.ID, out var locationGuid) ? locationGuid : Guid.Empty,
                Radius = record.RADIUS,
                DeliveryModes = record.DELIVERY_MODE
            };
        }

        internal bool? NationalOrAcrossEngland(bool? national, bool? acrossEngland)
        {
            if (acrossEngland.HasValue)
                return acrossEngland;

            if (national.HasValue)
                return national;
            return null;
        }

         
        public List<string> ValidateApprenticeships(List<Apprenticeship> apprenticeships)
        {
            List<string> errors = new List<string>();
            var errorList = apprenticeships.Select(x => x.BulkUploadErrors);
           

            foreach (var apprentice in apprenticeships)
            {
                if (string.IsNullOrEmpty(apprentice.MarketingInformation))
                {
                    errors.Add("Marketing Information is required");
                }
                if (string.IsNullOrEmpty(apprentice.ContactEmail))
                {
                    errors.Add("Email is required");
                }
                if (string.IsNullOrEmpty(apprentice.ContactTelephone))
                {
                    errors.Add("Telephone is required");
                }

               
            }

            return errors;
        }

    }
}