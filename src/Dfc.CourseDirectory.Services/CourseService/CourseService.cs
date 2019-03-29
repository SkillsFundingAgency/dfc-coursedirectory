
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
                            new SubRegionItemModel
                            {
                                Id = "E06000001",
                                SubRegionName = "County Durham",
                                Checked = false,
                                Latitude = 54.77869,
                                Longitude = -1.55961
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000002",
                                SubRegionName = "Darlington",
                                Checked = false,
                                Latitude = 54.52873,
                                Longitude = -1.55305
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000003",
                                SubRegionName = "Gateshead",
                                Checked = false,
                                Latitude = 54.95937,
                                Longitude = -1.60182
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000004",
                                SubRegionName = "Hartlepool",
                                Checked = false,
                                Latitude = 54.68249,
                                Longitude = -1.2167
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000005",
                                SubRegionName = "Middlesbrough",
                                Checked = false,
                                Latitude = 54.57301,
                                Longitude = -1.23791
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000047",
                                SubRegionName = "Newcastle upon Tyne",
                                Checked = false,
                                Latitude = 54.97784,
                                Longitude = -1.61292
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000057",
                                SubRegionName = "North Tyneside",
                                Checked = false,
                                Latitude = 50.78061,
                                Longitude = 3.12184
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000021",
                                SubRegionName = "Northumberland",
                                Checked = false,
                                Latitude = 40.88925,
                                Longitude = -76.79354
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000022",
                                SubRegionName = "Redcar and Cleveland",
                                Checked = false,
                                Latitude = 54.60301,
                                Longitude = -1.07763
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000023",
                                SubRegionName = "South Tyneside",
                                Checked = false,
                                Latitude = 51.27034,
                                Longitude = 7.20263
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000024",
                                SubRegionName = "Stockton-on-Tees",
                                Checked = false,
                                Latitude = 54.56823,
                                Longitude = -1.31443
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000037",
                                SubRegionName = "Sunderland",
                                Checked = false,
                                Latitude = 54.90445,
                                Longitude = -1.38145
                            }
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000002",
                        Checked = false,
                        RegionName = "North West",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel
                            {
                                Id = "E06000006",
                                SubRegionName = "Halton",
                                Checked = false,
                                Latitude = -32.31548,
                                Longitude = 151.5143
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000007",
                                SubRegionName = "Warrington",
                                Checked = false,
                                Latitude = 53.39266,
                                Longitude = -2.587
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000008",
                                SubRegionName = "Blackburn with Darwen",
                                Checked = false,
                                Latitude = 53.7501,
                                Longitude = -2.48471
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000009",
                                SubRegionName = "Blackpool",
                                Checked = false,
                                Latitude = 53.81418,
                                Longitude = -3.05354
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000049",
                                SubRegionName = "Cheshire East",
                                Checked = false,
                                Latitude = 41.45423,
                                Longitude = -72.81231
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000050",
                                SubRegionName = "Cheshire West and Chester",
                                Checked = false,
                                Latitude = 53.21744,
                                Longitude = -2.74297
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000001",
                                SubRegionName = "Bolton",
                                Checked = false,
                                Latitude = 53.57846,
                                Longitude = -2.42984
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000002",
                                SubRegionName = "Bury",
                                Checked = false,
                                Latitude = 53.59346,
                                Longitude = -2.29854
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000003",
                                SubRegionName = "Manchester",
                                Checked = false,
                                Latitude = 53.48071,
                                Longitude = -2.23438
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000004",
                                SubRegionName = "Oldham",
                                Checked = false,
                                Latitude = 53.54125,
                                Longitude = -2.11766
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000005",
                                SubRegionName = "Rochdale",
                                Checked = false,
                                Latitude = 53.61635,
                                Longitude = -2.15871
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000006",
                                SubRegionName = "Salford",
                                Checked = false,
                                Latitude = 42.99748,
                                Longitude = -80.82748
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000007",
                                SubRegionName = "Stockport",
                                Checked = false,
                                Latitude = 53.40849,
                                Longitude = -2.14929
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000008",
                                SubRegionName = "Tameside",
                                Checked = false,
                                Latitude = 54.47409,
                                Longitude = -1.19072
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000009",
                                SubRegionName = "Trafford",
                                Checked = false,
                                Latitude = 33.82052,
                                Longitude = -86.74535
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000010",
                                SubRegionName = "Wigan",
                                Checked = false,
                                Latitude = 53.54427,
                                Longitude = -2.63106
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000011",
                                SubRegionName = "Knowsley",
                                Checked = false,
                                Latitude = -36.82613,
                                Longitude = 144.5873
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000012",
                                SubRegionName = "Liverpool",
                                Checked = false,
                                Latitude = 53.41078,
                                Longitude = -2.97784
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000013",
                                SubRegionName = "St. Helens",
                                Checked = false,
                                Latitude = 53.45388,
                                Longitude = -2.73689
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000014",
                                SubRegionName = "Sefton",
                                Checked = false,
                                Latitude = -43.2486,
                                Longitude = 172.669
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000015",
                                SubRegionName = "Wirral",
                                Checked = false,
                                Latitude = 53.39199,
                                Longitude = -3.17886
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000006",
                                SubRegionName = "Cumbria",
                                Checked = false,
                                Latitude = 19.64903,
                                Longitude = -99.21791
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000017",
                                SubRegionName = "Lancashire",
                                Checked = false,
                                Latitude = 53.54125,
                                Longitude = -2.11766
                            }
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000003",
                        Checked = false,
                        RegionName = "Yorkshire and The Humber",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel
                            {
                                Id = "E06000010",
                                SubRegionName = "Kingston upon Hull, City of",
                                Checked = false,
                                Latitude = 53.74434,
                                Longitude = -0.33244
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000011",
                                SubRegionName = "East Riding of Yorkshire",
                                Checked = false,
                                Latitude = 53.84292,
                                Longitude = -0.42766
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000012",
                                SubRegionName = "North East Lincolnshire",
                                Checked = false,
                                Latitude = 42.22473,
                                Longitude = -87.84481
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000013",
                                SubRegionName = "North Lincolnshire",
                                Checked = false,
                                Latitude = 40.57776,
                                Longitude = -85.68432
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000014",
                                SubRegionName = "York",
                                Checked = false,
                                Latitude = 40.71305,
                                Longitude = -74.00723
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000016",
                                SubRegionName = "Barnsley",
                                Checked = false,
                                Latitude = 53.55293,
                                Longitude = -1.48127
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000017",
                                SubRegionName = "Doncaster",
                                Checked = false,
                                Latitude = 53.52304,
                                Longitude = -1.13376
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000018",
                                SubRegionName = "Rotherham",
                                Checked = false,
                                Latitude = 53.4302,
                                Longitude = -1.35685
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000019",
                                SubRegionName = "Sheffield",
                                Checked = false,
                                Latitude = 53.38306,
                                Longitude = -1.46479
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000032",
                                SubRegionName = "Bradford",
                                Checked = false,
                                Latitude = 53.79385,
                                Longitude = -1.75244
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000033",
                                SubRegionName = "Calderdale",
                                Checked = false,
                                Latitude = 54.56555,
                                Longitude = -0.97638
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000034",
                                SubRegionName = "Kirklees",
                                Checked = false,
                                Latitude = 52.60206,
                                Longitude = 1.28036
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000035",
                                SubRegionName = "Leeds",
                                Checked = false,
                                Latitude = 53.79969,
                                Longitude = -1.5491
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000036",
                                SubRegionName = "Wakefield",
                                Checked = false,
                                Latitude = 53.68297,
                                Longitude = -1.4991
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000023",
                                SubRegionName = "North Yorkshire",
                                Checked = false,
                                Latitude = 53.95774,
                                Longitude = -1.08226
                            }
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000004",
                        Checked = false,
                        RegionName = "East Midlands",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel
                            {
                                Id = "E06000015",
                                SubRegionName = "Derby",
                                Checked = false,
                                Latitude = 52.9219,
                                Longitude = -1.47564
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000007",
                                SubRegionName = "Derbyshire",
                                Checked = false,
                                Latitude = 52.9219,
                                Longitude = -1.47564
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000016",
                                SubRegionName = "Leicester",
                                Checked = false,
                                Latitude = 52.63486,
                                Longitude = -1.12906
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000018",
                                SubRegionName = "Leicestershire",
                                Checked = false,
                                Latitude = 52.63486,
                                Longitude = -1.12906
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000019",
                                SubRegionName = "Lincolnshire",
                                Checked = false,
                                Latitude = 42.19589,
                                Longitude = -87.90625
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000021",
                                SubRegionName = "Northamptonshire",
                                Checked = false,
                                Latitude = 52.23484,
                                Longitude = -0.89732
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000018",
                                SubRegionName = "Nottingham",
                                Checked = false,
                                Latitude = 52.95512,
                                Longitude = -1.14917
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000024",
                                SubRegionName = "Nottinghamshire",
                                Checked = false,
                                Latitude = 52.95512,
                                Longitude = -1.14917
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000017",
                                SubRegionName = "Rutland",
                                Checked = false,
                                Latitude = 43.61063,
                                Longitude = -72.97269
                            }
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000005",
                        Checked = false,
                        RegionName = "West Midlands",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel
                            {
                                Id = "E06000019",
                                SubRegionName = "Herefordshire, County of",
                                Checked = false,
                                Latitude = 38.77813,
                                Longitude = -90.19985
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000020",
                                SubRegionName = "Telford and Wrekin",
                                Checked = false,
                                Latitude = 52.67677,
                                Longitude = -2.53548
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000021",
                                SubRegionName = "Stoke-on-Trent",
                                Checked = false,
                                Latitude = 53.02578,
                                Longitude = -2.17739
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000051",
                                SubRegionName = "Shropshire",
                                Checked = false,
                                Latitude = 52.67587,
                                Longitude = -2.4497
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000025",
                                SubRegionName = "Birmingham",
                                Checked = false,
                                Latitude = 52.4829,
                                Longitude = -1.89346
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000026",
                                SubRegionName = "Coventry",
                                Checked = false,
                                Latitude = 52.40631,
                                Longitude = -1.50852
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000027",
                                SubRegionName = "Dudley",
                                Checked = false,
                                Latitude = 52.50867,
                                Longitude = -2.08734
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000028",
                                SubRegionName = "Sandwell",
                                Checked = false,
                                Latitude = 52.50636,
                                Longitude = -1.96258
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000029",
                                SubRegionName = "Solihull",
                                Checked = false,
                                Latitude = 52.41471,
                                Longitude = -1.7743
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000030",
                                SubRegionName = "Walsall",
                                Checked = false,
                                Latitude = 52.58595,
                                Longitude = -1.98229
                            },
                            new SubRegionItemModel
                            {
                                Id = "E08000031",
                                SubRegionName = "Wolverhampton",
                                Checked = false,
                                Latitude = 52.58533,
                                Longitude = -2.13192
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000028",
                                SubRegionName = "Staffordshire",
                                Checked = false,
                                Latitude = -37.74601,
                                Longitude = 143.7076
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000031",
                                SubRegionName = "Warwickshire",
                                Checked = false,
                                Latitude = 52.28194,
                                Longitude = -1.58447
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000034",
                                SubRegionName = "Worcestershire",
                                Checked = false,
                                Latitude = 52.19204,
                                Longitude = -2.22353
                            },
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000006",
                        Checked = false,
                        RegionName = "East of England",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel
                            {
                                Id = "E06000055",
                                SubRegionName = "Bedford",
                                Checked = false,
                                Latitude = 52.13571,
                                Longitude = -0.46804
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000003",
                                SubRegionName = "Cambridgeshire",
                                Checked = false,
                                Latitude = 52.57339,
                                Longitude = -0.24846
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000056",
                                SubRegionName = "Central Bedfordshire",
                                Checked = false,
                                Latitude = 52.00268,
                                Longitude = -0.29749
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000012",
                                SubRegionName = "Essex",
                                Checked = false,
                                Latitude = 42.17247,
                                Longitude = -82.81928
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000015",
                                SubRegionName = "Hertfordshire",
                                Checked = false,
                                Latitude = 41.30676,
                                Longitude = -72.93177
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000032",
                                SubRegionName = "Luton",
                                Checked = false,
                                Latitude = 51.87965,
                                Longitude = -0.41756
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000020",
                                SubRegionName = "Norfolk",
                                Checked = false,
                                Latitude = -29.04393,
                                Longitude = 167.9727
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000031",
                                SubRegionName = "Peterborough",
                                Checked = false,
                                Latitude = 52.57339,
                                Longitude = -0.24846
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000033",
                                SubRegionName = "Southend-on-Sea",
                                Checked = false,
                                Latitude = 51.54041,
                                Longitude = 0.71176
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000029",
                                SubRegionName = "Suffolk",
                                Checked = false,
                                Latitude = 36.72819,
                                Longitude = -76.58319
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000034",
                                SubRegionName = "Thurrock",
                                Checked = false,
                                Latitude = 51.48201,
                                Longitude = 0.28067
                            }
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000007",
                        Checked = false,
                        RegionName = "London",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel
                            {
                                Id = "E09000001",
                                SubRegionName = "City of London",
                                Checked = false,
                                Latitude = 51.51333,
                                Longitude = -0.08895
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000002",
                                SubRegionName = "Barking and Dagenham",
                                Checked = false,
                                Latitude = 51.53628,
                                Longitude = 0.08148
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000003",
                                SubRegionName = "Barnet",
                                Checked = false,
                                Latitude = 51.65293,
                                Longitude = -0.19961
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000004",
                                SubRegionName = "Bexley,",
                                Checked = false,
                                Latitude = 51.44135,
                                Longitude = 0.14861
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000005",
                                SubRegionName = "Brent",
                                Checked = false,
                                Latitude = 30.46826,
                                Longitude = -87.23904
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000006",
                                SubRegionName = "Bromley",
                                Checked = false,
                                Latitude = 51.40568,
                                Longitude = 0.01435
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000007",
                                SubRegionName = "Camden",
                                Checked = false,
                                Latitude = 39.94521,
                                Longitude = -75.11883
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000008",
                                SubRegionName = "Croydon",
                                Checked = false,
                                Latitude = 51.37236,
                                Longitude = -0.1004
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000009",
                                SubRegionName = "Ealing",
                                Checked = false,
                                Latitude = -44.0452,
                                Longitude = 171.419
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000010",
                                SubRegionName = "Enfield",
                                Checked = false,
                                Latitude = 51.6521,
                                Longitude = -0.08153
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000011",
                                SubRegionName = "Greenwich",
                                Checked = false,
                                Latitude = 41.02653,
                                Longitude = -73.62855
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000012",
                                SubRegionName = "Hackney",
                                Checked = false,
                                Latitude = -34.91217,
                                Longitude = 138.6135
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000013",
                                SubRegionName = "Hammersmith and Fulham",
                                Checked = false,
                                Latitude = 51.47736,
                                Longitude = -0.20167
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000014",
                                SubRegionName = "Haringey",
                                Checked = false,
                                Latitude = 48.91493,
                                Longitude = 2.53423
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000015",
                                SubRegionName = "Harrow",
                                Checked = false,
                                Latitude = 51.57881,
                                Longitude = -0.33376
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000016",
                                SubRegionName = "Havering",
                                Checked = false,
                                Latitude = 51.61583,
                                Longitude = 0.18344
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000017",
                                SubRegionName = "Hillingdon",
                                Checked = false,
                                Latitude = 51.53358,
                                Longitude = -0.45258
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000018",
                                SubRegionName = "Hounslow",
                                Checked = false,
                                Latitude = 51.46759,
                                Longitude = -0.3618
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000019",
                                SubRegionName = "Islington",
                                Checked = false,
                                Latitude = -41.50998,
                                Longitude = 173.9643
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000020",
                                SubRegionName = "Kensington and Chelsea",
                                Checked = false,
                                Latitude = 40.69744,
                                Longitude = -73.97944
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000021",
                                SubRegionName = "Kingston upon Thames",
                                Checked = false,
                                Latitude = 51.41232,
                                Longitude = -0.30044
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000022",
                                SubRegionName = "Lambeth",
                                Checked = false,
                                Latitude = 42.90688,
                                Longitude = -81.29608
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000023",
                                SubRegionName = "Lewisham",
                                Checked = false,
                                Latitude = -42.83335,
                                Longitude = 147.6102
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000024",
                                SubRegionName = "Merton",
                                Checked = false,
                                Latitude = 43.14135,
                                Longitude = -88.31182
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000025",
                                SubRegionName = "Newham",
                                Checked = false,
                                Latitude = -37.31047,
                                Longitude = 144.5907
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000026",
                                SubRegionName = "Redbridge",
                                Checked = false,
                                Latitude = 46.40087,
                                Longitude = -79.18479
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000027",
                                SubRegionName = "Richmond upon Thames",
                                Checked = false,
                                Latitude = 51.41232,
                                Longitude = -0.30044
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000028",
                                SubRegionName = "Southwark",
                                Checked = false,
                                Latitude = 51.50172,
                                Longitude = -0.09796
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000029",
                                SubRegionName = "Sutton",
                                Checked = false,
                                Latitude = 51.36045,
                                Longitude = -0.19178
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000030",
                                SubRegionName = "Tower Hamlets",
                                Checked = false,
                                Latitude = 51.12929,
                                Longitude = 1.30422
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000031",
                                SubRegionName = "Waltham Forest",
                                Checked = false,
                                Latitude = -34.77444,
                                Longitude = 138.7221
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000032",
                                SubRegionName = "Wandsworth",
                                Checked = false,
                                Latitude = -30.06261,
                                Longitude = 151.5185
                            },
                            new SubRegionItemModel
                            {
                                Id = "E09000033",
                                SubRegionName = "Westminster",
                                Checked = false,
                                Latitude = 34.66488,
                                Longitude = -83.0967
                            }
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000008",
                        Checked = false,
                        RegionName = "South East",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel
                            {
                                Id = "E06000035",
                                SubRegionName = "Medway",
                                Checked = false,
                                Latitude = 42.14028,
                                Longitude = -71.39804
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000036",
                                SubRegionName = "Bracknell Forest",
                                Checked = false,
                                Latitude = 28.80676,
                                Longitude = -81.70188
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000037",
                                SubRegionName = "West Berkshire",
                                Checked = false,
                                Latitude = 44.98942,
                                Longitude = -72.81304
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000038",
                                SubRegionName = "Reading",
                                Checked = false,
                                Latitude = 51.45504,
                                Longitude = -0.96909
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000039",
                                SubRegionName = "Slough",
                                Checked = false,
                                Latitude = 51.50935,
                                Longitude = -0.59545
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000040",
                                SubRegionName = "Windsor and Maidenhead",
                                Checked = false,
                                Latitude = 51.48467,
                                Longitude = -0.64786
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000041",
                                SubRegionName = "Wokingham",
                                Checked = false,
                                Latitude = 51.41097,
                                Longitude = -0.83493
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000042",
                                SubRegionName = "Milton Keynes",
                                Checked = false,
                                Latitude = 52.04144,
                                Longitude = -0.76056
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000043",
                                SubRegionName = "Brighton and Hove",
                                Checked = false,
                                Latitude = 50.83022,
                                Longitude = -0.16783
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000044",
                                SubRegionName = "Portsmouth",
                                Checked = false,
                                Latitude = 50.79891,
                                Longitude = -1.09116
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000045",
                                SubRegionName = "Southampton",
                                Checked = false,
                                Latitude = 50.90497,
                                Longitude = -1.40323
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000046",
                                SubRegionName = "Isle of Wight",
                                Checked = false,
                                Latitude = 37.00561,
                                Longitude = -76.66292
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000002",
                                SubRegionName = "Buckinghamshire",
                                Checked = false,
                                Latitude = 52.04144,
                                Longitude = -0.76056
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000011",
                                SubRegionName = "East Sussex",
                                Checked = false,
                                Latitude = 50.81952,
                                Longitude = -0.13642
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000014",
                                SubRegionName = "Hampshire",
                                Checked = false,
                                Latitude = 42.09691,
                                Longitude = -88.53014
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000016",
                                SubRegionName = "Kent",
                                Checked = false,
                                Latitude = 42.40135,
                                Longitude = -82.19016
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000025",
                                SubRegionName = "Oxfordshire",
                                Checked = false,
                                Latitude = 51.75374,
                                Longitude = -1.26346
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000030",
                                SubRegionName = "Surrey",
                                Checked = false,
                                Latitude = 49.1046,
                                Longitude = -122.8235
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000032",
                                SubRegionName = "West Sussex",
                                Checked = false,
                                Latitude = 50.83664,
                                Longitude = -0.78018
                            }
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000009",
                        Checked = false,
                        RegionName = "South West",
                        SubRegion = new List<SubRegionItemModel>
                        {
                            new SubRegionItemModel
                            {
                                Id = "E06000022",
                                SubRegionName = "Bath and North East Somerset",
                                Checked = false,
                                Latitude = 51.34762,
                                Longitude = -2.9793
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000023",
                                SubRegionName = "Bristol, City of",
                                Checked = false,
                                Latitude = 33.72628,
                                Longitude = -117.8714
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000024",
                                SubRegionName = "North Somerset",
                                Checked = false,
                                Latitude = 33.44696,
                                Longitude = -111.7062
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000025",
                                SubRegionName = "South Gloucestershire",
                                Checked = false,
                                Latitude = 51.34762,
                                Longitude = -2.9793
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000026",
                                SubRegionName = "Plymouth",
                                Checked = false,
                                Latitude = 50.37038,
                                Longitude = -4.14265
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000027",
                                SubRegionName = "Torbay",
                                Checked = false,
                                Latitude = 47.65778,
                                Longitude = -52.73541
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000028",
                                SubRegionName = "Bournemouth",
                                Checked = false,
                                Latitude = 50.72168,
                                Longitude = -1.87853
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000029",
                                SubRegionName = "Poole",
                                Checked = false,
                                Latitude = 50.71939,
                                Longitude = -1.98114
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000030",
                                SubRegionName = "Swindon",
                                Checked = false,
                                Latitude = 51.55842,
                                Longitude = -1.78204
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000052",
                                SubRegionName = "Cornwall",
                                Checked = false,
                                Latitude = 45.01826,
                                Longitude = -74.72858
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000053",
                                SubRegionName = "Isles of Scilly",
                                Checked = false,
                                Latitude = 49.9146,
                                Longitude = -6.31574
                            },
                            new SubRegionItemModel
                            {
                                Id = "E06000054",
                                SubRegionName = "Wiltshire",
                                Checked = false,
                                Latitude = -40.83647,
                                Longitude = 145.2789
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000008",
                                SubRegionName = "Devon",
                                Checked = false,
                                Latitude = 53.36011,
                                Longitude = -113.7247
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000009",
                                SubRegionName = "Dorset",
                                Checked = false,
                                Latitude = 43.25522,
                                Longitude = -73.09921
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000013",
                                SubRegionName = "Gloucestershire",
                                Checked = false,
                                Latitude = 51.86674,
                                Longitude = -2.24867
                            },
                            new SubRegionItemModel
                            {
                                Id = "E10000027",
                                SubRegionName = "Somerset",
                                Checked = false,
                                Latitude = 37.09208,
                                Longitude = -84.60467
                            }
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
