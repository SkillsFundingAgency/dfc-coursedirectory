using System.Collections.Generic;

namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class RegionInfo
    {
        public static IReadOnlyCollection<RegionInfo> All { get; } = new List<RegionInfo>()
        {
            new RegionInfo
            {
                Id = "E12000001",
                Name = "North East",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E06000001",
                        Name = "County Durham",
                    },
                    new RegionInfo
                    {
                        Id = "E06000002",
                        Name = "Darlington",
                    },
                    new RegionInfo
                    {
                        Id = "E06000003",
                        Name = "Gateshead",
                    },
                    new RegionInfo
                    {
                        Id = "E06000004",
                        Name = "Hartlepool",
                    },
                    new RegionInfo
                    {
                        Id = "E06000005",
                        Name = "Middlesbrough",
                    },
                    new RegionInfo
                    {
                        Id = "E06000047",
                        Name = "Newcastle upon Tyne",
                    },
                    new RegionInfo
                    {
                        Id = "E06000057",
                        Name = "North Tyneside",
                    },
                    new RegionInfo
                    {
                        Id = "E08000021",
                        Name = "Northumberland",
                    },
                    new RegionInfo
                    {
                        Id = "E08000022",
                        Name = "Redcar and Cleveland",
                    },
                    new RegionInfo
                    {
                        Id = "E08000023",
                        Name = "South Tyneside",
                    },
                    new RegionInfo
                    {
                        Id = "E08000024",
                        Name = "Stockton-on-Tees",
                    },
                    new RegionInfo
                    {
                        Id = "E08000037",
                        Name = "Sunderland",
                    }
                }
            },
            new RegionInfo
            {
                Id = "E12000002",
                Name = "North West",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E06000006",
                        Name = "Halton",
                    },
                    new RegionInfo
                    {
                        Id = "E06000007",
                        Name = "Warrington",
                    },
                    new RegionInfo
                    {
                        Id = "E06000008",
                        Name = "Blackburn with Darwen",
                    },
                    new RegionInfo
                    {
                        Id = "E06000009",
                        Name = "Blackpool",
                    },
                    new RegionInfo
                    {
                        Id = "E06000049",
                        Name = "Cheshire East",
                    },
                    new RegionInfo
                    {
                        Id = "E06000050",
                        Name = "Cheshire West and Chester",
                    },
                    new RegionInfo
                    {
                        Id = "E08000001",
                        Name = "Bolton",
                    },
                    new RegionInfo
                    {
                        Id = "E08000002",
                        Name = "Bury",
                    },
                    new RegionInfo
                    {
                        Id = "E08000003",
                        Name = "Manchester",
                    },
                    new RegionInfo
                    {
                        Id = "E08000004",
                        Name = "Oldham",
                    },
                    new RegionInfo
                    {
                        Id = "E08000005",
                        Name = "Rochdale",
                    },
                    new RegionInfo
                    {
                        Id = "E08000006",
                        Name = "Salford",
                    },
                    new RegionInfo
                    {
                        Id = "E08000007",
                        Name = "Stockport",
                    },
                    new RegionInfo
                    {
                        Id = "E08000008",
                        Name = "Tameside",
                    },
                    new RegionInfo
                    {
                        Id = "E08000009",
                        Name = "Trafford",
                    },
                    new RegionInfo
                    {
                        Id = "E08000010",
                        Name = "Wigan",
                    },
                    new RegionInfo
                    {
                        Id = "E08000011",
                        Name = "Knowsley",
                    },
                    new RegionInfo
                    {
                        Id = "E08000012",
                        Name = "Liverpool",
                    },
                    new RegionInfo
                    {
                        Id = "E08000013",
                        Name = "St Helens",
                    },
                    new RegionInfo
                    {
                        Id = "E08000014",
                        Name = "Sefton",
                    },
                    new RegionInfo
                    {
                        Id = "E08000015",
                        Name = "Wirral",
                    },
                    new RegionInfo
                    {
                        Id = "E10000006",
                        Name = "Cumbria",
                    },
                    new RegionInfo
                    {
                        Id = "E10000017",
                        Name = "Lancashire",
                    }
                }
            },
            new RegionInfo
            {
                Id = "E12000003",
                Name = "Yorkshire and The Humber",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E06000010",
                        Name = "Kingston upon Hull",
                    },
                    new RegionInfo
                    {
                        Id = "E06000011",
                        Name = "East Riding of Yorkshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000012",
                        Name = "North East Lincolnshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000013",
                        Name = "North Lincolnshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000014",
                        Name = "York",
                    },
                    new RegionInfo
                    {
                        Id = "E08000016",
                        Name = "Barnsley",
                    },
                    new RegionInfo
                    {
                        Id = "E08000017",
                        Name = "Doncaster",
                    },
                    new RegionInfo
                    {
                        Id = "E08000018",
                        Name = "Rotherham",
                    },
                    new RegionInfo
                    {
                        Id = "E08000019",
                        Name = "Sheffield",
                    },
                    new RegionInfo
                    {
                        Id = "E08000032",
                        Name = "Bradford",
                    },
                    new RegionInfo
                    {
                        Id = "E08000033",
                        Name = "Calderdale",
                    },
                    new RegionInfo
                    {
                        Id = "E08000034",
                        Name = "Kirklees",
                    },
                    new RegionInfo
                    {
                        Id = "E08000035",
                        Name = "Leeds",
                    },
                    new RegionInfo
                    {
                        Id = "E08000036",
                        Name = "Wakefield",
                    },
                    new RegionInfo
                    {
                        Id = "E10000023",
                        Name = "North Yorkshire",
                    }
                }
            },
            new RegionInfo
            {
                Id = "E12000004",
                Name = "East Midlands",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E06000015",
                        Name = "Derby",
                    },
                    new RegionInfo
                    {
                        Id = "E10000007",
                        Name = "Derbyshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000016",
                        Name = "Leicester",
                    },
                    new RegionInfo
                    {
                        Id = "E10000018",
                        Name = "Leicestershire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000019",
                        Name = "Lincolnshire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000021",
                        Name = "Northamptonshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000018",
                        Name = "Nottingham",
                    },
                    new RegionInfo
                    {
                        Id = "E10000024",
                        Name = "Nottinghamshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000017",
                        Name = "Rutland",
                    }
                }
            },
            new RegionInfo
            {
                Id = "E12000005",
                Name = "West Midlands",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E06000019",
                        Name = "Herefordshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000020",
                        Name = "Telford and Wrekin",
                    },
                    new RegionInfo
                    {
                        Id = "E06000021",
                        Name = "Stoke-on-Trent",
                    },
                    new RegionInfo
                    {
                        Id = "E06000051",
                        Name = "Shropshire",
                    },
                    new RegionInfo
                    {
                        Id = "E08000025",
                        Name = "Birmingham",
                    },
                    new RegionInfo
                    {
                        Id = "E08000026",
                        Name = "Coventry",
                    },
                    new RegionInfo
                    {
                        Id = "E08000027",
                        Name = "Dudley",
                    },
                    new RegionInfo
                    {
                        Id = "E08000028",
                        Name = "Sandwell",
                    },
                    new RegionInfo
                    {
                        Id = "E08000029",
                        Name = "Solihull",
                    },
                    new RegionInfo
                    {
                        Id = "E08000030",
                        Name = "Walsall",
                    },
                    new RegionInfo
                    {
                        Id = "E08000031",
                        Name = "Wolverhampton",
                    },
                    new RegionInfo
                    {
                        Id = "E10000028",
                        Name = "Staffordshire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000031",
                        Name = "Warwickshire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000034",
                        Name = "Worcestershire",
                    },
                }
            },
            new RegionInfo
            {
                Id = "E12000006",
                Name = "East of England",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E06000055",
                        Name = "Bedford",
                    },
                    new RegionInfo
                    {
                        Id = "E10000003",
                        Name = "Cambridgeshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000056",
                        Name = "Central Bedfordshire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000012",
                        Name = "Essex",
                    },
                    new RegionInfo
                    {
                        Id = "E10000015",
                        Name = "Hertfordshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000032",
                        Name = "Luton",
                    },
                    new RegionInfo
                    {
                        Id = "E10000020",
                        Name = "Norfolk",
                    },
                    new RegionInfo
                    {
                        Id = "E06000031",
                        Name = "Peterborough",
                    },
                    new RegionInfo
                    {
                        Id = "E06000033",
                        Name = "Southend-on-Sea",
                    },
                    new RegionInfo
                    {
                        Id = "E10000029",
                        Name = "Suffolk",
                    },
                    new RegionInfo
                    {
                        Id = "E06000034",
                        Name = "Thurrock",
                    }
                }
            },
            new RegionInfo
            {
                Id = "E12000007",
                Name = "London",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E09000001",
                        Name = "City of London",
                    },
                    new RegionInfo
                    {
                        Id = "E09000002",
                        Name = "Barking and Dagenham",
                    },
                    new RegionInfo
                    {
                        Id = "E09000003",
                        Name = "Barnet",
                    },
                    new RegionInfo
                    {
                        Id = "E09000004",
                        Name = "Bexley",
                    },
                    new RegionInfo
                    {
                        Id = "E09000005",
                        Name = "Brent",
                    },
                    new RegionInfo
                    {
                        Id = "E09000006",
                        Name = "Bromley",
                    },
                    new RegionInfo
                    {
                        Id = "E09000007",
                        Name = "Camden",
                    },
                    new RegionInfo
                    {
                        Id = "E09000008",
                        Name = "Croydon",
                    },
                    new RegionInfo
                    {
                        Id = "E09000009",
                        Name = "Ealing",
                    },
                    new RegionInfo
                    {
                        Id = "E09000010",
                        Name = "Enfield",
                    },
                    new RegionInfo
                    {
                        Id = "E09000011",
                        Name = "Greenwich",
                    },
                    new RegionInfo
                    {
                        Id = "E09000012",
                        Name = "Hackney",
                    },
                    new RegionInfo
                    {
                        Id = "E09000013",
                        Name = "Hammersmith and Fulham",
                    },
                    new RegionInfo
                    {
                        Id = "E09000014",
                        Name = "Haringey",
                    },
                    new RegionInfo
                    {
                        Id = "E09000015",
                        Name = "Harrow",
                    },
                    new RegionInfo
                    {
                        Id = "E09000016",
                        Name = "Havering",
                    },
                    new RegionInfo
                    {
                        Id = "E09000017",
                        Name = "Hillingdon",
                    },
                    new RegionInfo
                    {
                        Id = "E09000018",
                        Name = "Hounslow",
                    },
                    new RegionInfo
                    {
                        Id = "E09000019",
                        Name = "Islington",
                    },
                    new RegionInfo
                    {
                        Id = "E09000020",
                        Name = "Kensington and Chelsea",
                    },
                    new RegionInfo
                    {
                        Id = "E09000021",
                        Name = "Kingston upon Thames",
                    },
                    new RegionInfo
                    {
                        Id = "E09000022",
                        Name = "Lambeth",
                    },
                    new RegionInfo
                    {
                        Id = "E09000023",
                        Name = "Lewisham",
                    },
                    new RegionInfo
                    {
                        Id = "E09000024",
                        Name = "Merton",
                    },
                    new RegionInfo
                    {
                        Id = "E09000025",
                        Name = "Newham",
                    },
                    new RegionInfo
                    {
                        Id = "E09000026",
                        Name = "Redbridge",
                    },
                    new RegionInfo
                    {
                        Id = "E09000027",
                        Name = "Richmond upon Thames",
                    },
                    new RegionInfo
                    {
                        Id = "E09000028",
                        Name = "Southwark",
                    },
                    new RegionInfo
                    {
                        Id = "E09000029",
                        Name = "Sutton",
                    },
                    new RegionInfo
                    {
                        Id = "E09000030",
                        Name = "Tower Hamlets",
                    },
                    new RegionInfo
                    {
                        Id = "E09000031",
                        Name = "Waltham Forest",
                    },
                    new RegionInfo
                    {
                        Id = "E09000032",
                        Name = "Wandsworth",
                    },
                    new RegionInfo
                    {
                        Id = "E09000033",
                        Name = "Westminster",
                    }
                }
            },
            new RegionInfo
            {
                Id = "E12000008",
                Name = "South East",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E06000035",
                        Name = "Medway",
                    },
                    new RegionInfo
                    {
                        Id = "E06000036",
                        Name = "Bracknell Forest",
                    },
                    new RegionInfo
                    {
                        Id = "E06000037",
                        Name = "West Berkshire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000038",
                        Name = "Reading",
                    },
                    new RegionInfo
                    {
                        Id = "E06000039",
                        Name = "Slough",
                    },
                    new RegionInfo
                    {
                        Id = "E06000040",
                        Name = "Windsor and Maidenhead",
                    },
                    new RegionInfo
                    {
                        Id = "E06000041",
                        Name = "Wokingham",
                    },
                    new RegionInfo
                    {
                        Id = "E06000042",
                        Name = "Milton Keynes",
                    },
                    new RegionInfo
                    {
                        Id = "E06000043",
                        Name = "Brighton and Hove",
                    },
                    new RegionInfo
                    {
                        Id = "E06000044",
                        Name = "Portsmouth",
                    },
                    new RegionInfo
                    {
                        Id = "E06000045",
                        Name = "Southampton",
                    },
                    new RegionInfo
                    {
                        Id = "E06000046",
                        Name = "Isle of Wight",
                    },
                    new RegionInfo
                    {
                        Id = "E10000002",
                        Name = "Buckinghamshire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000011",
                        Name = "East Sussex",
                    },
                    new RegionInfo
                    {
                        Id = "E10000014",
                        Name = "Hampshire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000016",
                        Name = "Kent",
                    },
                    new RegionInfo
                    {
                        Id = "E10000025",
                        Name = "Oxfordshire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000030",
                        Name = "Surrey",
                    },
                    new RegionInfo
                    {
                        Id = "E10000032",
                        Name = "West Sussex",
                    }
                }
            },
            new RegionInfo
            {
                Id = "E12000009",
                Name = "South West",
                SubRegions = new List<RegionInfo>
                {
                    new RegionInfo
                    {
                        Id = "E06000022",
                        Name = "Bath and North East Somerset",
                    },
                    new RegionInfo
                    {
                        Id = "E06000023",
                        Name = "Bristol",
                    },
                    new RegionInfo
                    {
                        Id = "E06000024",
                        Name = "North Somerset",
                    },
                    new RegionInfo
                    {
                        Id = "E06000025",
                        Name = "South Gloucestershire",
                    },
                    new RegionInfo
                    {
                        Id = "E06000026",
                        Name = "Plymouth",
                    },
                    new RegionInfo
                    {
                        Id = "E06000027",
                        Name = "Torbay",
                    },
                    new RegionInfo
                    {
                        Id = "E06000028",
                        Name = "Bournemouth",
                    },
                    new RegionInfo
                    {
                        Id = "E06000029",
                        Name = "Poole",
                    },
                    new RegionInfo
                    {
                        Id = "E06000030",
                        Name = "Swindon",
                    },
                    new RegionInfo
                    {
                        Id = "E06000052",
                        Name = "Cornwall",
                    },
                    new RegionInfo
                    {
                        Id = "E06000053",
                        Name = "Isles of Scilly",
                    },
                    new RegionInfo
                    {
                        Id = "E06000054",
                        Name = "Wiltshire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000008",
                        Name = "Devon",
                    },
                    new RegionInfo
                    {
                        Id = "E10000009",
                        Name = "Dorset",
                    },
                    new RegionInfo
                    {
                        Id = "E10000013",
                        Name = "Gloucestershire",
                    },
                    new RegionInfo
                    {
                        Id = "E10000027",
                        Name = "Somerset",
                    }
                }
            }
        };

        public string Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<RegionInfo> SubRegions { get; set; }
    }
}
