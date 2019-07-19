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
                    ApiLocationId = 1,
                    Checked = false,
                    RegionName = "North East",
                    Latitude = 54.770012,
                    Longitude = -1.333720,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000001",
                            ApiLocationId = 11,
                            SubRegionName = "County Durham",
                            Checked = false,
                            Latitude = 54.77869,
                            Longitude = -1.55961
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000002",
                            ApiLocationId = 12,
                            SubRegionName = "Darlington",
                            Checked = false,
                            Latitude = 54.52873,
                            Longitude = -1.55305
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000003",
                            ApiLocationId = 13,
                            SubRegionName = "Gateshead",
                            Checked = false,
                            Latitude = 54.95937,
                            Longitude = -1.60182
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000004",
                            ApiLocationId = 14,
                            SubRegionName = "Hartlepool",
                            Checked = false,
                            Latitude = 54.68249,
                            Longitude = -1.2167
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000005",
                            ApiLocationId = 15,
                            SubRegionName = "Middlesbrough",
                            Checked = false,
                            Latitude = 54.57301,
                            Longitude = -1.23791
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000047",
                            ApiLocationId = 16,
                            SubRegionName = "Newcastle upon Tyne",
                            Checked = false,
                            Latitude = 54.97784,
                            Longitude = -1.61292
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000057",
                            ApiLocationId = 17,
                            SubRegionName = "North Tyneside",
                            Checked = false,
                            Latitude = 50.78061,
                            Longitude = 3.12184
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000021",
                            ApiLocationId = 18,
                            SubRegionName = "Northumberland",
                            Checked = false,
                            Latitude = 40.88925,
                            Longitude = -76.79354
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000022",
                            ApiLocationId = 19,
                            SubRegionName = "Redcar and Cleveland",
                            Checked = false,
                            Latitude = 54.60301,
                            Longitude = -1.07763
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000023",
                            ApiLocationId = 20,
                            SubRegionName = "South Tyneside",
                            Checked = false,
                            Latitude = 51.27034,
                            Longitude = 7.20263
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000024",
                            ApiLocationId = 21,
                            SubRegionName = "Stockton-on-Tees",
                            Checked = false,
                            Latitude = 54.56823,
                            Longitude = -1.31443
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000037",
                            ApiLocationId = 22,
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
                    ApiLocationId = 2,
                    Checked = false,
                    RegionName = "North West",
                    Latitude = 53.789707,
                    Longitude = -2.654100,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000006",
                            ApiLocationId = 23,
                            SubRegionName = "Halton",
                            Checked = false,
                            Latitude = -32.31548,
                            Longitude = 151.5143
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000007",
                            ApiLocationId = 24,
                            SubRegionName = "Warrington",
                            Checked = false,
                            Latitude = 53.39266,
                            Longitude = -2.587
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000008",
                            ApiLocationId = 25,
                            SubRegionName = "Blackburn with Darwen",
                            Checked = false,
                            Latitude = 53.7501,
                            Longitude = -2.48471
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000009",
                            ApiLocationId = 26,
                            SubRegionName = "Blackpool",
                            Checked = false,
                            Latitude = 53.81418,
                            Longitude = -3.05354
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000049",
                            ApiLocationId = 27,
                            SubRegionName = "Cheshire East",
                            Checked = false,
                            Latitude = 41.45423,
                            Longitude = -72.81231
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000050",
                            ApiLocationId = 28,
                            SubRegionName = "Cheshire West and Chester",
                            Checked = false,
                            Latitude = 53.21744,
                            Longitude = -2.74297
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000001",
                            ApiLocationId = 29,
                            SubRegionName = "Bolton",
                            Checked = false,
                            Latitude = 53.57846,
                            Longitude = -2.42984
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000002",
                            ApiLocationId = 30,
                            SubRegionName = "Bury",
                            Checked = false,
                            Latitude = 53.59346,
                            Longitude = -2.29854
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000003",
                            ApiLocationId = 31,
                            SubRegionName = "Manchester",
                            Checked = false,
                            Latitude = 53.48071,
                            Longitude = -2.23438
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000004",
                            ApiLocationId = 32,
                            SubRegionName = "Oldham",
                            Checked = false,
                            Latitude = 53.54125,
                            Longitude = -2.11766
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000005",
                            ApiLocationId = 33,
                            SubRegionName = "Rochdale",
                            Checked = false,
                            Latitude = 53.61635,
                            Longitude = -2.15871
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000006",
                            ApiLocationId = 34,
                            SubRegionName = "Salford",
                            Checked = false,
                            Latitude = 42.99748,
                            Longitude = -80.82748
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000007",
                            ApiLocationId = 35,
                            SubRegionName = "Stockport",
                            Checked = false,
                            Latitude = 53.40849,
                            Longitude = -2.14929
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000008",
                            ApiLocationId = 36,
                            SubRegionName = "Tameside",
                            Checked = false,
                            Latitude = 54.47409,
                            Longitude = -1.19072
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000009",
                            ApiLocationId = 37,
                            SubRegionName = "Trafford",
                            Checked = false,
                            Latitude = 33.82052,
                            Longitude = -86.74535
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000010",
                            ApiLocationId = 38,
                            SubRegionName = "Wigan",
                            Checked = false,
                            Latitude = 53.54427,
                            Longitude = -2.63106
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000011",
                            ApiLocationId = 39,
                            SubRegionName = "Knowsley",
                            Checked = false,
                            Latitude = -36.82613,
                            Longitude = 144.5873
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000012",
                            ApiLocationId = 40,
                            SubRegionName = "Liverpool",
                            Checked = false,
                            Latitude = 53.41078,
                            Longitude = -2.97784
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000013",
                            ApiLocationId = 41,
                            SubRegionName = "St Helens",
                            Checked = false,
                            Latitude = 53.45388,
                            Longitude = -2.73689
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000014",
                            ApiLocationId = 42,
                            SubRegionName = "Sefton",
                            Checked = false,
                            Latitude = -43.2486,
                            Longitude = 172.669
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000015",
                            ApiLocationId = 43,
                            SubRegionName = "Wirral",
                            Checked = false,
                            Latitude = 53.39199,
                            Longitude = -3.17886
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000006",
                            ApiLocationId = 44,
                            SubRegionName = "Cumbria",
                            Checked = false,
                            Latitude = 19.64903,
                            Longitude = -99.21791
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000017",
                            ApiLocationId = 45,
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
                    ApiLocationId = 3,
                    Checked = false,
                    RegionName = "Yorkshire and The Humber",
                    Latitude = 53.676289,
                    Longitude = -0.382000,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000010",
                            ApiLocationId = 46,
                            SubRegionName = "Kingston upon Hull",
                            Checked = false,
                            Latitude = 53.74434,
                            Longitude = -0.33244
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000011",
                            ApiLocationId = 47,
                            SubRegionName = "East Riding of Yorkshire",
                            Checked = false,
                            Latitude = 53.84292,
                            Longitude = -0.42766
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000012",
                            ApiLocationId = 48,
                            SubRegionName = "North East Lincolnshire",
                            Checked = false,
                            Latitude = 42.22473,
                            Longitude = -87.84481
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000013",
                            ApiLocationId = 49,
                            SubRegionName = "North Lincolnshire",
                            Checked = false,
                            Latitude = 40.57776,
                            Longitude = -85.68432
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000014",
                            ApiLocationId = 50,
                            SubRegionName = "York",
                            Checked = false,
                            Latitude = 40.71305,
                            Longitude = -74.00723
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000016",
                            ApiLocationId = 51,
                            SubRegionName = "Barnsley",
                            Checked = false,
                            Latitude = 53.55293,
                            Longitude = -1.48127
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000017",
                            ApiLocationId = 52,
                            SubRegionName = "Doncaster",
                            Checked = false,
                            Latitude = 53.52304,
                            Longitude = -1.13376
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000018",
                            ApiLocationId = 53,
                            SubRegionName = "Rotherham",
                            Checked = false,
                            Latitude = 53.4302,
                            Longitude = -1.35685
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000019",
                            ApiLocationId = 54,
                            SubRegionName = "Sheffield",
                            Checked = false,
                            Latitude = 53.38306,
                            Longitude = -1.46479
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000032",
                            ApiLocationId = 55,
                            SubRegionName = "Bradford",
                            Checked = false,
                            Latitude = 53.79385,
                            Longitude = -1.75244
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000033",
                            ApiLocationId = 56,
                            SubRegionName = "Calderdale",
                            Checked = false,
                            Latitude = 54.56555,
                            Longitude = -0.97638
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000034",
                            ApiLocationId = 57,
                            SubRegionName = "Kirklees",
                            Checked = false,
                            Latitude = 52.60206,
                            Longitude = 1.28036
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000035",
                            ApiLocationId = 58,
                            SubRegionName = "Leeds",
                            Checked = false,
                            Latitude = 53.79969,
                            Longitude = -1.5491
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000036",
                            ApiLocationId = 59,
                            SubRegionName = "Wakefield",
                            Checked = false,
                            Latitude = 53.68297,
                            Longitude = -1.4991
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000023",
                            ApiLocationId = 60,
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
                    ApiLocationId = 4,
                    Checked = false,
                    RegionName = "East Midlands",
                    Latitude = 52.829372,
                    Longitude = -1.332134,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000015",
                            ApiLocationId = 61,
                            SubRegionName = "Derby",
                            Checked = false,
                            Latitude = 52.9219,
                            Longitude = -1.47564
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000007",
                            ApiLocationId = 62,
                            SubRegionName = "Derbyshire",
                            Checked = false,
                            Latitude = 52.9219,
                            Longitude = -1.47564
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000016",
                            ApiLocationId = 63,
                            SubRegionName = "Leicester",
                            Checked = false,
                            Latitude = 52.63486,
                            Longitude = -1.12906
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000018",
                            ApiLocationId = 64,
                            SubRegionName = "Leicestershire",
                            Checked = false,
                            Latitude = 52.63486,
                            Longitude = -1.12906
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000019",
                            ApiLocationId = 65,
                            SubRegionName = "Lincolnshire",
                            Checked = false,
                            Latitude = 42.19589,
                            Longitude = -87.90625
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000021",
                            ApiLocationId = 66,
                            SubRegionName = "Northamptonshire",
                            Checked = false,
                            Latitude = 52.23484,
                            Longitude = -0.89732
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000018",
                            ApiLocationId = 67,
                            SubRegionName = "Nottingham",
                            Checked = false,
                            Latitude = 52.95512,
                            Longitude = -1.14917
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000024",
                            ApiLocationId = 68,
                            SubRegionName = "Nottinghamshire",
                            Checked = false,
                            Latitude = 52.95512,
                            Longitude = -1.14917
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000017",
                            ApiLocationId = 69,
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
                    ApiLocationId = 5,
                    Checked = false,
                    RegionName = "West Midlands",
                    Latitude = 52.475075,
                    Longitude = -1.829833,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000019",
                            ApiLocationId = 70,
                            SubRegionName = "Herefordshire",
                            Checked = false,
                            Latitude = 38.77813,
                            Longitude = -90.19985
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000020",
                            ApiLocationId = 71,
                            SubRegionName = "Telford and Wrekin",
                            Checked = false,
                            Latitude = 52.67677,
                            Longitude = -2.53548
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000021",
                            ApiLocationId = 72,
                            SubRegionName = "Stoke-on-Trent",
                            Checked = false,
                            Latitude = 53.02578,
                            Longitude = -2.17739
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000051",
                            ApiLocationId = 73,
                            SubRegionName = "Shropshire",
                            Checked = false,
                            Latitude = 52.67587,
                            Longitude = -2.4497
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000025",
                            ApiLocationId = 74,
                            SubRegionName = "Birmingham",
                            Checked = false,
                            Latitude = 52.4829,
                            Longitude = -1.89346
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000026",
                            ApiLocationId = 75,
                            SubRegionName = "Coventry",
                            Checked = false,
                            Latitude = 52.40631,
                            Longitude = -1.50852
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000027",
                            ApiLocationId = 76,
                            SubRegionName = "Dudley",
                            Checked = false,
                            Latitude = 52.50867,
                            Longitude = -2.08734
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000028",
                            ApiLocationId = 77,
                            SubRegionName = "Sandwell",
                            Checked = false,
                            Latitude = 52.50636,
                            Longitude = -1.96258
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000029",
                            ApiLocationId = 78,
                            SubRegionName = "Solihull",
                            Checked = false,
                            Latitude = 52.41471,
                            Longitude = -1.7743
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000030",
                            ApiLocationId = 79,
                            SubRegionName = "Walsall",
                            Checked = false,
                            Latitude = 52.58595,
                            Longitude = -1.98229
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000031",
                            ApiLocationId = 80,
                            SubRegionName = "Wolverhampton",
                            Checked = false,
                            Latitude = 52.58533,
                            Longitude = -2.13192
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000028",
                            ApiLocationId = 81,
                            SubRegionName = "Staffordshire",
                            Checked = false,
                            Latitude = -37.74601,
                            Longitude = 143.7076
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000031",
                            ApiLocationId = 82,
                            SubRegionName = "Warwickshire",
                            Checked = false,
                            Latitude = 52.28194,
                            Longitude = -1.58447
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000034",
                            ApiLocationId = 83,
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
                    ApiLocationId = 6,
                    Checked = false,
                    RegionName = "East of England",
                    Latitude = 52.543724,
                    Longitude = -0.319955,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000055",
                            ApiLocationId = 84,
                            SubRegionName = "Bedford",
                            Checked = false,
                            Latitude = 52.13571,
                            Longitude = -0.46804
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000003",
                            ApiLocationId = 85,
                            SubRegionName = "Cambridgeshire",
                            Checked = false,
                            Latitude = 52.57339,
                            Longitude = -0.24846
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000056",
                            ApiLocationId = 86,
                            SubRegionName = "Central Bedfordshire",
                            Checked = false,
                            Latitude = 52.00268,
                            Longitude = -0.29749
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000012",
                            ApiLocationId = 87,
                            SubRegionName = "Essex",
                            Checked = false,
                            Latitude = 42.17247,
                            Longitude = -82.81928
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000015",
                            ApiLocationId = 88,
                            SubRegionName = "Hertfordshire",
                            Checked = false,
                            Latitude = 41.30676,
                            Longitude = -72.93177
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000032",
                            ApiLocationId = 89,
                            SubRegionName = "Luton",
                            Checked = false,
                            Latitude = 51.87965,
                            Longitude = -0.41756
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000020",
                            ApiLocationId = 90,
                            SubRegionName = "Norfolk",
                            Checked = false,
                            Latitude = -29.04393,
                            Longitude = 167.9727
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000031",
                            ApiLocationId = 91,
                            SubRegionName = "Peterborough",
                            Checked = false,
                            Latitude = 52.57339,
                            Longitude = -0.24846
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000033",
                            ApiLocationId = 92,
                            SubRegionName = "Southend-on-Sea",
                            Checked = false,
                            Latitude = 51.54041,
                            Longitude = 0.71176
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000029",
                            ApiLocationId = 93,
                            SubRegionName = "Suffolk",
                            Checked = false,
                            Latitude = 36.72819,
                            Longitude = -76.58319
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000034",
                            ApiLocationId = 90,
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
                    ApiLocationId = 7,
                    Checked = false,
                    RegionName = "London",
                    Latitude = 51.507351,
                    Longitude = -0.127758,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E09000001",
                            ApiLocationId = 91,
                            SubRegionName = "City of London",
                            Checked = false,
                            Latitude = 51.51333,
                            Longitude = -0.08895
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000002",
                            ApiLocationId = 92,
                            SubRegionName = "Barking and Dagenham",
                            Checked = false,
                            Latitude = 51.53628,
                            Longitude = 0.08148
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000003",
                            ApiLocationId = 93,
                            SubRegionName = "Barnet",
                            Checked = false,
                            Latitude = 51.65293,
                            Longitude = -0.19961
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000004",
                            ApiLocationId = 94,
                            SubRegionName = "Bexley",
                            Checked = false,
                            Latitude = 51.44135,
                            Longitude = 0.14861
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000005",
                            ApiLocationId = 95,
                            SubRegionName = "Brent",
                            Checked = false,
                            Latitude = 30.46826,
                            Longitude = -87.23904
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000006",
                            ApiLocationId = 96,
                            SubRegionName = "Bromley",
                            Checked = false,
                            Latitude = 51.40568,
                            Longitude = 0.01435
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000007",
                            ApiLocationId = 97,
                            SubRegionName = "Camden",
                            Checked = false,
                            Latitude = 39.94521,
                            Longitude = -75.11883
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000008",
                            ApiLocationId = 98,
                            SubRegionName = "Croydon",
                            Checked = false,
                            Latitude = 51.37236,
                            Longitude = -0.1004
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000009",
                            ApiLocationId = 99,
                            SubRegionName = "Ealing",
                            Checked = false,
                            Latitude = -44.0452,
                            Longitude = 171.419
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000010",
                            ApiLocationId = 100,
                            SubRegionName = "Enfield",
                            Checked = false,
                            Latitude = 51.6521,
                            Longitude = -0.08153
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000011",
                            ApiLocationId = 101,
                            SubRegionName = "Greenwich",
                            Checked = false,
                            Latitude = 41.02653,
                            Longitude = -73.62855
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000012",
                            ApiLocationId = 102,
                            SubRegionName = "Hackney",
                            Checked = false,
                            Latitude = -34.91217,
                            Longitude = 138.6135
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000013",
                            ApiLocationId = 103,
                            SubRegionName = "Hammersmith and Fulham",
                            Checked = false,
                            Latitude = 51.47736,
                            Longitude = -0.20167
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000014",
                            ApiLocationId = 104,
                            SubRegionName = "Haringey",
                            Checked = false,
                            Latitude = 48.91493,
                            Longitude = 2.53423
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000015",
                            ApiLocationId = 105,
                            SubRegionName = "Harrow",
                            Checked = false,
                            Latitude = 51.57881,
                            Longitude = -0.33376
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000016",
                            ApiLocationId = 106,
                            SubRegionName = "Havering",
                            Checked = false,
                            Latitude = 51.61583,
                            Longitude = 0.18344
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000017",
                            ApiLocationId = 107,
                            SubRegionName = "Hillingdon",
                            Checked = false,
                            Latitude = 51.53358,
                            Longitude = -0.45258
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000018",
                            ApiLocationId = 108,
                            SubRegionName = "Hounslow",
                            Checked = false,
                            Latitude = 51.46759,
                            Longitude = -0.3618
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000019",
                            ApiLocationId = 109,
                            SubRegionName = "Islington",
                            Checked = false,
                            Latitude = -41.50998,
                            Longitude = 173.9643
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000020",
                            ApiLocationId = 110,
                            SubRegionName = "Kensington and Chelsea",
                            Checked = false,
                            Latitude = 40.69744,
                            Longitude = -73.97944
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000021",
                            ApiLocationId = 111,
                            SubRegionName = "Kingston upon Thames",
                            Checked = false,
                            Latitude = 51.41232,
                            Longitude = -0.30044
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000022",
                            ApiLocationId = 112,
                            SubRegionName = "Lambeth",
                            Checked = false,
                            Latitude = 42.90688,
                            Longitude = -81.29608
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000023",
                            ApiLocationId = 113,
                            SubRegionName = "Lewisham",
                            Checked = false,
                            Latitude = -42.83335,
                            Longitude = 147.6102
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000024",
                            ApiLocationId = 114,
                            SubRegionName = "Merton",
                            Checked = false,
                            Latitude = 43.14135,
                            Longitude = -88.31182
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000025",
                            ApiLocationId = 115,
                            SubRegionName = "Newham",
                            Checked = false,
                            Latitude = -37.31047,
                            Longitude = 144.5907
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000026",
                            ApiLocationId = 116,
                            SubRegionName = "Redbridge",
                            Checked = false,
                            Latitude = 46.40087,
                            Longitude = -79.18479
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000027",
                            ApiLocationId = 117,
                            SubRegionName = "Richmond upon Thames",
                            Checked = false,
                            Latitude = 51.41232,
                            Longitude = -0.30044
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000028",
                            ApiLocationId = 118,
                            SubRegionName = "Southwark",
                            Checked = false,
                            Latitude = 51.50172,
                            Longitude = -0.09796
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000029",
                            ApiLocationId = 119,
                            SubRegionName = "Sutton",
                            Checked = false,
                            Latitude = 51.36045,
                            Longitude = -0.19178
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000030",
                            ApiLocationId = 120,
                            SubRegionName = "Tower Hamlets",
                            Checked = false,
                            Latitude = 51.12929,
                            Longitude = 1.30422
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000031",
                            ApiLocationId = 121,
                            SubRegionName = "Waltham Forest",
                            Checked = false,
                            Latitude = -34.77444,
                            Longitude = 138.7221
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000032",
                            ApiLocationId = 122,
                            SubRegionName = "Wandsworth",
                            Checked = false,
                            Latitude = -30.06261,
                            Longitude = 151.5185
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000033",
                            ApiLocationId = 123,
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
                    ApiLocationId = 8,
                    Checked = false,
                    RegionName = "South East",
                    Latitude = 52.355518,
                    Longitude = -1.174320,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000035",
                            ApiLocationId = 124,
                            SubRegionName = "Medway",
                            Checked = false,
                            Latitude = 42.14028,
                            Longitude = -71.39804
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000036",
                            ApiLocationId = 125,
                            SubRegionName = "Bracknell Forest",
                            Checked = false,
                            Latitude = 28.80676,
                            Longitude = -81.70188
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000037",
                            ApiLocationId = 126,
                            SubRegionName = "West Berkshire",
                            Checked = false,
                            Latitude = 44.98942,
                            Longitude = -72.81304
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000038",
                            ApiLocationId = 127,
                            SubRegionName = "Reading",
                            Checked = false,
                            Latitude = 51.45504,
                            Longitude = -0.96909
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000039",
                            ApiLocationId = 128,
                            SubRegionName = "Slough",
                            Checked = false,
                            Latitude = 51.50935,
                            Longitude = -0.59545
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000040",
                            ApiLocationId = 129,
                            SubRegionName = "Windsor and Maidenhead",
                            Checked = false,
                            Latitude = 51.48467,
                            Longitude = -0.64786
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000041",
                            ApiLocationId = 130,
                            SubRegionName = "Wokingham",
                            Checked = false,
                            Latitude = 51.41097,
                            Longitude = -0.83493
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000042",
                            ApiLocationId = 131,
                            SubRegionName = "Milton Keynes",
                            Checked = false,
                            Latitude = 52.04144,
                            Longitude = -0.76056
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000043",
                            ApiLocationId = 132,
                            SubRegionName = "Brighton and Hove",
                            Checked = false,
                            Latitude = 50.83022,
                            Longitude = -0.16783
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000044",
                            ApiLocationId = 133,
                            SubRegionName = "Portsmouth",
                            Checked = false,
                            Latitude = 50.79891,
                            Longitude = -1.09116
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000045",
                            ApiLocationId = 134,
                            SubRegionName = "Southampton",
                            Checked = false,
                            Latitude = 50.90497,
                            Longitude = -1.40323
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000046",
                            ApiLocationId = 135,
                            SubRegionName = "Isle of Wight",
                            Checked = false,
                            Latitude = 37.00561,
                            Longitude = -76.66292
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000002",
                            ApiLocationId = 136,
                            SubRegionName = "Buckinghamshire",
                            Checked = false,
                            Latitude = 52.04144,
                            Longitude = -0.76056
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000011",
                            ApiLocationId = 137,
                            SubRegionName = "East Sussex",
                            Checked = false,
                            Latitude = 50.81952,
                            Longitude = -0.13642
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000014",
                            ApiLocationId = 138,
                            SubRegionName = "Hampshire",
                            Checked = false,
                            Latitude = 42.09691,
                            Longitude = -88.53014
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000016",
                            ApiLocationId = 139,
                            SubRegionName = "Kent",
                            Checked = false,
                            Latitude = 42.40135,
                            Longitude = -82.19016
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000025",
                            ApiLocationId = 140,
                            SubRegionName = "Oxfordshire",
                            Checked = false,
                            Latitude = 51.75374,
                            Longitude = -1.26346
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000030",
                            ApiLocationId = 141,
                            SubRegionName = "Surrey",
                            Checked = false,
                            Latitude = 49.1046,
                            Longitude = -122.8235
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000032",
                            ApiLocationId = 142,
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
                    ApiLocationId = 9,
                    Checked = false,
                    RegionName = "South West",
                    Latitude = 50.777214,
                    Longitude = -3.999461,
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000022",
                            ApiLocationId = 143,
                            SubRegionName = "Bath and North East Somerset",
                            Checked = false,
                            Latitude = 51.34762,
                            Longitude = -2.9793
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000023",
                            ApiLocationId = 144,
                            SubRegionName = "Bristol",
                            Checked = false,
                            Latitude = 33.72628,
                            Longitude = -117.8714
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000024",
                            ApiLocationId = 145,
                            SubRegionName = "North Somerset",
                            Checked = false,
                            Latitude = 33.44696,
                            Longitude = -111.7062
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000025",
                            ApiLocationId = 146,
                            SubRegionName = "South Gloucestershire",
                            Checked = false,
                            Latitude = 51.34762,
                            Longitude = -2.9793
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000026",
                            ApiLocationId = 147,
                            SubRegionName = "Plymouth",
                            Checked = false,
                            Latitude = 50.37038,
                            Longitude = -4.14265
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000027",
                            ApiLocationId = 148,
                            SubRegionName = "Torbay",
                            Checked = false,
                            Latitude = 47.65778,
                            Longitude = -52.73541
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000028",
                            ApiLocationId = 148,
                            SubRegionName = "Bournemouth",
                            Checked = false,
                            Latitude = 50.72168,
                            Longitude = -1.87853
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000029",
                            ApiLocationId = 149,
                            SubRegionName = "Poole",
                            Checked = false,
                            Latitude = 50.71939,
                            Longitude = -1.98114
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000030",
                            ApiLocationId = 150,
                            SubRegionName = "Swindon",
                            Checked = false,
                            Latitude = 51.55842,
                            Longitude = -1.78204
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000052",
                            ApiLocationId = 151,
                            SubRegionName = "Cornwall",
                            Checked = false,
                            Latitude = 45.01826,
                            Longitude = -74.72858
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000053",
                            ApiLocationId = 152,
                            SubRegionName = "Isles of Scilly",
                            Checked = false,
                            Latitude = 49.9146,
                            Longitude = -6.31574
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000054",
                            ApiLocationId = 153,
                            SubRegionName = "Wiltshire",
                            Checked = false,
                            Latitude = -40.83647,
                            Longitude = 145.2789
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000008",
                            ApiLocationId = 154,
                            SubRegionName = "Devon",
                            Checked = false,
                            Latitude = 53.36011,
                            Longitude = -113.7247
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000009",
                            ApiLocationId = 155,
                            SubRegionName = "Dorset",
                            Checked = false,
                            Latitude = 43.25522,
                            Longitude = -73.09921
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000013",
                            ApiLocationId = 156,
                            SubRegionName = "Gloucestershire",
                            Checked = false,
                            Latitude = 51.86674,
                            Longitude = -2.24867
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000027",
                            ApiLocationId = 157,
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