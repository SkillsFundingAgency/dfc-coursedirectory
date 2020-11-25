using System.Collections.Generic;
using System.Linq;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models.Regions
{
    public class SelectRegionModel
    {
        public IEnumerable<RegionItemModel> RegionItems { get; set; }

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
                    Postcode = "SR8 5AJ",
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000001",
                            ApiLocationId = 200000011,
                            SubRegionName = "County Durham",
                            Checked = false,
                            Latitude = 54.77869,
                            Longitude = -1.55961,
                            Postcode = "DH1 1QQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000002",
                            ApiLocationId = 200000012,
                            SubRegionName = "Darlington",
                            Checked = false,
                            Latitude = 54.52873,
                            Longitude = -1.55305,
                            Postcode ="DL1 1RL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000003",
                            ApiLocationId = 200000013,
                            SubRegionName = "Gateshead",
                            Checked = false,
                            Latitude = 54.95937,
                            Longitude = -1.60182,
                            Postcode = "NE8 1EN"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000004",
                            ApiLocationId = 200000014,
                            SubRegionName = "Hartlepool",
                            Checked = false,
                            Latitude = 54.68249,
                            Longitude = -1.2167,
                            Postcode = "TS26 9HU"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000005",
                            ApiLocationId = 200000015,
                            SubRegionName = "Middlesbrough",
                            Checked = false,
                            Latitude = 54.57301,
                            Longitude = -1.23791,
                            Postcode = "TS1 5DL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000047",
                            ApiLocationId = 200000016,
                            SubRegionName = "Newcastle upon Tyne",
                            Checked = false,
                            Latitude = 54.97784,
                            Longitude = -1.61292,
                            Postcode = "NE1 7DQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000057",
                            ApiLocationId = 200000017,
                            SubRegionName = "North Tyneside",
                            Checked = false,
                            Latitude = 55.0182,
                            Longitude = 1.4858,
                            Postcode = "NE27 0BY"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000021",
                            ApiLocationId = 200000018,
                            SubRegionName = "Northumberland",
                            Checked = false,
                            Latitude = 55.2083,
                            Longitude = -2.0784,
                            Postcode = "NE19 1BT"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000022",
                            ApiLocationId = 200000019,
                            SubRegionName = "Redcar and Cleveland",
                            Checked = false,
                            Latitude = 54.60301,
                            Longitude = -1.07763,
                            Postcode = "TS10 4BF"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000023",
                            ApiLocationId = 200000020,
                            SubRegionName = "South Tyneside",
                            Checked = false,
                            Latitude = 54.9637,
                            Longitude = -1.4419,
                            Postcode = "NE34 9UG"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000024",
                            ApiLocationId = 200000021,
                            SubRegionName = "Stockton-on-Tees",
                            Checked = false,
                            Latitude = 54.56823,
                            Longitude = -1.31443,
                            Postcode = "TS18 2AA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000037",
                            ApiLocationId = 200000022,
                            SubRegionName = "Sunderland",
                            Checked = false,
                            Latitude = 54.90445,
                            Longitude = -1.38145,
                            Postcode = "SR1 1QB"
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
                    Postcode =  "PR2 5PD",
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000006",
                            ApiLocationId = 200000023,
                            SubRegionName = "Halton",
                            Checked = false,
                            Latitude = 53.3613,
                            Longitude = -2.7335,
                            Postcode = "WA8 0DS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000007",
                            ApiLocationId = 200000024,
                            SubRegionName = "Warrington",
                            Checked = false,
                            Latitude = 53.39266,
                            Longitude = -2.587,
                            Postcode = "WA1 2PR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000008",
                            ApiLocationId = 200000025,
                            SubRegionName = "Blackburn with Darwen",
                            Checked = false,
                            Latitude = 53.7501,
                            Longitude = -2.48471,
                            Postcode = "BB1 7HU"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000009",
                            ApiLocationId = 200000026,
                            SubRegionName = "Blackpool",
                            Checked = false,
                            Latitude = 53.81418,
                            Longitude = -3.05354,
                            Postcode = "FY1 5PY"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000049",
                            ApiLocationId = 200000027,
                            SubRegionName = "Cheshire East",
                            Checked = false,
                            Latitude = 53.1610,
                            Longitude = -2.2186,
                            Postcode = "CW11 2SS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000050",
                            ApiLocationId = 200000028,
                            SubRegionName = "Cheshire West and Chester",
                            Checked = false,
                            Latitude = 53.21744,
                            Longitude = -2.74297,
                            Postcode = "CH3 8BJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000001",
                            ApiLocationId = 200000029,
                            SubRegionName = "Bolton",
                            Checked = false,
                            Latitude = 53.57846,
                            Longitude = -2.42984,
                            Postcode = "BL1 1RU"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000002",
                            ApiLocationId = 200000030,
                            SubRegionName = "Bury",
                            Checked = false,
                            Latitude = 53.59346,
                            Longitude = -2.29854,
                            Postcode = "BL9 0EY"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000003",
                            ApiLocationId = 200000031,
                            SubRegionName = "Manchester",
                            Checked = false,
                            Latitude = 53.48071,
                            Longitude = -2.23438,
                            Postcode = "M1 2BS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000004",
                            ApiLocationId = 200000032,
                            SubRegionName = "Oldham",
                            Checked = false,
                            Latitude = 53.54125,
                            Longitude = -2.11766,
                            Postcode = "OL1 1UT"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000005",
                            ApiLocationId = 200000033,
                            SubRegionName = "Rochdale",
                            Checked = false,
                            Latitude = 53.61635,
                            Longitude = -2.15871,
                            Postcode = "OL16 1AE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000006",
                            ApiLocationId = 200000034,
                            SubRegionName = "Salford",
                            Checked = false,
                            Latitude = 53.4875,
                            Longitude = -2.2901,
                            Postcode = "M6 5JE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000007",
                            ApiLocationId = 200000035,
                            SubRegionName = "Stockport",
                            Checked = false,
                            Latitude = 53.40849,
                            Longitude = -2.14929,
                            Postcode = "SK1 4AA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000008",
                            ApiLocationId = 200000036,
                            SubRegionName = "Tameside",
                            Checked = false,
                            Latitude = 53.4806,
                            Longitude = -2.0810,
                            Postcode = "TS9 5PE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000009",
                            ApiLocationId = 200000037,
                            SubRegionName = "Trafford",
                            Checked = false,
                            Latitude = 53.4707,
                            Longitude = -2.3231,
                            Postcode = "M33 4TJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000010",
                            ApiLocationId = 200000038,
                            SubRegionName = "Wigan",
                            Checked = false,
                            Latitude = 53.54427,
                            Longitude = -2.63106,
                            Postcode = "WN1 1DY"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000011",
                            ApiLocationId = 200000039,
                            SubRegionName = "Knowsley",
                            Checked = false,
                            Latitude = 53.4546,
                            Longitude = -2.8529,
                            Postcode = "L34 1NL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000012",
                            ApiLocationId = 200000040,
                            SubRegionName = "Liverpool",
                            Checked = false,
                            Latitude = 53.41078,
                            Longitude = -2.97784,
                            Postcode = "L3 8EG"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000013",
                            ApiLocationId = 200000041,
                            SubRegionName = "St Helens",
                            Checked = false,
                            Latitude = 53.45388,
                            Longitude = -2.73689,
                            Postcode = "WA10 1HP"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000014",
                            ApiLocationId = 200000042,
                            SubRegionName = "Sefton",
                            Checked = false,
                            Latitude = 53.5034,
                            Longitude = -2.9704,
                            Postcode = "L37 3HA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000015",
                            ApiLocationId = 200000043,
                            SubRegionName = "Wirral",
                            Checked = false,
                            Latitude = 53.3727,
                            Longitude = -3.0738,
                            Postcode = "CH49 5PL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000006",
                            ApiLocationId = 200000044,
                            SubRegionName = "Cumbria",
                            Checked = false,
                            Latitude = 54.5772,
                            Longitude = -2.7975,
                            Postcode = "CA12 4TW"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000017",
                            ApiLocationId = 200000045,
                            SubRegionName = "Lancashire",
                            Checked = false,
                            Latitude = 53.54125,
                            Longitude = -2.11766,
                            Postcode = "OL1 1UT"
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
                    Postcode = "DN19 7SS",
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000010",
                            ApiLocationId = 200000046,
                            SubRegionName = "Kingston upon Hull",
                            Checked = false,
                            Latitude = 53.74434,
                            Longitude = -0.33244,
                            Postcode = "HU1 2AA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000011",
                            ApiLocationId = 200000047,
                            SubRegionName = "East Riding of Yorkshire",
                            Checked = false,
                            Latitude = 53.84292,
                            Longitude = -0.42766,
                            Postcode = "HU17 9FB"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000012",
                            ApiLocationId = 200000048,
                            SubRegionName = "North East Lincolnshire",
                            Checked = false,
                            Latitude = 53.5668,
                            Longitude = -0.0815,
                            Postcode = "DN37 0BL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000013",
                            ApiLocationId = 200000049,
                            SubRegionName = "North Lincolnshire",
                            Checked = false,
                            Latitude = 53.6056,
                            Longitude = -0.5597,
                            Postcode = "DN15 0DE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000014",
                            ApiLocationId = 200000050,
                            SubRegionName = "York",
                            Checked = false,
                            Latitude = 53.9600,
                            Longitude = -1.0873,
                            Postcode = "YO1 6DU"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000016",
                            ApiLocationId = 200000051,
                            SubRegionName = "Barnsley",
                            Checked = false,
                            Latitude = 53.55293,
                            Longitude = -1.48127,
                            Postcode = "S70 2QE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000017",
                            ApiLocationId = 200000052,
                            SubRegionName = "Doncaster",
                            Checked = false,
                            Latitude = 53.52304,
                            Longitude = -1.13376,
                            Postcode = "DN1 1DN"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000018",
                            ApiLocationId = 200000053,
                            SubRegionName = "Rotherham",
                            Checked = false,
                            Latitude = 53.4302,
                            Longitude = -1.35685,
                            Postcode = "S60 1FF"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000019",
                            ApiLocationId = 200000054,
                            SubRegionName = "Sheffield",
                            Checked = false,
                            Latitude = 53.38306,
                            Longitude = -1.46479,
                            Postcode = "S1 2AZ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000032",
                            ApiLocationId = 200000055,
                            SubRegionName = "Bradford",
                            Checked = false,
                            Latitude = 53.79385,
                            Longitude = -1.75244,
                            Postcode = "BD1 1JF"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000033",
                            ApiLocationId = 200000056,
                            SubRegionName = "Calderdale",
                            Checked = false,
                            Latitude = 53.7248,
                            Longitude = -1.8658,
                            Postcode = "HX7 5HQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000034",
                            ApiLocationId = 200000057,
                            SubRegionName = "Kirklees",
                            Checked = false,
                            Latitude = 53.5933,
                            Longitude = -1.8010,
                            Postcode = "HD5 8XR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000035",
                            ApiLocationId = 200000058,
                            SubRegionName = "Leeds",
                            Checked = false,
                            Latitude = 53.79969,
                            Longitude = -1.5491,
                            Postcode = "LS2 7AU"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000036",
                            ApiLocationId = 200000059,
                            SubRegionName = "Wakefield",
                            Checked = false,
                            Latitude = 53.68297,
                            Longitude = -1.4991,
                            Postcode = "WF1 1PQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000023",
                            ApiLocationId = 200000060,
                            SubRegionName = "North Yorkshire",
                            Checked = false,
                            Latitude = 53.9915,
                            Longitude = -1.5412,
                            Postcode = "YO7 4EG"
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
                    Postcode = "DE74 2SA",
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000015",
                            ApiLocationId = 200000061,
                            SubRegionName = "Derby",
                            Checked = false,
                            Latitude = 52.9219,
                            Longitude = -1.47564,
                            Postcode = "DE1 2DS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000007",
                            ApiLocationId = 200000062,
                            SubRegionName = "Derbyshire",
                            Checked = false,
                            Latitude = 52.9219,
                            Longitude = -1.47564,
                            Postcode = "DE1 2DS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000016",
                            ApiLocationId = 200000063,
                            SubRegionName = "Leicester",
                            Checked = false,
                            Latitude = 52.63486,
                            Longitude = -1.12906,
                            Postcode = "LE1 1TQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000018",
                            ApiLocationId = 200000064,
                            SubRegionName = "Leicestershire",
                            Checked = false,
                            Latitude = 52.63486,
                            Longitude = -1.12906,
                            Postcode = "LE1 1TQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000019",
                            ApiLocationId = 200000065,
                            SubRegionName = "Lincolnshire",
                            Checked = false,
                            Latitude = 52.9452,
                            Longitude = -0.1601,
                            Postcode = "LN10 6TT"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000021",
                            ApiLocationId = 200000066,
                            SubRegionName = "Northamptonshire",
                            Checked = false,
                            Latitude = 52.23484,
                            Longitude = -0.89732,
                            Postcode = "NN1 1PR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000018",
                            ApiLocationId = 200000067,
                            SubRegionName = "Nottingham",
                            Checked = false,
                            Latitude = 52.95512,
                            Longitude = -1.14917,
                            Postcode = "NG1 2AR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000024",
                            ApiLocationId = 200000068,
                            SubRegionName = "Nottinghamshire",
                            Checked = false,
                            Latitude = 52.95512,
                            Longitude = -1.14917,
                            Postcode = "NG1 2AR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000017",
                            ApiLocationId = 200000069,
                            SubRegionName = "Rutland",
                            Checked = false,
                            Latitude = 52.6583,
                            Longitude = -0.6396,
                            Postcode = "LE15 8TH"
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
                    Postcode = "B9 5BP", 
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000019",
                            ApiLocationId = 200000070,
                            SubRegionName = "Herefordshire",
                            Checked = false,
                            Latitude = 52.0765,
                            Longitude = -2.6544,
                            Postcode = "HR4 8DR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000020",
                            ApiLocationId = 200000071,
                            SubRegionName = "Telford and Wrekin",
                            Checked = false,
                            Latitude = 52.7410,
                            Longitude = -2.4869,
                            Postcode = "TF6 5AL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000021",
                            ApiLocationId = 200000072,
                            SubRegionName = "Stoke-on-Trent",
                            Checked = false,
                            Latitude = 53.02578,
                            Longitude = -2.177394,
                            Postcode = "ST1 5NA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000051",
                            ApiLocationId = 200000073,
                            SubRegionName = "Shropshire",
                            Checked = false,
                            Latitude = 52.67587,
                            Longitude = -2.4497,
                            Postcode = "TF3 4BS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000025",
                            ApiLocationId = 200000074,
                            SubRegionName = "Birmingham",
                            Checked = false,
                            Latitude = 52.4829,
                            Longitude = -1.89346,
                            Postcode = "B4 6UD"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000026",
                            ApiLocationId = 200000075,
                            SubRegionName = "Coventry",
                            Checked = false,
                            Latitude = 52.40631,
                            Longitude = -1.50852,
                            Postcode = "CV1 2JZ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000027",
                            ApiLocationId = 200000076,
                            SubRegionName = "Dudley",
                            Checked = false,
                            Latitude = 52.50867,
                            Longitude = -2.08734,
                            Postcode = "DY1 1QD"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000028",
                            ApiLocationId = 200000077,
                            SubRegionName = "Sandwell",
                            Checked = false,
                            Latitude = 52.50636,
                            Longitude = -1.96258,
                            Postcode = "B71 4LF"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000029",
                            ApiLocationId = 200000078,
                            SubRegionName = "Solihull",
                            Checked = false,
                            Latitude = 52.41471,
                            Longitude = -1.7743,
                            Postcode = "B91 3DA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000030",
                            ApiLocationId = 200000079,
                            SubRegionName = "Walsall",
                            Checked = false,
                            Latitude = 52.58595,
                            Longitude = -1.98229,
                            Postcode = "WS1 1XR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E08000031",
                            ApiLocationId = 200000080,
                            SubRegionName = "Wolverhampton",
                            Checked = false,
                            Latitude = 52.58533,
                            Longitude = -2.13192,
                            Postcode = "WV1 4EX"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000028",
                            ApiLocationId = 200000081,
                            SubRegionName = "Staffordshire",
                            Checked = false,
                            Latitude = 52.8793,
                            Longitude = -2.0572,
                            Postcode = "ST18 0LA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000031",
                            ApiLocationId = 200000082,
                            SubRegionName = "Warwickshire",
                            Checked = false,
                            Latitude = 52.28194,
                            Longitude = -1.58447,
                            Postcode = "CV34 4HJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000034",
                            ApiLocationId = 200000083,
                            SubRegionName = "Worcestershire",
                            Checked = false,
                            Latitude = 52.19204,
                            Longitude = -2.22353,
                            Postcode = "WR1 2JJ"
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
                    Postcode = "PE2 6XE",
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000055",
                            ApiLocationId = 200000084,
                            SubRegionName = "Bedford",
                            Checked = false,
                            Latitude = 52.13571,
                            Longitude = -0.46804,
                            Postcode = "MK40 1AN"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000003",
                            ApiLocationId = 200000085,
                            SubRegionName = "Cambridgeshire",
                            Checked = false,
                            Latitude = 52.57339,
                            Longitude = -0.24846,
                            Postcode = "PE28 3PP"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000056",
                            ApiLocationId = 200000086,
                            SubRegionName = "Central Bedfordshire",
                            Checked = false,
                            Latitude = 52.00268,
                            Longitude = -0.29749,
                            Postcode = "MK45 4EB"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000012",
                            ApiLocationId = 200000087,
                            SubRegionName = "Essex",
                            Checked = false,
                            Latitude = 51.5742,
                            Longitude = -0.4857,
                            Postcode = "CM3 2BY"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000015",
                            ApiLocationId = 200000088,
                            SubRegionName = "Hertfordshire",
                            Checked = false,
                            Latitude = 51.8098,
                            Longitude = -0.2377,
                            Postcode = "AL8 7BP"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000032",
                            ApiLocationId = 200000089,
                            SubRegionName = "Luton",
                            Checked = false,
                            Latitude = 51.87965,
                            Longitude = -0.41756,
                            Postcode = "LU1 2SQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000020",
                            ApiLocationId = 200000090,
                            SubRegionName = "Norfolk",
                            Checked = false,
                            Latitude = 52.6140,
                            Longitude = 0.8864,
                            Postcode = "NR19 1ED"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000031",
                            ApiLocationId = 200000158,
                            SubRegionName = "Peterborough",
                            Checked = false,
                            Latitude = 52.57339,
                            Longitude = -0.24846,
                            Postcode = "PE1 1QL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000033",
                            ApiLocationId = 200000159,
                            SubRegionName = "Southend-on-Sea",
                            Checked = false,
                            Latitude = 51.54041,
                            Longitude = 0.7077,
                            Postcode = "SS2 5SR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000029",
                            ApiLocationId = 200000160,
                            SubRegionName = "Suffolk",
                            Checked = false,
                            Latitude = 52.1872, 
                            Longitude = 0.9708,
                            Postcode = "IP14 4DA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000034",
                            ApiLocationId = 200000161,
                            SubRegionName = "Thurrock",
                            Checked = false,
                            Latitude = 51.4935,
                            Longitude = 0.3529,
                            Postcode = "RM16 3EU"
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
                    Postcode = "WC2N 5DU",
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E09000001",
                            ApiLocationId = 200000091,
                            SubRegionName = "City of London",
                            Checked = false,
                            Latitude = 51.5074,
                            Longitude = -0.1278,
                            Postcode = "WC2N 5DU"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000002",
                            ApiLocationId = 200000092,
                            SubRegionName = "Barking and Dagenham",
                            Checked = false,
                            Latitude = 51.53628,
                            Longitude = 0.08148,
                            Postcode = "IG11 7PG"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000003",
                            ApiLocationId = 200000093,
                            SubRegionName = "Barnet",
                            Checked = false,
                            Latitude = 51.65293,
                            Longitude = -0.19961,
                            Postcode = "CM14 5AY"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000004",
                            ApiLocationId = 200000094,
                            SubRegionName = "Bexley",
                            Checked = false,
                            Latitude = 51.44135,
                            Longitude = 0.14861,
                            Postcode = "DA5 1AH"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000005",
                            ApiLocationId = 200000095,
                            SubRegionName = "Brent",
                            Checked = false,
                            Latitude = 51.5673,
                            Longitude = -0.2711,
                            Postcode = "HA9 0TF"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000006",
                            ApiLocationId = 200000096,
                            SubRegionName = "Bromley",
                            Checked = false,
                            Latitude = 51.40568,
                            Longitude = 0.01435,
                            Postcode = "BR1 1LB"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000007",
                            ApiLocationId = 200000097,
                            SubRegionName = "Camden",
                            Checked = false,
                            Latitude = 51.5517,
                            Longitude = -0.1588,
                            Postcode = "NW3 2YE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000008",
                            ApiLocationId = 200000098,
                            SubRegionName = "Croydon",
                            Checked = false,
                            Latitude = 51.37236,
                            Longitude = -0.0982,
                            Postcode = "CR0 1QB"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000009",
                            ApiLocationId = 200000099,
                            SubRegionName = "Ealing",
                            Checked = false,
                            Latitude = 51.5133,
                            Longitude = -0.3043,
                            Postcode = "W13 0DH"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000010",
                            ApiLocationId = 200000100,
                            SubRegionName = "Enfield",
                            Checked = false,
                            Latitude = 51.6521,
                            Longitude = -0.08153,
                            Postcode = "EN2 6LS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000011",
                            ApiLocationId = 200000101,
                            SubRegionName = "Greenwich",
                            Checked = false,
                            Latitude = 51.4934,
                            Longitude = 0.0098,
                            Postcode = "SE10 9AH"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000012",
                            ApiLocationId = 200000102,
                            SubRegionName = "Hackney",
                            Checked = false,
                            Latitude = 51.5734,
                            Longitude = -0.0724,
                            Postcode = "E5 8JY"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000013",
                            ApiLocationId = 200000103,
                            SubRegionName = "Hammersmith and Fulham",
                            Checked = false,
                            Latitude = 51.4990,
                            Longitude = -0.2291,
                            Postcode = "SW6 5UJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000014",
                            ApiLocationId = 200000104,
                            SubRegionName = "Haringey",
                            Checked = false,
                            Latitude = 51.5906,
                            Longitude = -0.1110,
                            Postcode = "N8 0EE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000015",
                            ApiLocationId = 200000105,
                            SubRegionName = "Harrow",
                            Checked = false,
                            Latitude = 51.57881,
                            Longitude = -0.33376,
                            Postcode = "HA1 2AT"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000016",
                            ApiLocationId = 200000106,
                            SubRegionName = "Havering",
                            Checked = false,
                            Latitude = 51.5779,
                            Longitude =  0.2121,
                            Postcode = "RM4 1PL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000017",
                            ApiLocationId = 200000107,
                            SubRegionName = "Hillingdon",
                            Checked = false,
                            Latitude = 51.53358,
                            Longitude = -0.45258,
                            Postcode = "UB10 0EG"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000018",
                            ApiLocationId = 200000108,
                            SubRegionName = "Hounslow",
                            Checked = false,
                            Latitude = 51.46759,
                            Longitude = -0.3618,
                            Postcode = "UB6 9RG"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000019",
                            ApiLocationId = 200000109,
                            SubRegionName = "Islington",
                            Checked = false,
                            Latitude = 51.5465,
                            Longitude = -0.1058,
                            Postcode = "N7 8JE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000020",
                            ApiLocationId = 200000110,
                            SubRegionName = "Kensington and Chelsea",
                            Checked = false,
                            Latitude = 51.4991,
                            Longitude = -0.1938,
                            Postcode = "W8 7HA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000021",
                            ApiLocationId = 200000111,
                            SubRegionName = "Kingston upon Thames",
                            Checked = false,
                            Latitude = 51.41232,
                            Longitude = -0.30044,
                            Postcode = "KT1 1UJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000022",
                            ApiLocationId = 200000112,
                            SubRegionName = "Lambeth",
                            Checked = false,
                            Latitude = 51.4571, 
                            Longitude = -0.1231,
                            Postcode = "SW2 1QN"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000023",
                            ApiLocationId = 200000113,
                            SubRegionName = "Lewisham",
                            Checked = false,
                            Latitude = 51.4415,
                            Longitude = -0.0117,
                            Postcode = "SE6 2JZ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000024",
                            ApiLocationId = 200000114,
                            SubRegionName = "Merton",
                            Checked = false,
                            Latitude = 51.4098,
                            Longitude = -0.2108,
                            Postcode = "SW19 3HD"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000025",
                            ApiLocationId = 200000115,
                            SubRegionName = "Newham",
                            Checked = false,
                            Latitude = 51.5255,
                            Longitude = 0.0352,
                            Postcode = "E13 9LB"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000026",
                            ApiLocationId = 200000116,
                            SubRegionName = "Redbridge",
                            Checked = false,
                            Latitude = 51.5901,
                            Longitude = 0.0819,
                            Postcode = "IG6 1HJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000027",
                            ApiLocationId = 200000117,
                            SubRegionName = "Richmond upon Thames",
                            Checked = false,
                            Latitude = 51.4613,
                            Longitude = -0.30044,
                            Postcode = "KT1 1UJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000028",
                            ApiLocationId = 200000118,
                            SubRegionName = "Southwark",
                            Checked = false,
                            Latitude = 51.5028,
                            Longitude = -0.0877,
                            Postcode = "SE1 1PF"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000029",
                            ApiLocationId = 200000119,
                            SubRegionName = "Sutton",
                            Checked = false,
                            Latitude = 51.36045,
                            Longitude = -0.19178,
                            Postcode = "SM1 1DE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000030",
                            ApiLocationId = 200000120,
                            SubRegionName = "Tower Hamlets",
                            Checked = false,
                            Latitude = 51.5203,
                            Longitude = -0.0293,
                            Postcode = " E1 4SD"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000031",
                            ApiLocationId = 200000121,
                            SubRegionName = "Waltham Forest",
                            Checked = false,
                            Latitude = 51.5886,
                            Longitude = -0.0118,
                            Postcode = "E17 4LS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000032",
                            ApiLocationId = 200000122,
                            SubRegionName = "Wandsworth",
                            Checked = false,
                            Latitude = 51.4571,
                            Longitude = -0.1818,
                            Postcode = "SW18 4DJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E09000033",
                            ApiLocationId = 200000123,
                            SubRegionName = "Westminster",
                            Checked = false,
                            Latitude = 51.4975,
                            Longitude = -0.1357,
                            Postcode = "W1H 7EJ"
                        }
                    }
                },
                new RegionItemModel
                {
                    Id = "E12000008",
                    ApiLocationId = 200000008,
                    Checked = false,
                    RegionName = "South East",
                    Latitude = 51.5074,
                    Longitude = -0.1278,
                    Postcode = "WC2N 5DU",
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000035",
                            ApiLocationId = 200000124,
                            SubRegionName = "Medway",
                            Checked = false,
                            Latitude = 51.4047,
                            Longitude = 0.5418,
                            Postcode = "ME3 9AA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000036",
                            ApiLocationId = 200000125,
                            SubRegionName = "Bracknell Forest",
                            Checked = false,
                            Latitude = 51.4154,
                            Longitude = -0.7536,
                            Postcode = "RG12 1LS"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000037",
                            ApiLocationId = 200000126,
                            SubRegionName = "West Berkshire",
                            Checked = false,
                            Latitude = 51.4308,
                            Longitude = -1.1445,
                            Postcode = "RG20 8UR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000038",
                            ApiLocationId = 200000127,
                            SubRegionName = "Reading",
                            Checked = false,
                            Latitude = 51.45504,
                            Longitude = -0.9781,
                            Postcode = "RG1 2EA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000039",
                            ApiLocationId = 200000128,
                            SubRegionName = "Slough",
                            Checked = false,
                            Latitude = 51.50935,
                            Longitude = -0.59545,
                            Postcode = "SL1 1EL"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000040",
                            ApiLocationId = 200000129,
                            SubRegionName = "Windsor and Maidenhead",
                            Checked = false,
                            Latitude = 51.48467,
                            Longitude = -0.64786,
                            Postcode = "SL4 5SE"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000041",
                            ApiLocationId = 200000130,
                            SubRegionName = "Wokingham",
                            Checked = false,
                            Latitude = 51.41097,
                            Longitude = -0.83493,
                            Postcode = "RG40 1BW"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000042",
                            ApiLocationId = 200000131,
                            SubRegionName = "Milton Keynes",
                            Checked = false,
                            Latitude = 52.04144,
                            Longitude = -0.76056,
                            Postcode = "MK9 2ES"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000043",
                            ApiLocationId = 200000132,
                            SubRegionName = "Brighton and Hove",
                            Checked = false,
                            Latitude = 50.83022,
                            Longitude = -0.1372,
                            Postcode = "BN3 3JG"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000044",
                            ApiLocationId = 200000133,
                            SubRegionName = "Portsmouth",
                            Checked = false,
                            Latitude = 50.79891,
                            Longitude = -1.09116,
                            Postcode = "PO1 1EJ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000045",
                            ApiLocationId = 200000134,
                            SubRegionName = "Southampton",
                            Checked = false,
                            Latitude = 50.90497,
                            Longitude = -1.40323,
                            Postcode = "SO4O 3PT"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000046",
                            ApiLocationId = 200000135,
                            SubRegionName = "Isle of Wight",
                            Checked = false,
                            Latitude = 50.6938,
                            Longitude = -1.3047,
                            Postcode = "PO30 1NR"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000002",
                            ApiLocationId = 200000136,
                            SubRegionName = "Buckinghamshire",
                            Checked = false,
                            Latitude = 51.8137,
                            Longitude = -0.8095,
                            Postcode = "MK9 2ES"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000011",
                            ApiLocationId = 200000137,
                            SubRegionName = "East Sussex",
                            Checked = false,
                            Latitude = 50.9086,
                            Longitude = -0.2494,
                            Postcode = "BN2 1TW"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000014",
                            ApiLocationId = 200000138,
                            SubRegionName = "Hampshire",
                            Checked = false,
                            Latitude = 51.0577,
                            Longitude = -1.3081,
                            Postcode = "SO23 9LH"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000016",
                            ApiLocationId = 200000139,
                            SubRegionName = "Kent",
                            Checked = false,
                            Latitude = 51.2787,
                            Longitude = 0.5217,
                            Postcode = "TN27 0AU"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000025",
                            ApiLocationId = 200000140,
                            SubRegionName = "Oxfordshire",
                            Checked = false,
                            Latitude = 51.75374,
                            Longitude = -1.26346,
                            Postcode = "OX1 2AW"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000030",
                            ApiLocationId = 200000141,
                            SubRegionName = "Surrey",
                            Checked = false,
                            Latitude = 51.3148,
                            Longitude = -0.5600,
                            Postcode = "KT24 6AH"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000032",
                            ApiLocationId = 200000142,
                            SubRegionName = "West Sussex",
                            Checked = false,
                            Latitude = 50.83664,
                            Longitude = -0.4617,
                            Postcode = "RH20 2BJ"
                        }
                    }
                },
                new RegionItemModel
                {
                    Id = "E12000009",
                    ApiLocationId = 200000009,
                    Checked = false,
                    RegionName = "South West",
                    Latitude = 50.7772,
                    Longitude = -3.999461,
                    Postcode = "EX20 3BD",
                    SubRegion = new List<SubRegionItemModel>
                    {
                        new SubRegionItemModel
                        {
                            Id = "E06000022",
                            ApiLocationId = 200000143,
                            SubRegionName = "Bath and North East Somerset",
                            Checked = false,
                            Latitude = 51.34762,
                            Longitude = -2.4766,
                            Postcode = "BS23 1SQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000023",
                            ApiLocationId = 200000144,
                            SubRegionName = "Bristol",
                            Checked = false,
                            Latitude = 51.4545,
                            Longitude = -2.5879,
                            Postcode = "BS1 2DP"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000024",
                            ApiLocationId = 200000145,
                            SubRegionName = "North Somerset",
                            Checked = false,
                            Latitude = 51.3879,
                            Longitude = -2.7781,
                            Postcode = "BS49 4EG"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000025",
                            ApiLocationId = 200000146,
                            SubRegionName = "South Gloucestershire",
                            Checked = false,
                            Latitude = 51.5264,
                            Longitude = -2.4728,
                            Postcode = "BS23 1SQ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000026",
                            ApiLocationId = 200000147,
                            SubRegionName = "Plymouth",
                            Checked = false,
                            Latitude = 50.37038,
                            Longitude = -4.14265,
                            Postcode = "PL1 1HH"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000027",
                            ApiLocationId = 200000148,
                            SubRegionName = "Torbay",
                            Checked = false,
                            Latitude = 50.4619,
                            Longitude = -3.5253,
                            Postcode = "TQ4 6AA"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000028",
                            ApiLocationId = 200000162,
                            SubRegionName = "Bournemouth",
                            Checked = false,
                            Latitude = 50.72168,
                            Longitude = -1.87853,
                            Postcode = "BH2 6EP"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000029",
                            ApiLocationId = 200000149,
                            SubRegionName = "Poole",
                            Checked = false,
                            Latitude = 50.71939,
                            Longitude = -1.98114,
                            Postcode = "BH15 1SX"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000030",
                            ApiLocationId = 200000150,
                            SubRegionName = "Swindon",
                            Checked = false,
                            Latitude = 51.55842,
                            Longitude = -1.78204,
                            Postcode = "SN1 1PZ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000052",
                            ApiLocationId = 200000151,
                            SubRegionName = "Cornwall",
                            Checked = false,
                            Latitude = 50.2660,
                            Longitude = -5.0527,
                            Postcode = "PL30 5HF"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000053",
                            ApiLocationId = 200000152,
                            SubRegionName = "Isles of Scilly",
                            Checked = false,
                            Latitude = 49.9146,
                            Longitude = -6.31574,
                            Postcode = "TR21 0LN"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E06000054",
                            ApiLocationId = 200000153,
                            SubRegionName = "Wiltshire",
                            Checked = false,
                            Latitude = 51.3492,
                            Longitude = -1.9927,
                            Postcode = "SN10 4SW"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000008",
                            ApiLocationId = 200000154,
                            SubRegionName = "Devon",
                            Checked = false,
                            Latitude = 50.7156,
                            Longitude = -3.5309,
                            Postcode = "EX17 6DB"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000009",
                            ApiLocationId = 200000155,
                            SubRegionName = "Dorset",
                            Checked = false,
                            Latitude = 50.7488,
                            Longitude = -2.3445,
                            Postcode = "DT11 0BX"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000013",
                            ApiLocationId = 200000156,
                            SubRegionName = "Gloucestershire",
                            Checked = false,
                            Latitude = 51.86674,
                            Longitude = -2.24867,
                            Postcode = "GL1 2NZ"
                        },
                        new SubRegionItemModel
                        {
                            Id = "E10000027",
                            ApiLocationId = 200000157,
                            SubRegionName = "Somerset",
                            Checked = false,
                            Latitude = 51.1051,
                            Longitude = -2.9262,
                            Postcode = "TA7 0LZ"
                        }
                    }
                }
            }; 
        }
    }
}