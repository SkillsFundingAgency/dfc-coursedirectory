
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Newtonsoft.Json;
using Dfc.CourseDirectory.Models.Models.Courses;
using System.Net;
using System.Text.RegularExpressions;
using Dfc.CourseDirectory.Common.Settings;
using System.Linq;
using Dfc.CourseDirectory.Models.Enums;
using static Dfc.CourseDirectory.Services.CourseService.CourseValidationResult;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseService : ICourseService
    {
        private readonly ILogger<CourseService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _addCourseUri;
        private readonly Uri _getYourCoursesUri;
        private readonly Uri _updateCourseUri;
        private readonly Uri _getCourseByIdUri;
        private readonly Uri _updateStatusUri;
        private readonly Uri _getCourseCountsByStatusForUKPRNUri;
        private readonly Uri _getRecentCourseChangesByUKPRNUri;
        private readonly Uri _changeCourseRunStatusesForUKPRNSelectionUri;

        private readonly int _courseForTextFieldMaxChars;
        private readonly int _entryRequirementsTextFieldMaxChars;
        private readonly int _whatWillLearnTextFieldMaxChars;
        private readonly int _howYouWillLearnTextFieldMaxChars;
        private readonly int _whatYouNeedTextFieldMaxChars;
        private readonly int _howAssessedTextFieldMaxChars;
        private readonly int _whereNextTextFieldMaxChars;
        private readonly Uri _archiveLiveCoursesUri;


        public CourseService(
            ILogger<CourseService> logger,
            HttpClient httpClient,
            IOptions<CourseServiceSettings> settings,
            IOptions<CourseForComponentSettings> courseForComponentSettings,
            IOptions<EntryRequirementsComponentSettings> entryRequirementsComponentSettings,
            IOptions<WhatWillLearnComponentSettings> whatWillLearnComponentSettings,
            IOptions<HowYouWillLearnComponentSettings> howYouWillLearnComponentSettings,
            IOptions<WhatYouNeedComponentSettings> whatYouNeedComponentSettings,
            IOptions<HowAssessedComponentSettings> howAssessedComponentSettings,
            IOptions<WhereNextComponentSettings> whereNextComponentSettings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));
            Throw.IfNull(courseForComponentSettings, nameof(courseForComponentSettings));
            Throw.IfNull(entryRequirementsComponentSettings, nameof(entryRequirementsComponentSettings));
            Throw.IfNull(whatWillLearnComponentSettings, nameof(whatWillLearnComponentSettings));
            Throw.IfNull(howYouWillLearnComponentSettings, nameof(howYouWillLearnComponentSettings));
            Throw.IfNull(whatYouNeedComponentSettings, nameof(whatYouNeedComponentSettings));
            Throw.IfNull(howAssessedComponentSettings, nameof(howAssessedComponentSettings));
            Throw.IfNull(whereNextComponentSettings, nameof(whereNextComponentSettings));


            _logger = logger;
            _httpClient = httpClient;

            _addCourseUri = settings.Value.ToAddCourseUri();
            _getYourCoursesUri = settings.Value.ToGetYourCoursesUri();
            _updateCourseUri = settings.Value.ToUpdateCourseUri();
            _getCourseByIdUri = settings.Value.ToGetCourseByIdUri();
            _archiveLiveCoursesUri = settings.Value.ToArchiveLiveCoursesUri();
            _updateStatusUri = settings.Value.ToUpdateStatusUri();
            _getCourseCountsByStatusForUKPRNUri = settings.Value.ToGetCourseCountsByStatusForUKPRNUri();
            _getRecentCourseChangesByUKPRNUri = settings.Value.ToGetRecentCourseChangesByUKPRNUri();
            _changeCourseRunStatusesForUKPRNSelectionUri = settings.Value.ToChangeCourseRunStatusesForUKPRNSelectionUri();

            _courseForTextFieldMaxChars = courseForComponentSettings.Value.TextFieldMaxChars;
            _entryRequirementsTextFieldMaxChars = entryRequirementsComponentSettings.Value.TextFieldMaxChars;
            _whatWillLearnTextFieldMaxChars = whatWillLearnComponentSettings.Value.TextFieldMaxChars;
            _howYouWillLearnTextFieldMaxChars = howYouWillLearnComponentSettings.Value.TextFieldMaxChars;
            _whatYouNeedTextFieldMaxChars = whatYouNeedComponentSettings.Value.TextFieldMaxChars;
            _howAssessedTextFieldMaxChars = howAssessedComponentSettings.Value.TextFieldMaxChars;
            _whereNextTextFieldMaxChars = whereNextComponentSettings.Value.TextFieldMaxChars;
        }

        public SelectRegionModel GetRegions()
        {
            return new SelectRegionModel
            {
                LabelText = "Select course region",
                HintText = "For example, South West",
                AriaDescribedBy = "Select all that apply.",
                RegionItems = new RegionItemModel[]
                {
                    new RegionItemModel
                    {
                        Id = "E12000001",
                        Checked = false,
                        RegionName = "North East",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000001", SubRegionName = "County Durham", Checked = false},
                            new SubRegionItemModel{Id="E06000002", SubRegionName = "Darlington", Checked = false},
                            new SubRegionItemModel{Id="E06000003", SubRegionName = "Gateshead", Checked = false},
                            new SubRegionItemModel{Id="E06000004", SubRegionName = "Hartlepool", Checked = false},
                            new SubRegionItemModel{Id="E06000005", SubRegionName = "Middlesbrough", Checked = false},
                            new SubRegionItemModel{Id="E06000047", SubRegionName = "Newcastle upon Tyne", Checked = false},
                            new SubRegionItemModel{Id="E06000057", SubRegionName = "North Tyneside", Checked = false},
                            new SubRegionItemModel{Id="E08000021", SubRegionName = "Northumberland", Checked = false},
                            new SubRegionItemModel{Id="E08000022", SubRegionName = "Redcar and Cleveland", Checked = false},
                            new SubRegionItemModel{Id="E08000023", SubRegionName = "South Tyneside", Checked = false},
                            new SubRegionItemModel{Id="E08000024", SubRegionName = "Stockton-on-Tees", Checked = false},
                            new SubRegionItemModel{Id="E08000037", SubRegionName = "Sunderland", Checked = false},
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000002",
                        Checked = false,
                        RegionName = "North West",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000006", SubRegionName  = "Halton", Checked = false},
                            new SubRegionItemModel{Id="E06000007", SubRegionName  = "Warrington", Checked = false},
                            new SubRegionItemModel{Id="E06000008", SubRegionName  = "Blackburn with Darwen", Checked = false},
                            new SubRegionItemModel{Id="E06000009", SubRegionName  = "Blackpool", Checked = false},
                            new SubRegionItemModel{Id="E06000049", SubRegionName  = "Cheshire East", Checked = false},
                            new SubRegionItemModel{Id="E06000050", SubRegionName  = "Cheshire West and Chester", Checked = false},
                            new SubRegionItemModel{Id="E08000001", SubRegionName  = "Bolton", Checked = false},
                            new SubRegionItemModel{Id="E08000002", SubRegionName  = "Bury", Checked = false},
                            new SubRegionItemModel{Id="E08000003", SubRegionName  = "Manchester", Checked = false},
                            new SubRegionItemModel{Id="E08000004", SubRegionName  = "Oldham", Checked = false},
                            new SubRegionItemModel{Id="E08000005", SubRegionName  = "Rochdale", Checked = false},
                            new SubRegionItemModel{Id="E08000006", SubRegionName  = "Salford", Checked = false},
                            new SubRegionItemModel{Id="E08000007", SubRegionName  = "Stockport", Checked = false},
                            new SubRegionItemModel{Id="E08000008", SubRegionName  = "Tameside", Checked = false},
                            new SubRegionItemModel{Id="E08000009", SubRegionName  = "Trafford", Checked = false},
                            new SubRegionItemModel{Id="E08000010", SubRegionName  = "Wigan", Checked = false},
                            new SubRegionItemModel{Id="E08000011", SubRegionName  = "Knowsley", Checked = false},
                            new SubRegionItemModel{Id="E08000012", SubRegionName  = "Liverpool", Checked = false},
                            new SubRegionItemModel{Id="E08000013", SubRegionName  = "St. Helens", Checked = false},
                            new SubRegionItemModel{Id="E08000014", SubRegionName  = "Sefton", Checked = false},
                            new SubRegionItemModel{Id="E08000015", SubRegionName  = "Wirral", Checked = false},
                            new SubRegionItemModel{Id="E10000006", SubRegionName  = "Cumbria", Checked = false},
                            new SubRegionItemModel{Id="E10000017", SubRegionName  = "Lancashire", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000003",
                        Checked = false,
                        RegionName = "Yorkshire and The Humber",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000010", SubRegionName  = "Kingston upon Hull, City of", Checked = false},
                            new SubRegionItemModel{Id="E06000011", SubRegionName  = "East Riding of Yorkshire", Checked = false},
                            new SubRegionItemModel{Id="E06000012", SubRegionName  = "North East Lincolnshire", Checked = false},
                            new SubRegionItemModel{Id="E06000013", SubRegionName  = "North Lincolnshire", Checked = false},
                            new SubRegionItemModel{Id="E06000014", SubRegionName  = "York", Checked = false},
                            new SubRegionItemModel{Id="E08000016", SubRegionName  = "Barnsley", Checked = false},
                            new SubRegionItemModel{Id="E08000017", SubRegionName  = "Doncaster", Checked = false},
                            new SubRegionItemModel{Id="E08000018", SubRegionName  = "Rotherham", Checked = false},
                            new SubRegionItemModel{Id="E08000019", SubRegionName  = "Sheffield", Checked = false},
                            new SubRegionItemModel{Id="E08000032", SubRegionName  = "Bradford", Checked = false},
                            new SubRegionItemModel{Id="E08000033", SubRegionName  = "Calderdale", Checked = false},
                            new SubRegionItemModel{Id="E08000034", SubRegionName  = "Kirklees", Checked = false},
                            new SubRegionItemModel{Id="E08000035", SubRegionName  = "Leeds", Checked = false},
                            new SubRegionItemModel{Id="E08000036", SubRegionName  = "Wakefield", Checked = false},
                            new SubRegionItemModel{Id="E10000023", SubRegionName  = "North Yorkshire", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000004",
                        Checked = false,
                        RegionName = "East Midlands",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000015", SubRegionName  = "Derby", Checked = false},
                            new SubRegionItemModel{Id="E10000007", SubRegionName  = "Derbyshire", Checked = false},
                            new SubRegionItemModel{Id="E06000016", SubRegionName  = "Leicester", Checked = false},
                            new SubRegionItemModel{Id="E10000018", SubRegionName  = "Leicestershire", Checked = false},
                            new SubRegionItemModel{Id="E10000019", SubRegionName  = "Lincolnshire", Checked = false},
                            new SubRegionItemModel{Id="E10000021", SubRegionName  = "Northamptonshire", Checked = false},
                            new SubRegionItemModel{Id="E06000018", SubRegionName  = "Nottingham", Checked = false},
                            new SubRegionItemModel{Id="E10000024", SubRegionName  = "Nottinghamshire", Checked = false},
                            new SubRegionItemModel{Id="E06000017", SubRegionName  = "Rutland", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000005",
                        Checked = false,
                        RegionName = "West Midlands",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000019", SubRegionName  = "Herefordshire, County of", Checked = false},
                            new SubRegionItemModel{Id="E06000020", SubRegionName  = "Telford and Wrekin", Checked = false},
                            new SubRegionItemModel{Id="E06000021", SubRegionName  = "Stoke-on-Trent", Checked = false},
                            new SubRegionItemModel{Id="E06000051", SubRegionName  = "Shropshire", Checked = false},
                            new SubRegionItemModel{Id="E08000025", SubRegionName  = "Birmingham", Checked = false},
                            new SubRegionItemModel{Id="E08000026", SubRegionName  = "Coventry", Checked = false},
                            new SubRegionItemModel{Id="E08000027", SubRegionName  = "Dudley", Checked = false},
                            new SubRegionItemModel{Id="E08000028", SubRegionName  = "Sandwell", Checked = false},
                            new SubRegionItemModel{Id="E08000029", SubRegionName  = "Solihull", Checked = false},
                            new SubRegionItemModel{Id="E08000030", SubRegionName  = "Walsall", Checked = false},
                            new SubRegionItemModel{Id="E08000031", SubRegionName  = "Wolverhampton", Checked = false},
                            new SubRegionItemModel{Id="E10000028", SubRegionName  = "Staffordshire", Checked = false},
                            new SubRegionItemModel{Id="E10000031", SubRegionName  = "Warwickshire", Checked = false},
                            new SubRegionItemModel{Id="E10000034", SubRegionName  = "Worcestershire", Checked = false},
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000006",
                        Checked = false,
                        RegionName = "East of England",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000055", SubRegionName  = "Bedford", Checked = false},
                            new SubRegionItemModel{Id="E10000003", SubRegionName  = "Cambridgeshire", Checked = false},
                            new SubRegionItemModel{Id="E06000056", SubRegionName  = "Central Bedfordshire", Checked = false},
                            new SubRegionItemModel{Id="E10000012", SubRegionName  = "Essex", Checked = false},
                            new SubRegionItemModel{Id="E10000015", SubRegionName  = "Hertfordshire", Checked = false},
                            new SubRegionItemModel{Id="E06000032", SubRegionName  = "Luton", Checked = false},
                            new SubRegionItemModel{Id="E10000020", SubRegionName  = "Norfolk", Checked = false},
                            new SubRegionItemModel{Id="E06000031", SubRegionName  = "Peterborough", Checked = false},
                            new SubRegionItemModel{Id="E06000033", SubRegionName  = "Southend-on-Sea", Checked = false},
                            new SubRegionItemModel{Id="E10000029", SubRegionName  = "Suffolk", Checked = false},
                            new SubRegionItemModel{Id="E06000034", SubRegionName  = "Thurrock", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000007",
                        Checked = false,
                        RegionName = "London",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E09000001", SubRegionName  = "City of London", Checked = false},
                            new SubRegionItemModel{Id="E09000002", SubRegionName  = "Barking and Dagenham", Checked = false},
                            new SubRegionItemModel{Id="E09000003", SubRegionName  = "Barnet", Checked = false},
                            new SubRegionItemModel{Id="E09000004", SubRegionName  = "Bexley,", Checked = false},
                            new SubRegionItemModel{Id="E09000005", SubRegionName  = "Brent", Checked = false},
                            new SubRegionItemModel{Id="E09000006", SubRegionName  = "Bromley", Checked = false},
                            new SubRegionItemModel{Id="E09000007", SubRegionName  = "Camden", Checked = false},
                            new SubRegionItemModel{Id="E09000008", SubRegionName  = "Croydon", Checked = false},
                            new SubRegionItemModel{Id="E09000009", SubRegionName  = "Ealing", Checked = false},
                            new SubRegionItemModel{Id="E09000010", SubRegionName  = "Enfield", Checked = false},
                            new SubRegionItemModel{Id="E09000011", SubRegionName  = "Greenwich", Checked = false},
                            new SubRegionItemModel{Id="E09000012", SubRegionName  = "Hackney", Checked = false},
                            new SubRegionItemModel{Id="E09000013", SubRegionName  = "Hammersmith and Fulham", Checked = false},
                            new SubRegionItemModel{Id="E09000014", SubRegionName  = "Haringey", Checked = false},
                            new SubRegionItemModel{Id="E09000015", SubRegionName  = "Harrow", Checked = false},
                            new SubRegionItemModel{Id="E09000016", SubRegionName  = "Havering", Checked = false},
                            new SubRegionItemModel{Id="E09000017", SubRegionName  = "Hillingdon", Checked = false},
                            new SubRegionItemModel{Id="E09000018", SubRegionName  = "Hounslow", Checked = false},
                            new SubRegionItemModel{Id="E09000019", SubRegionName  = "Islington", Checked = false},
                            new SubRegionItemModel{Id="E09000020", SubRegionName  = "Kensington and Chelsea", Checked = false},
                            new SubRegionItemModel{Id="E09000021", SubRegionName  = "Kingston upon Thames", Checked = false},
                            new SubRegionItemModel{Id="E09000022", SubRegionName  = "Lambeth", Checked = false},
                            new SubRegionItemModel{Id="E09000023", SubRegionName  = "Lewisham", Checked = false},
                            new SubRegionItemModel{Id="E09000024", SubRegionName  = "Merton", Checked = false},
                            new SubRegionItemModel{Id="E09000025", SubRegionName  = "Newham", Checked = false},
                            new SubRegionItemModel{Id="E09000026", SubRegionName  = "Redbridge", Checked = false},
                            new SubRegionItemModel{Id="E09000027", SubRegionName  = "Richmond upon Thames", Checked = false},
                            new SubRegionItemModel{Id="E09000028", SubRegionName  = "Southwark", Checked = false},
                            new SubRegionItemModel{Id="E09000029", SubRegionName  = "Sutton", Checked = false},
                            new SubRegionItemModel{Id="E09000030", SubRegionName  = "Tower Hamlets", Checked = false},
                            new SubRegionItemModel{Id="E09000031", SubRegionName  = "Waltham Forest", Checked = false},
                            new SubRegionItemModel{Id="E09000032", SubRegionName  = "Wandsworth", Checked = false},
                            new SubRegionItemModel{Id="E09000033", SubRegionName  = "Westminster", Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000008",
                        Checked = false,
                        RegionName = "South East",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000035", SubRegionName  = "Medway", Checked = false},
                            new SubRegionItemModel{Id="E06000036", SubRegionName  = "Bracknell Forest", Checked = false},
                            new SubRegionItemModel{Id="E06000037", SubRegionName  = "West Berkshire", Checked = false},
                            new SubRegionItemModel{Id="E06000038", SubRegionName  = "Reading", Checked = false},
                            new SubRegionItemModel{Id="E06000039", SubRegionName  = "Slough", Checked = false},
                            new SubRegionItemModel{Id="E06000040", SubRegionName  = "Windsor and Maidenhead", Checked = false},
                            new SubRegionItemModel{Id="E06000041", SubRegionName  = "Wokingham", Checked = false},
                            new SubRegionItemModel{Id="E06000042", SubRegionName  = "Milton Keynes", Checked = false},
                            new SubRegionItemModel{Id="E06000043", SubRegionName  = "Brighton and Hove", Checked = false},
                            new SubRegionItemModel{Id="E06000044", SubRegionName  = "Portsmouth", Checked = false},
                            new SubRegionItemModel{Id="E06000045", SubRegionName  = "Southampton", Checked = false},
                            new SubRegionItemModel{Id="E06000046", SubRegionName  = "Isle of Wight", Checked = false},
                            new SubRegionItemModel{Id="E10000002", SubRegionName  = "Buckinghamshire", Checked = false},
                            new SubRegionItemModel{Id="E10000011", SubRegionName  = "East Sussex", Checked = false},
                            new SubRegionItemModel{Id="E10000014", SubRegionName  = "Hampshire", Checked = false},
                            new SubRegionItemModel{Id="E10000016", SubRegionName  = "Kent", Checked = false},
                            new SubRegionItemModel{Id="E10000025", SubRegionName  = "Oxfordshire", Checked = false},
                            new SubRegionItemModel{Id="E10000030", SubRegionName  = "Surrey", Checked = false},
                            new SubRegionItemModel{Id="E10000032", SubRegionName  = "West Sussex",  Checked = false}
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000009",
                        Checked = false,
                        RegionName = "South West",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel{Id="E06000022", SubRegionName  = "Bath and North East Somerset", Checked = false},
                            new SubRegionItemModel{Id="E06000023", SubRegionName  = "Bristol, City of", Checked = false},
                            new SubRegionItemModel{Id="E06000024", SubRegionName  = "North Somerset", Checked = false},
                            new SubRegionItemModel{Id="E06000025", SubRegionName  = "South Gloucestershire", Checked = false},
                            new SubRegionItemModel{Id="E06000026", SubRegionName  = "Plymouth", Checked = false},
                            new SubRegionItemModel{Id="E06000027", SubRegionName  = "Torbay", Checked = false},
                            new SubRegionItemModel{Id="E06000028", SubRegionName  = "Bournemouth", Checked = false},
                            new SubRegionItemModel{Id="E06000029", SubRegionName  = "Poole", Checked = false},
                            new SubRegionItemModel{Id="E06000030", SubRegionName  = "Swindon", Checked = false},
                            new SubRegionItemModel{Id="E06000052", SubRegionName  = "Cornwall", Checked = false},
                            new SubRegionItemModel{Id="E06000053", SubRegionName  = "Isles of Scilly", Checked = false},
                            new SubRegionItemModel{Id="E06000054", SubRegionName  = "Wiltshire", Checked = false},
                            new SubRegionItemModel{Id="E10000008", SubRegionName  = "Devon", Checked = false},
                            new SubRegionItemModel{Id="E10000009", SubRegionName  = "Dorset", Checked = false},
                            new SubRegionItemModel{Id="E10000013", SubRegionName  = "Gloucestershire", Checked = false},
                            new SubRegionItemModel{Id="E10000027", SubRegionName  = "Somerset", Checked = false},
                        }
                    }

                }
            };
        }

        public async Task<IResult<ICourse>> GetCourseByIdAsync(IGetCourseByIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get Course By Id criteria.", criteria);
                _logger.LogInformationObject("Get Course By Id URI", _getCourseByIdUri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");

                var response = await _httpClient.GetAsync(new Uri(_getCourseByIdUri.AbsoluteUri + "&id=" + criteria.Id));

                _logger.LogHttpResponseMessage("Get Course By Id service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get Course By Id service json response", json);


                    var course = JsonConvert.DeserializeObject<Course>(json);


                    return Result.Ok<ICourse>(course);
                }
                else
                {
                    return Result.Fail<ICourse>("Get Course By Id service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Course By Id service http request error", hre);
                return Result.Fail<ICourse>("Get Course By Id service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get Course By Id service unknown error.", e);

                return Result.Fail<ICourse>("Get Course By Id service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }

        }

        public async Task<IResult<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get your courses criteria", criteria);
                _logger.LogInformationObject("Get your courses URI", _getYourCoursesUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<ICourseSearchResult>("Get your courses unknown UKRLP");

                var response = await _httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get your courses service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get your courses service json response", json);
                    IEnumerable<IEnumerable<IEnumerable<Course>>> courses = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<Course>>>>(json);

                    CourseSearchResult searchResult = new CourseSearchResult(courses);
                    return Result.Ok<ICourseSearchResult>(searchResult);

                } else {
                    return Result.Fail<ICourseSearchResult>("Get your courses service unsuccessful http response");
                }

            } catch (HttpRequestException hre) {
                _logger.LogException("Get your courses service http request error", hre);
                return Result.Fail<ICourseSearchResult>("Get your courses service http request error.");

            } catch (Exception e) {
                _logger.LogException("Get your courses service unknown error.", e);
                return Result.Fail<ICourseSearchResult>("Get your courses service unknown error.");

            } finally {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ICourseSearchResult>> GetCoursesByLevelForUKPRNAsync(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get your courses criteria", criteria);
                _logger.LogInformationObject("Get your courses URI", _getYourCoursesUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<ICourseSearchResult>("Get your courses unknown UKRLP");

                var response = await _httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get your courses service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get your courses service json response", json);
                    IEnumerable<IEnumerable<IEnumerable<Course>>> courses = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<Course>>>>(json);
                    var searchResult = new CourseSearchResult(courses);

                    return Result.Ok<ICourseSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<ICourseSearchResult>("Get your courses service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get your courses service http request error", hre);
                return Result.Fail<ICourseSearchResult>("Get your courses service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get your courses service unknown error.", e);
                return Result.Fail<ICourseSearchResult>("Get your courses service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<IEnumerable<ICourseStatusCountResult>>> GetCourseCountsByStatusForUKPRN(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get course counts criteria", criteria);
                _logger.LogInformationObject("Get course counts URI", _getCourseCountsByStatusForUKPRNUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts unknown UKRLP");

                var response = await _httpClient.GetAsync(new Uri(_getCourseCountsByStatusForUKPRNUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get course counts service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get course counts service json response", json);
                    IEnumerable<ICourseStatusCountResult> counts = JsonConvert.DeserializeObject<IEnumerable<CourseStatusCountResult>>(json);

                    //CourseSearchResult searchResult = new CourseSearchResult(courses);
                    return Result.Ok<IEnumerable<ICourseStatusCountResult>>(counts);

                } else {
                    return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service unsuccessful http response");
                }

            } catch (HttpRequestException hre) {
                _logger.LogException("Get course counts service http request error", hre);
                return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service http request error.");

            } catch (Exception e) {
                _logger.LogException("Get course counts service unknown error.", e);
                return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service unknown error.");

            } finally {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<IEnumerable<ICourse>>> GetRecentCourseChangesByUKPRN(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get recent course changes criteria", criteria);
                _logger.LogInformationObject("Get recent course changes URI", _getRecentCourseChangesByUKPRNUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<IEnumerable<ICourse>>("Get recent course changes unknown UKRLP");

                var response = await _httpClient.GetAsync(new Uri(_getRecentCourseChangesByUKPRNUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get recent course changes service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get recent course changes service json response", json);
                    IEnumerable<ICourse> courses = JsonConvert.DeserializeObject<IEnumerable<Course>>(json);

                    return Result.Ok<IEnumerable<ICourse>>(courses);

                } else {
                    return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service unsuccessful http response");
                }

            } catch (HttpRequestException hre) {
                _logger.LogException("Get recent course changes service http request error", hre);
                return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service http request error.");

            } catch (Exception e) {
                _logger.LogException("Get recent course changes service unknown error.", e);
                return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service unknown error.");

            } finally {
                _logger.LogMethodExit();
            }
        }

        public IResult<IList<CourseValidationResult>> PendingCourseValidationMessages(IEnumerable<ICourse> courses)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(courses, nameof(courses));

            try {
                IList<CourseValidationResult> results = new List<CourseValidationResult>();

                foreach (ICourse c in courses) {
                    CourseValidationResult cvr = new CourseValidationResult() {
                        Course = c,
                        Issues = ValidateCourse(c),
                        RunValidationResults = new List<CourseRunValidationResult>()
                    };
                    foreach (ICourseRun r in c.CourseRuns)
                        cvr.RunValidationResults.Add(new CourseRunValidationResult() { Run = r, Issues = ValidateCourseRun(r, ValidationMode.BulkUploadCourse) });
                    results.Add(cvr);
                }
                return Result.Ok(results);

            } catch (Exception ex) {
                _logger.LogException("PendingCourseValidationMessages error", ex);
                return Result.Fail<IList<CourseValidationResult>>("Error compiling messages for items requiring attention on landing page");

            } finally {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ICourse>> AddCourseAsync(ICourse course)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(course, nameof(course));

            try
            {
                _logger.LogInformationObject("Course add object.", course);
                _logger.LogInformationObject("Course add URI", _addCourseUri);

                var courseJson = JsonConvert.SerializeObject(course);

                var content = new StringContent(courseJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_addCourseUri, content);

                _logger.LogHttpResponseMessage("Course add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Course add service json response", json);


                    var courseResult = JsonConvert.DeserializeObject<Course>(json);


                    return Result.Ok<ICourse>(courseResult);
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return Result.Fail<ICourse>("Course add service unsuccessful http response - TooManyRequests");
                }
                else
                {
                    return Result.Fail<ICourse>("Course add service unsuccessful http response - ResponseStatusCode: " + response.StatusCode);
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Course add service http request error", hre);
                return Result.Fail<ICourse>("Course add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Course add service unknown error.", e);

                return Result.Fail<ICourse>("Course add service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }


        public async Task<IResult<ICourse>> UpdateCourseAsync(ICourse course)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(course, nameof(course));

            try
            {
                _logger.LogInformationObject("Course update object.", course);
                _logger.LogInformationObject("Course update URI", _updateCourseUri);

                var courseJson = JsonConvert.SerializeObject(course);

                var content = new StringContent(courseJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_updateCourseUri, content);

                _logger.LogHttpResponseMessage("Course update service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Course update service json response", json);


                    var courseResult = JsonConvert.DeserializeObject<Course>(json);


                    return Result.Ok<ICourse>(courseResult);
                }
                else
                {
                    return Result.Fail<ICourse>("Course update service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Course update service http request error", hre);
                return Result.Fail<ICourse>("Course update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Course update service unknown error.", e);

                return Result.Fail<ICourse>("Course update service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public IList<string> ValidateCourse(ICourse course)
        {
            var validationMessages = new List<string>();

            // CourseDescription
            if (string.IsNullOrEmpty(course.CourseDescription))
            {
                validationMessages.Add("Course For decription is required");
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(course.CourseDescription))
                    validationMessages.Add("Course For decription contains invalid character");
                if (course.CourseDescription.Length > _courseForTextFieldMaxChars)
                    validationMessages.Add($"Course For decription must be { _courseForTextFieldMaxChars } characters or less");
            }

            // EntryRequirements
            if (!string.IsNullOrEmpty(course.EntryRequirements))
            {
                if (!HasOnlyFollowingValidCharacters(course.EntryRequirements))
                    validationMessages.Add("Entry Requirements contains invalid character");
                if (course.EntryRequirements.Length > _entryRequirementsTextFieldMaxChars)
                    validationMessages.Add($"Entry Requirements must be { _entryRequirementsTextFieldMaxChars } characters or less");
            }

            // WhatYoullLearn 
            if (!string.IsNullOrEmpty(course.WhatYoullLearn))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhatYoullLearn))
                    validationMessages.Add("What You'll Learn contains invalid character");
                if (course.WhatYoullLearn.Length > _whatWillLearnTextFieldMaxChars)
                    validationMessages.Add($"What You'll Learn must be { _whatWillLearnTextFieldMaxChars } characters or less");
            }

            // HowYoullLearn 
            if (!string.IsNullOrEmpty(course.HowYoullLearn))
            {
                if (!HasOnlyFollowingValidCharacters(course.HowYoullLearn))
                    validationMessages.Add("How You'll Learn contains invalid character");
                if (course.HowYoullLearn.Length > _howYouWillLearnTextFieldMaxChars)
                    validationMessages.Add($"How You'll Learn must be { _howYouWillLearnTextFieldMaxChars } characters or less");
            }

            // WhatYoullNeed 
            if (!string.IsNullOrEmpty(course.WhatYoullNeed))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhatYoullNeed))
                    validationMessages.Add("What You'll Need contains invalid character");
                if (course.WhatYoullNeed.Length > _whatYouNeedTextFieldMaxChars)
                    validationMessages.Add($"What You'll Need must be { _whatYouNeedTextFieldMaxChars } characters or less");
            }

            // HowYoullBeAssessed 
            if (!string.IsNullOrEmpty(course.HowYoullBeAssessed))
            {
                if (!HasOnlyFollowingValidCharacters(course.HowYoullBeAssessed))
                    validationMessages.Add("How You'll Be Assessed contains invalid character");
                if (course.HowYoullBeAssessed.Length > _howAssessedTextFieldMaxChars)
                    validationMessages.Add($"How You'll Be Assessed must be { _howAssessedTextFieldMaxChars } characters or less");
            }

            // WhereNext 
            if (!string.IsNullOrEmpty(course.WhereNext))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhereNext))
                    validationMessages.Add("Where Next contains invalid character");
                if (course.WhereNext.Length > _whereNextTextFieldMaxChars)
                    validationMessages.Add($"Where Next must be { _whereNextTextFieldMaxChars } characters or less");
            }

            return validationMessages;
        }

        public IList<string> ValidateCourseRun(ICourseRun courseRun, ValidationMode validationMode)
        {
            var validationMessages = new List<string>();

            // CourseName
            if (string.IsNullOrEmpty(courseRun.CourseName))
            {
                validationMessages.Add("Course Name is required"); // "Enter Course Name"
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.CourseName))
                    validationMessages.Add("Course Name contains invalid character");
                if (courseRun.CourseName.Length > 255)
                    validationMessages.Add($"Course Name must be 255 characters or less");
            }

            // ProviderCourseID
            if (!string.IsNullOrEmpty(courseRun.ProviderCourseID))
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.ProviderCourseID))
                    validationMessages.Add("ID contains invalid characters");
                if (courseRun.ProviderCourseID.Length > 255)
                    validationMessages.Add($"The maximum length of 'ID' is 255 characters");
            }

            // DeliveryMode
            switch (courseRun.DeliveryMode)
            {
                case DeliveryMode.ClassroomBased:

                    // VenueId
                    if (courseRun.VenueId == null || courseRun.VenueId == Guid.Empty)
                        validationMessages.Add($"Select a venue");

                    // StudyMode
                    if (courseRun.StudyMode.Equals(StudyMode.Undefined))
                        validationMessages.Add($"Select Study Mode");

                    // AttendancePattern
                    if (courseRun.AttendancePattern.Equals(AttendancePattern.Undefined))
                        validationMessages.Add($"Select Attendance Pattern");

                    break;
                case DeliveryMode.Online:
                    // No Specific Fields
                    break;
                case DeliveryMode.WorkBased:

                    // Regions
                    if (courseRun.Regions == null || courseRun.Regions.Count().Equals(0))
                        validationMessages.Add($"Select a region");
                    break;
                case DeliveryMode.Undefined: // Question ???
                default:
                    validationMessages.Add($"DeliveryMode is Undefined. We are not checking the specific fields now. On editing you can select the appropriate Delivery Mode and the rest of the fields will be validated accordingly.");
                    break;
            }

            // StartDate & FlexibleStartDate
            if (courseRun.StartDate != null)
            {
                switch (validationMode)
                {
                    case ValidationMode.AddCourseRun:
                    case ValidationMode.CopyCourseRun:
                        if (courseRun.StartDate < DateTime.Now || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date cannot be before Today's Date and must be less than or equal to 2 years from Today's Date");
                        break;
                    case ValidationMode.EditCourseYC:
                    case ValidationMode.EditCourseMT:
                        // It cannot be done easily as we need both value - the newly entered and the previous. Call to saved version or modification in the model
                        break;
                    case ValidationMode.EditCourseBU:
                        // If the Provider does the editing on the same day of uploading it's fine. But from next day forward ?????????
                        if (courseRun.StartDate < DateTime.Now || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date cannot be before Today's Date and must be less than or equal to 2 years from Today's Date");
                        break;
                    case ValidationMode.BulkUploadCourse:
                        if (courseRun.StartDate < DateTime.Now || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date cannot be before Today's Date and must be less than or equal to 2 years from Today's Date");
                        break;
                    case ValidationMode.MigrateCourse:
                        if (courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date must be less than or equal to 2 years from Today's Date");
                        break;
                    case ValidationMode.Undefined: 
                    default:
                        validationMessages.Add($"Validation Mode was not defined.");
                        break;
                }
            }

            if (courseRun.StartDate == null && courseRun.FlexibleStartDate == false)
                validationMessages.Add($"Either 'Defined Start Date' or 'Flexible Start Date' has to be provided");

            // CourseURL
            if (!string.IsNullOrEmpty(courseRun.CourseURL))
            {
                if (!IsValidUrl(courseRun.CourseURL))
                    validationMessages.Add("The format of URL is incorrect");
                if (courseRun.CourseURL.Length > 255)
                    validationMessages.Add($"The maximum length of URL is 255 characters");
            }

            // Cost & CostDescription
            if (string.IsNullOrEmpty(courseRun.CostDescription) && courseRun.Cost.Equals(null))
                validationMessages.Add($"Enter cost or cost description");

            if (!string.IsNullOrEmpty(courseRun.CostDescription))
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.CostDescription))
                    validationMessages.Add("Cost Description contains invalid characters");
                if (courseRun.CostDescription.Length > 255)
                    validationMessages.Add($"Cost description must be 255 characters or less");
            }

            if (!courseRun.Cost.Equals(null))
            {
                if (!IsCorrectCostFormatting(courseRun.Cost.ToString()))
                    validationMessages.Add($"Enter the cost in pounds and pence");
                if(courseRun.Cost > decimal.Parse("999999.99"))
                    validationMessages.Add($"Maximum allowed cost value is 999,999.99");
            }

            // DurationUnit
            if (courseRun.DurationUnit.Equals(DurationUnit.Undefined))
                validationMessages.Add($"Select Duration Unit");

            // DurationValue
            if (courseRun.DurationValue.Equals(null))
            {
                validationMessages.Add($"Enter Duration");
            }
            else
            {
                if (!ValidDurationValue(courseRun.DurationValue?.ToString()))
                    validationMessages.Add("Duration must be numeric and maximum length is 3 digits");
            }

            return validationMessages;
        }

        public bool HasOnlyFollowingValidCharacters(string value)
        {
            string regex = @"^[a-zA-Z0-9 /\n/\r/\¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`" + "\"" + "\\\\]+$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsValidUrl(string value)
        {
            string regex = @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsCorrectCostFormatting(string value)
        {
            string regex = @"^[0-9]*(\.[0-9]{1,2})?$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool ValidDurationValue(string value)
        {
            string regex = @"^([0-9]|[0-9][0-9]|[0-9][0-9][0-9])$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public async Task<IResult> ArchiveProviderLiveCourses(int? UKPRN)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));

            var response = await _httpClient.GetAsync(new Uri(_archiveLiveCoursesUri.AbsoluteUri + "&UKPRN=" + UKPRN));
            _logger.LogHttpResponseMessage("Archive courses service http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();

            }
            else
            {
                return Result.Fail("Archive courses service unsuccessful http response");
            }
        }

        public async Task<IResult> ChangeCourseRunStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));
            Throw.IfNull(CurrentStatus, nameof(CurrentStatus));
            Throw.IfNull(StatusToBeChangedTo, nameof(StatusToBeChangedTo));

            var response = await _httpClient.GetAsync(new Uri(_changeCourseRunStatusesForUKPRNSelectionUri.AbsoluteUri + "&UKPRN=" + UKPRN + "&CurrentStatus=" + CurrentStatus + "&StatusToBeChangedTo=" + StatusToBeChangedTo));
            _logger.LogHttpResponseMessage("Archive courses service http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("ChangeCourseRunStatusesForUKPRNSelection service unsuccessful http response");
            }
        }

        public async Task<IResult> UpdateStatus(Guid courseId, Guid courseRunId, int statusToUpdateTo)
        {
            Throw.IfNullGuid(courseId, nameof(courseId));
            Throw.IfLessThan(0, statusToUpdateTo, nameof(statusToUpdateTo));
            Throw.IfGreaterThan(Enum.GetValues(typeof(RecordStatus)).Cast<int>().Max(), statusToUpdateTo, nameof(statusToUpdateTo));

            var response = await _httpClient.GetAsync(new Uri(_updateStatusUri.AbsoluteUri 
                + "&CourseId=" + courseId
                + "&CourseRunId=" + courseRunId
                + "&Status=" + statusToUpdateTo));
            _logger.LogHttpResponseMessage("Update Status http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();

            }
            else
            {
                return Result.Fail("Update course unsuccessful http response");
            }
        }
    }

    internal static class IGetCourseByIdCriteriaExtensions
    {
        internal static string ToJson(this IGetCourseByIdCriteria extendee)
        {

            GetCourseByIdJson json = new GetCourseByIdJson
            {
                id = extendee.Id.ToString()
            };
            var result = JsonConvert.SerializeObject(json);

            return result;
        }
    }

    internal class GetCourseByIdJson
    {
        public string id { get; set; }
    }

    internal static class CourseServiceSettingsExtensions
    {
        internal static Uri ToAddCourseUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "AddCourse?code=" + extendee.ApiKey}");
        }
        internal static Uri ToGetYourCoursesUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCoursesByLevelForUKPRN?code=" + extendee.ApiKey}");
        }
        internal static Uri ToUpdateCourseUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "UpdateCourse?code=" + extendee.ApiKey}");
        }
        internal static Uri ToGetCourseByIdUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCourseById?code=" + extendee.ApiKey}");
        }
        internal static Uri ToArchiveLiveCoursesUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "ArchiveProvidersLiveCourses?code=" + extendee.ApiKey}");
        }
        internal static Uri ToUpdateStatusUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "UpdateStatus?code=" + extendee.ApiKey}");
        }
        internal static Uri ToGetCourseCountsByStatusForUKPRNUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCourseCountsByStatusForUKPRN?code=" + extendee.ApiKey}");
        }
        internal static Uri ToGetRecentCourseChangesByUKPRNUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetRecentCourseChangesByUKPRN?code=" + extendee.ApiKey}");
        }
        internal static Uri ToChangeCourseRunStatusesForUKPRNSelectionUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "ChangeCourseRunStatusesForUKPRNSelection?code=" + extendee.ApiKey}");
        }
    }
}
