using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Models.Models.Regions
{
    public class SelectRegionModel
    {
        public IEnumerable<RegionItemModel> RegionItems { get; set; }

        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
        

        public SelectRegionModel()
        {
            RegionItems = new[]
            {
                new RegionItemModel
                {
                    Id = "E12000001",
                    ApiLocationId = 200000001,
                    Checked = false,
                    RegionName = "North East",
                    Latitude = 54.770012,
                    Longitude = -1.333720,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000001",
                            ApiLocationId = 200000011,
                            SubRegionName = "County Durham",
                            Checked = false,
                            Latitude = 54.77869,
                            Longitude = -1.55961
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000002",
                            ApiLocationId = 200000012,
                            SubRegionName = "Darlington",
                            Checked = false,
                            Latitude = 54.52873,
                            Longitude = -1.55305
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000003",
                            ApiLocationId = 200000013,
                            SubRegionName = "Gateshead",
                            Checked = false,
                            Latitude = 54.95937,
                            Longitude = -1.60182
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000004",
                            ApiLocationId = 200000014,
                            SubRegionName = "Hartlepool",
                            Checked = false,
                            Latitude = 54.68249,
                            Longitude = -1.2167
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000005",
                            ApiLocationId = 200000015,
                            SubRegionName = "Middlesbrough",
                            Checked = false,
                            Latitude = 54.57301,
                            Longitude = -1.23791
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000047",
                            ApiLocationId = 200000016,
                            SubRegionName = "Newcastle upon Tyne",
                            Checked = false,
                            Latitude = 54.97784,
                            Longitude = -1.61292
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000057",
                            ApiLocationId = 200000017,
                            SubRegionName = "North Tyneside",
                            Checked = false,
                            Latitude = 50.78061,
                            Longitude = 3.12184
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000021",
                            ApiLocationId = 200000018,
                            SubRegionName = "Northumberland",
                            Checked = false,
                            Latitude = 40.88925,
                            Longitude = -76.79354
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000022",
                            ApiLocationId = 200000019,
                            SubRegionName = "Redcar and Cleveland",
                            Checked = false,
                            Latitude = 54.60301,
                            Longitude = -1.07763
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000023",
                            ApiLocationId = 200000020,
                            SubRegionName = "South Tyneside",
                            Checked = false,
                            Latitude = 51.27034,
                            Longitude = 7.20263
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000024",
                            ApiLocationId = 200000021,
                            SubRegionName = "Stockton-on-Tees",
                            Checked = false,
                            Latitude = 54.56823,
                            Longitude = -1.31443
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000037",
                            ApiLocationId = 200000022,
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
                    ApiLocationId = 200000002,
                    Checked = false,
                    RegionName = "North West",
                    Latitude = 53.789707,
                    Longitude = -2.654100,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000006",
                            ApiLocationId = 200000023,
                            SubRegionName = "Halton",
                            Checked = false,
                            Latitude = -32.31548,
                            Longitude = 151.5143
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000007",
                            ApiLocationId = 200000024,
                            SubRegionName = "Warrington",
                            Checked = false,
                            Latitude = 53.39266,
                            Longitude = -2.587
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000008",
                            ApiLocationId = 200000025,
                            SubRegionName = "Blackburn with Darwen",
                            Checked = false,
                            Latitude = 53.7501,
                            Longitude = -2.48471
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000009",
                            ApiLocationId = 200000026,
                            SubRegionName = "Blackpool",
                            Checked = false,
                            Latitude = 53.81418,
                            Longitude = -3.05354
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000049",
                            ApiLocationId = 200000027,
                            SubRegionName = "Cheshire East",
                            Checked = false,
                            Latitude = 41.45423,
                            Longitude = -72.81231
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000050",
                            ApiLocationId = 200000028,
                            SubRegionName = "Cheshire West and Chester",
                            Checked = false,
                            Latitude = 53.21744,
                            Longitude = -2.74297
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000001",
                            ApiLocationId = 200000029,
                            SubRegionName = "Bolton",
                            Checked = false,
                            Latitude = 53.57846,
                            Longitude = -2.42984
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000002",
                            ApiLocationId = 200000030,
                            SubRegionName = "Bury",
                            Checked = false,
                            Latitude = 53.59346,
                            Longitude = -2.29854
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000003",
                            ApiLocationId = 200000031,
                            SubRegionName = "Manchester",
                            Checked = false,
                            Latitude = 53.48071,
                            Longitude = -2.23438
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000004",
                            ApiLocationId = 200000032,
                            SubRegionName = "Oldham",
                            Checked = false,
                            Latitude = 53.54125,
                            Longitude = -2.11766
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000005",
                            ApiLocationId = 200000033,
                            SubRegionName = "Rochdale",
                            Checked = false,
                            Latitude = 53.61635,
                            Longitude = -2.15871
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000006",
                            ApiLocationId = 200000034,
                            SubRegionName = "Salford",
                            Checked = false,
                            Latitude = 42.99748,
                            Longitude = -80.82748
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000007",
                            ApiLocationId = 200000035,
                            SubRegionName = "Stockport",
                            Checked = false,
                            Latitude = 53.40849,
                            Longitude = -2.14929
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000008",
                            ApiLocationId = 200000036,
                            SubRegionName = "Tameside",
                            Checked = false,
                            Latitude = 54.47409,
                            Longitude = -1.19072
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000009",
                            ApiLocationId = 200000037,
                            SubRegionName = "Trafford",
                            Checked = false,
                            Latitude = 33.82052,
                            Longitude = -86.74535
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000010",
                            ApiLocationId = 200000038,
                            SubRegionName = "Wigan",
                            Checked = false,
                            Latitude = 53.54427,
                            Longitude = -2.63106
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000011",
                            ApiLocationId = 200000039,
                            SubRegionName = "Knowsley",
                            Checked = false,
                            Latitude = -36.82613,
                            Longitude = 144.5873
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000012",
                            ApiLocationId = 200000040,
                            SubRegionName = "Liverpool",
                            Checked = false,
                            Latitude = 53.41078,
                            Longitude = -2.97784
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000013",
                            ApiLocationId = 200000041,
                            SubRegionName = "St Helens",
                            Checked = false,
                            Latitude = 53.45388,
                            Longitude = -2.73689
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000014",
                            ApiLocationId = 200000042,
                            SubRegionName = "Sefton",
                            Checked = false,
                            Latitude = -43.2486,
                            Longitude = 172.669
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000015",
                            ApiLocationId = 200000043,
                            SubRegionName = "Wirral",
                            Checked = false,
                            Latitude = 53.39199,
                            Longitude = -3.17886
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000006",
                            ApiLocationId = 200000044,
                            SubRegionName = "Cumbria",
                            Checked = false,
                            Latitude = 19.64903,
                            Longitude = -99.21791
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000017",
                            ApiLocationId = 200000045,
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
                    ApiLocationId = 200000003,
                    Checked = false,
                    RegionName = "Yorkshire and The Humber",
                    Latitude = 53.676289,
                    Longitude = -0.382000,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000010",
                            ApiLocationId = 200000046,
                            SubRegionName = "Kingston upon Hull",
                            Checked = false,
                            Latitude = 53.74434,
                            Longitude = -0.33244
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000011",
                            ApiLocationId = 200000047,
                            SubRegionName = "East Riding of Yorkshire",
                            Checked = false,
                            Latitude = 53.84292,
                            Longitude = -0.42766
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000012",
                            ApiLocationId = 200000048,
                            SubRegionName = "North East Lincolnshire",
                            Checked = false,
                            Latitude = 42.22473,
                            Longitude = -87.84481
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000013",
                            ApiLocationId = 200000049,
                            SubRegionName = "North Lincolnshire",
                            Checked = false,
                            Latitude = 40.57776,
                            Longitude = -85.68432
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000014",
                            ApiLocationId = 200000050,
                            SubRegionName = "York",
                            Checked = false,
                            Latitude = 40.71305,
                            Longitude = -74.00723
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000016",
                            ApiLocationId = 200000051,
                            SubRegionName = "Barnsley",
                            Checked = false,
                            Latitude = 53.55293,
                            Longitude = -1.48127
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000017",
                            ApiLocationId = 200000052,
                            SubRegionName = "Doncaster",
                            Checked = false,
                            Latitude = 53.52304,
                            Longitude = -1.13376
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000018",
                            ApiLocationId = 200000053,
                            SubRegionName = "Rotherham",
                            Checked = false,
                            Latitude = 53.4302,
                            Longitude = -1.35685
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000019",
                            ApiLocationId = 200000054,
                            SubRegionName = "Sheffield",
                            Checked = false,
                            Latitude = 53.38306,
                            Longitude = -1.46479
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000032",
                            ApiLocationId = 200000055,
                            SubRegionName = "Bradford",
                            Checked = false,
                            Latitude = 53.79385,
                            Longitude = -1.75244
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000033",
                            ApiLocationId = 200000056,
                            SubRegionName = "Calderdale",
                            Checked = false,
                            Latitude = 54.56555,
                            Longitude = -0.97638
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000034",
                            ApiLocationId = 200000057,
                            SubRegionName = "Kirklees",
                            Checked = false,
                            Latitude = 52.60206,
                            Longitude = 1.28036
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000035",
                            ApiLocationId = 200000058,
                            SubRegionName = "Leeds",
                            Checked = false,
                            Latitude = 53.79969,
                            Longitude = -1.5491
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000036",
                            ApiLocationId = 200000059,
                            SubRegionName = "Wakefield",
                            Checked = false,
                            Latitude = 53.68297,
                            Longitude = -1.4991
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000023",
                            ApiLocationId = 200000060,
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
                    ApiLocationId = 200000004,
                    Checked = false,
                    RegionName = "East Midlands",
                    Latitude = 52.829372,
                    Longitude = -1.332134,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000015",
                            ApiLocationId = 200000061,
                            SubRegionName = "Derby",
                            Checked = false,
                            Latitude = 52.9219,
                            Longitude = -1.47564
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000007",
                            ApiLocationId = 200000062,
                            SubRegionName = "Derbyshire",
                            Checked = false,
                            Latitude = 52.9219,
                            Longitude = -1.47564
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000016",
                            ApiLocationId = 200000063,
                            SubRegionName = "Leicester",
                            Checked = false,
                            Latitude = 52.63486,
                            Longitude = -1.12906
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000018",
                            ApiLocationId = 200000064,
                            SubRegionName = "Leicestershire",
                            Checked = false,
                            Latitude = 52.63486,
                            Longitude = -1.12906
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000019",
                            ApiLocationId = 200000065,
                            SubRegionName = "Lincolnshire",
                            Checked = false,
                            Latitude = 42.19589,
                            Longitude = -87.90625
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000021",
                            ApiLocationId = 200000066,
                            SubRegionName = "Northamptonshire",
                            Checked = false,
                            Latitude = 52.23484,
                            Longitude = -0.89732
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000018",
                            ApiLocationId = 200000067,
                            SubRegionName = "Nottingham",
                            Checked = false,
                            Latitude = 52.95512,
                            Longitude = -1.14917
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000024",
                            ApiLocationId = 200000068,
                            SubRegionName = "Nottinghamshire",
                            Checked = false,
                            Latitude = 52.95512,
                            Longitude = -1.14917
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000017",
                            ApiLocationId = 200000069,
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
                    ApiLocationId = 200000005,
                    Checked = false,
                    RegionName = "West Midlands",
                    Latitude = 52.475075,
                    Longitude = -1.829833,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000019",
                            ApiLocationId = 200000070,
                            SubRegionName = "Herefordshire",
                            Checked = false,
                            Latitude = 38.77813,
                            Longitude = -90.19985
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000020",
                            ApiLocationId = 200000071,
                            SubRegionName = "Telford and Wrekin",
                            Checked = false,
                            Latitude = 52.67677,
                            Longitude = -2.53548
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000021",
                            ApiLocationId = 200000072,
                            SubRegionName = "Stoke-on-Trent",
                            Checked = false,
                            Latitude = 53.02578,
                            Longitude = -2.17739
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000051",
                            ApiLocationId = 200000073,
                            SubRegionName = "Shropshire",
                            Checked = false,
                            Latitude = 52.67587,
                            Longitude = -2.4497
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000025",
                            ApiLocationId = 200000074,
                            SubRegionName = "Birmingham",
                            Checked = false,
                            Latitude = 52.4829,
                            Longitude = -1.89346
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000026",
                            ApiLocationId = 200000075,
                            SubRegionName = "Coventry",
                            Checked = false,
                            Latitude = 52.40631,
                            Longitude = -1.50852
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000027",
                            ApiLocationId = 200000076,
                            SubRegionName = "Dudley",
                            Checked = false,
                            Latitude = 52.50867,
                            Longitude = -2.08734
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000028",
                            ApiLocationId = 200000077,
                            SubRegionName = "Sandwell",
                            Checked = false,
                            Latitude = 52.50636,
                            Longitude = -1.96258
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000029",
                            ApiLocationId = 200000078,
                            SubRegionName = "Solihull",
                            Checked = false,
                            Latitude = 52.41471,
                            Longitude = -1.7743
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000030",
                            ApiLocationId = 200000079,
                            SubRegionName = "Walsall",
                            Checked = false,
                            Latitude = 52.58595,
                            Longitude = -1.98229
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000031",
                            ApiLocationId = 200000080,
                            SubRegionName = "Wolverhampton",
                            Checked = false,
                            Latitude = 52.58533,
                            Longitude = -2.13192
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000028",
                            ApiLocationId = 200000081,
                            SubRegionName = "Staffordshire",
                            Checked = false,
                            Latitude = -37.74601,
                            Longitude = 143.7076
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000031",
                            ApiLocationId = 200000082,
                            SubRegionName = "Warwickshire",
                            Checked = false,
                            Latitude = 52.28194,
                            Longitude = -1.58447
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000034",
                            ApiLocationId = 200000083,
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
                    ApiLocationId = 200000006,
                    Checked = false,
                    RegionName = "East of England",
                    Latitude = 52.543724,
                    Longitude = -0.319955,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000055",
                            ApiLocationId = 200000084,
                            SubRegionName = "Bedford",
                            Checked = false,
                            Latitude = 52.13571,
                            Longitude = -0.46804
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000003",
                            ApiLocationId = 200000085,
                            SubRegionName = "Cambridgeshire",
                            Checked = false,
                            Latitude = 52.57339,
                            Longitude = -0.24846
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000056",
                            ApiLocationId = 200000086,
                            SubRegionName = "Central Bedfordshire",
                            Checked = false,
                            Latitude = 52.00268,
                            Longitude = -0.29749
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000012",
                            ApiLocationId = 200000087,
                            SubRegionName = "Essex",
                            Checked = false,
                            Latitude = 42.17247,
                            Longitude = -82.81928
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000015",
                            ApiLocationId = 200000088,
                            SubRegionName = "Hertfordshire",
                            Checked = false,
                            Latitude = 41.30676,
                            Longitude = -72.93177
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000032",
                            ApiLocationId = 200000089,
                            SubRegionName = "Luton",
                            Checked = false,
                            Latitude = 51.87965,
                            Longitude = -0.41756
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000020",
                            ApiLocationId = 200000090,
                            SubRegionName = "Norfolk",
                            Checked = false,
                            Latitude = -29.04393,
                            Longitude = 167.9727
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000031",
                            ApiLocationId = 200000091,
                            SubRegionName = "Peterborough",
                            Checked = false,
                            Latitude = 52.57339,
                            Longitude = -0.24846
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000033",
                            ApiLocationId = 200000092,
                            SubRegionName = "Southend-on-Sea",
                            Checked = false,
                            Latitude = 51.54041,
                            Longitude = 0.71176
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000029",
                            ApiLocationId = 200000093,
                            SubRegionName = "Suffolk",
                            Checked = false,
                            Latitude = 36.72819,
                            Longitude = -76.58319
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000034",
                            ApiLocationId = 200000090,
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
                    ApiLocationId = 200000007,
                    Checked = false,
                    RegionName = "London",
                    Latitude = 51.507351,
                    Longitude = -0.127758,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E09000001",
                            ApiLocationId = 200000091,
                            SubRegionName = "City of London",
                            Checked = false,
                            Latitude = 51.51333,
                            Longitude = -0.08895
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000002",
                            ApiLocationId = 200000092,
                            SubRegionName = "Barking and Dagenham",
                            Checked = false,
                            Latitude = 51.53628,
                            Longitude = 0.08148
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000003",
                            ApiLocationId = 200000093,
                            SubRegionName = "Barnet",
                            Checked = false,
                            Latitude = 51.65293,
                            Longitude = -0.19961
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000004",
                            ApiLocationId = 200000094,
                            SubRegionName = "Bexley",
                            Checked = false,
                            Latitude = 51.44135,
                            Longitude = 0.14861
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000005",
                            ApiLocationId = 200000095,
                            SubRegionName = "Brent",
                            Checked = false,
                            Latitude = 30.46826,
                            Longitude = -87.23904
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000006",
                            ApiLocationId = 200000096,
                            SubRegionName = "Bromley",
                            Checked = false,
                            Latitude = 51.40568,
                            Longitude = 0.01435
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000007",
                            ApiLocationId = 200000097,
                            SubRegionName = "Camden",
                            Checked = false,
                            Latitude = 39.94521,
                            Longitude = -75.11883
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000008",
                            ApiLocationId = 200000098,
                            SubRegionName = "Croydon",
                            Checked = false,
                            Latitude = 51.37236,
                            Longitude = -0.1004
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000009",
                            ApiLocationId = 200000099,
                            SubRegionName = "Ealing",
                            Checked = false,
                            Latitude = -44.0452,
                            Longitude = 171.419
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000010",
                            ApiLocationId = 200000100,
                            SubRegionName = "Enfield",
                            Checked = false,
                            Latitude = 51.6521,
                            Longitude = -0.08153
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000011",
                            ApiLocationId = 200000101,
                            SubRegionName = "Greenwich",
                            Checked = false,
                            Latitude = 41.02653,
                            Longitude = -73.62855
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000012",
                            ApiLocationId = 200000102,
                            SubRegionName = "Hackney",
                            Checked = false,
                            Latitude = -34.91217,
                            Longitude = 138.6135
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000013",
                            ApiLocationId = 200000103,
                            SubRegionName = "Hammersmith and Fulham",
                            Checked = false,
                            Latitude = 51.47736,
                            Longitude = -0.20167
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000014",
                            ApiLocationId = 200000104,
                            SubRegionName = "Haringey",
                            Checked = false,
                            Latitude = 48.91493,
                            Longitude = 2.53423
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000015",
                            ApiLocationId = 200000105,
                            SubRegionName = "Harrow",
                            Checked = false,
                            Latitude = 51.57881,
                            Longitude = -0.33376
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000016",
                            ApiLocationId = 200000106,
                            SubRegionName = "Havering",
                            Checked = false,
                            Latitude = 51.61583,
                            Longitude = 0.18344
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000017",
                            ApiLocationId = 200000107,
                            SubRegionName = "Hillingdon",
                            Checked = false,
                            Latitude = 51.53358,
                            Longitude = -0.45258
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000018",
                            ApiLocationId = 200000108,
                            SubRegionName = "Hounslow",
                            Checked = false,
                            Latitude = 51.46759,
                            Longitude = -0.3618
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000019",
                            ApiLocationId = 200000109,
                            SubRegionName = "Islington",
                            Checked = false,
                            Latitude = -41.50998,
                            Longitude = 173.9643
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000020",
                            ApiLocationId = 200000110,
                            SubRegionName = "Kensington and Chelsea",
                            Checked = false,
                            Latitude = 40.69744,
                            Longitude = -73.97944
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000021",
                            ApiLocationId = 200000111,
                            SubRegionName = "Kingston upon Thames",
                            Checked = false,
                            Latitude = 51.41232,
                            Longitude = -0.30044
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000022",
                            ApiLocationId = 200000112,
                            SubRegionName = "Lambeth",
                            Checked = false,
                            Latitude = 42.90688,
                            Longitude = -81.29608
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000023",
                            ApiLocationId = 200000113,
                            SubRegionName = "Lewisham",
                            Checked = false,
                            Latitude = -42.83335,
                            Longitude = 147.6102
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000024",
                            ApiLocationId = 200000114,
                            SubRegionName = "Merton",
                            Checked = false,
                            Latitude = 43.14135,
                            Longitude = -88.31182
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000025",
                            ApiLocationId = 200000115,
                            SubRegionName = "Newham",
                            Checked = false,
                            Latitude = -37.31047,
                            Longitude = 144.5907
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000026",
                            ApiLocationId = 200000116,
                            SubRegionName = "Redbridge",
                            Checked = false,
                            Latitude = 46.40087,
                            Longitude = -79.18479
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000027",
                            ApiLocationId = 200000117,
                            SubRegionName = "Richmond upon Thames",
                            Checked = false,
                            Latitude = 51.41232,
                            Longitude = -0.30044
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000028",
                            ApiLocationId = 200000118,
                            SubRegionName = "Southwark",
                            Checked = false,
                            Latitude = 51.50172,
                            Longitude = -0.09796
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000029",
                            ApiLocationId = 200000119,
                            SubRegionName = "Sutton",
                            Checked = false,
                            Latitude = 51.36045,
                            Longitude = -0.19178
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000030",
                            ApiLocationId = 200000120,
                            SubRegionName = "Tower Hamlets",
                            Checked = false,
                            Latitude = 51.12929,
                            Longitude = 1.30422
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000031",
                            ApiLocationId = 200000121,
                            SubRegionName = "Waltham Forest",
                            Checked = false,
                            Latitude = -34.77444,
                            Longitude = 138.7221
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000032",
                            ApiLocationId = 200000122,
                            SubRegionName = "Wandsworth",
                            Checked = false,
                            Latitude = -30.06261,
                            Longitude = 151.5185
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000033",
                            ApiLocationId = 200000123,
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
                    ApiLocationId = 200000008,
                    Checked = false,
                    RegionName = "South East",
                    Latitude = 52.355518,
                    Longitude = -1.174320,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000035",
                            ApiLocationId = 200000124,
                            SubRegionName = "Medway",
                            Checked = false,
                            Latitude = 42.14028,
                            Longitude = -71.39804
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000036",
                            ApiLocationId = 200000125,
                            SubRegionName = "Bracknell Forest",
                            Checked = false,
                            Latitude = 28.80676,
                            Longitude = -81.70188
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000037",
                            ApiLocationId = 200000126,
                            SubRegionName = "West Berkshire",
                            Checked = false,
                            Latitude = 44.98942,
                            Longitude = -72.81304
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000038",
                            ApiLocationId = 200000127,
                            SubRegionName = "Reading",
                            Checked = false,
                            Latitude = 51.45504,
                            Longitude = -0.96909
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000039",
                            ApiLocationId = 200000128,
                            SubRegionName = "Slough",
                            Checked = false,
                            Latitude = 51.50935,
                            Longitude = -0.59545
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000040",
                            ApiLocationId = 200000129,
                            SubRegionName = "Windsor and Maidenhead",
                            Checked = false,
                            Latitude = 51.48467,
                            Longitude = -0.64786
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000041",
                            ApiLocationId = 200000130,
                            SubRegionName = "Wokingham",
                            Checked = false,
                            Latitude = 51.41097,
                            Longitude = -0.83493
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000042",
                            ApiLocationId = 200000131,
                            SubRegionName = "Milton Keynes",
                            Checked = false,
                            Latitude = 52.04144,
                            Longitude = -0.76056
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000043",
                            ApiLocationId = 200000132,
                            SubRegionName = "Brighton and Hove",
                            Checked = false,
                            Latitude = 50.83022,
                            Longitude = -0.16783
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000044",
                            ApiLocationId = 200000133,
                            SubRegionName = "Portsmouth",
                            Checked = false,
                            Latitude = 50.79891,
                            Longitude = -1.09116
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000045",
                            ApiLocationId = 200000134,
                            SubRegionName = "Southampton",
                            Checked = false,
                            Latitude = 50.90497,
                            Longitude = -1.40323
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000046",
                            ApiLocationId = 200000135,
                            SubRegionName = "Isle of Wight",
                            Checked = false,
                            Latitude = 37.00561,
                            Longitude = -76.66292
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000002",
                            ApiLocationId = 200000136,
                            SubRegionName = "Buckinghamshire",
                            Checked = false,
                            Latitude = 52.04144,
                            Longitude = -0.76056
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000011",
                            ApiLocationId = 200000137,
                            SubRegionName = "East Sussex",
                            Checked = false,
                            Latitude = 50.81952,
                            Longitude = -0.13642
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000014",
                            ApiLocationId = 200000138,
                            SubRegionName = "Hampshire",
                            Checked = false,
                            Latitude = 42.09691,
                            Longitude = -88.53014
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000016",
                            ApiLocationId = 200000139,
                            SubRegionName = "Kent",
                            Checked = false,
                            Latitude = 42.40135,
                            Longitude = -82.19016
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000025",
                            ApiLocationId = 200000140,
                            SubRegionName = "Oxfordshire",
                            Checked = false,
                            Latitude = 51.75374,
                            Longitude = -1.26346
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000030",
                            ApiLocationId = 200000141,
                            SubRegionName = "Surrey",
                            Checked = false,
                            Latitude = 49.1046,
                            Longitude = -122.8235
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000032",
                            ApiLocationId = 200000142,
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
                    ApiLocationId = 200000009,
                    Checked = false,
                    RegionName = "South West",
                    Latitude = 50.777214,
                    Longitude = -3.999461,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000022",
                            ApiLocationId = 200000143,
                            SubRegionName = "Bath and North East Somerset",
                            Checked = false,
                            Latitude = 51.34762,
                            Longitude = -2.9793
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000023",
                            ApiLocationId = 200000144,
                            SubRegionName = "Bristol",
                            Checked = false,
                            Latitude = 33.72628,
                            Longitude = -117.8714
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000024",
                            ApiLocationId = 200000145,
                            SubRegionName = "North Somerset",
                            Checked = false,
                            Latitude = 33.44696,
                            Longitude = -111.7062
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000025",
                            ApiLocationId = 200000146,
                            SubRegionName = "South Gloucestershire",
                            Checked = false,
                            Latitude = 51.34762,
                            Longitude = -2.9793
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000026",
                            ApiLocationId = 200000147,
                            SubRegionName = "Plymouth",
                            Checked = false,
                            Latitude = 50.37038,
                            Longitude = -4.14265
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000027",
                            ApiLocationId = 200000148,
                            SubRegionName = "Torbay",
                            Checked = false,
                            Latitude = 47.65778,
                            Longitude = -52.73541
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000028",
                            ApiLocationId = 200000148,
                            SubRegionName = "Bournemouth",
                            Checked = false,
                            Latitude = 50.72168,
                            Longitude = -1.87853
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000029",
                            ApiLocationId = 200000149,
                            SubRegionName = "Poole",
                            Checked = false,
                            Latitude = 50.71939,
                            Longitude = -1.98114
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000030",
                            ApiLocationId = 200000150,
                            SubRegionName = "Swindon",
                            Checked = false,
                            Latitude = 51.55842,
                            Longitude = -1.78204
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000052",
                            ApiLocationId = 200000151,
                            SubRegionName = "Cornwall",
                            Checked = false,
                            Latitude = 45.01826,
                            Longitude = -74.72858
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000053",
                            ApiLocationId = 200000152,
                            SubRegionName = "Isles of Scilly",
                            Checked = false,
                            Latitude = 49.9146,
                            Longitude = -6.31574
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000054",
                            ApiLocationId = 200000153,
                            SubRegionName = "Wiltshire",
                            Checked = false,
                            Latitude = -40.83647,
                            Longitude = 145.2789
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000008",
                            ApiLocationId = 200000154,
                            SubRegionName = "Devon",
                            Checked = false,
                            Latitude = 53.36011,
                            Longitude = -113.7247
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000009",
                            ApiLocationId = 200000155,
                            SubRegionName = "Dorset",
                            Checked = false,
                            Latitude = 43.25522,
                            Longitude = -73.09921
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000013",
                            ApiLocationId = 200000156,
                            SubRegionName = "Gloucestershire",
                            Checked = false,
                            Latitude = 51.86674,
                            Longitude = -2.24867
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000027",
                            ApiLocationId = 200000157,
                            SubRegionName = "Somerset",
                            Checked = false,
                            Latitude = 37.09208,
                            Longitude = -84.60467
                        }
                    }
                }
            }; 
        }
        
        public SubRegionItemModel GetSubRegionItemByRegionCode(string regionCode)
        {
            foreach (var regionItemModel in RegionItems)
            {
                foreach (var subRegionItemModel in regionItemModel.SubRegion)
                {
                    if (subRegionItemModel.Id == regionCode)
                    {
                        return subRegionItemModel;
                    }
                }

                if (regionItemModel.Id == regionCode)
                {
                    return new SubRegionItemModel
                    {
                        Id = regionItemModel.Id,
                        Latitude = regionItemModel.Latitude,
                        Longitude = regionItemModel.Longitude,
                        SubRegionName = regionItemModel.RegionName
                    };
                }
            }

            return new SubRegionItemModel();
        }

        public string[] SubRegionsDataCleanse(List<string> subRegions)
        {
            List<string> revisedSubRegions = subRegions.Select(x=> x).ToList();

            foreach (var regionItemModel in RegionItems)
            {
                var subRegionCheckedCount = 0;
                foreach (var subRegionItemModel in regionItemModel.SubRegion)
                {
                    foreach (var subRegion in subRegions)
                    {
                        if (subRegionItemModel.Id == subRegion)
                        {
                            subRegionCheckedCount++;
                        }
                    }
                }

                var subRegionCount = regionItemModel.SubRegion.Count;
                if (subRegionCount == subRegionCheckedCount)
                {
                    foreach (var subRegionItemModel in regionItemModel.SubRegion)
                    {
                        revisedSubRegions.Remove(subRegionItemModel.Id);
                    }

                    revisedSubRegions.Add(regionItemModel.Id);
                }
            }

            return revisedSubRegions.Distinct().ToArray();
        }
        public string GetRegionNameForSubRegion(string subRegionCode)
        {
            var regionItem = RegionItems.Where(x => x.SubRegion.Any(y => y.Id == subRegionCode)).FirstOrDefault();
            return regionItem != null ? regionItem.RegionName : string.Empty;
        }

    }
}