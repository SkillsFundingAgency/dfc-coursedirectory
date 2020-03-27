using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.Models
{
    public class Region
    {
        public static IReadOnlyCollection<Region> All { get; } = new List<Region>()
        {
            new Region
            {
                Id = "E12000001",
                Name = "North East",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E06000001",
                        Name = "County Durham",
                    },
                    new Region
                    {
                        Id = "E06000002",
                        Name = "Darlington",
                    },
                    new Region
                    {
                        Id = "E06000003",
                        Name = "Gateshead",
                    },
                    new Region
                    {
                        Id = "E06000004",
                        Name = "Hartlepool",
                    },
                    new Region
                    {
                        Id = "E06000005",
                        Name = "Middlesbrough",
                    },
                    new Region
                    {
                        Id = "E06000047",
                        Name = "Newcastle upon Tyne",
                    },
                    new Region
                    {
                        Id = "E06000057",
                        Name = "North Tyneside",
                    },
                    new Region
                    {
                        Id = "E08000021",
                        Name = "Northumberland",
                    },
                    new Region
                    {
                        Id = "E08000022",
                        Name = "Redcar and Cleveland",
                    },
                    new Region
                    {
                        Id = "E08000023",
                        Name = "South Tyneside",
                    },
                    new Region
                    {
                        Id = "E08000024",
                        Name = "Stockton-on-Tees",
                    },
                    new Region
                    {
                        Id = "E08000037",
                        Name = "Sunderland",
                    }
                }
            },
            new Region
            {
                Id = "E12000002",
                Name = "North West",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E06000006",
                        Name = "Halton",
                    },
                    new Region
                    {
                        Id = "E06000007",
                        Name = "Warrington",
                    },
                    new Region
                    {
                        Id = "E06000008",
                        Name = "Blackburn with Darwen",
                    },
                    new Region
                    {
                        Id = "E06000009",
                        Name = "Blackpool",
                    },
                    new Region
                    {
                        Id = "E06000049",
                        Name = "Cheshire East",
                    },
                    new Region
                    {
                        Id = "E06000050",
                        Name = "Cheshire West and Chester",
                    },
                    new Region
                    {
                        Id = "E08000001",
                        Name = "Bolton",
                    },
                    new Region
                    {
                        Id = "E08000002",
                        Name = "Bury",
                    },
                    new Region
                    {
                        Id = "E08000003",
                        Name = "Manchester",
                    },
                    new Region
                    {
                        Id = "E08000004",
                        Name = "Oldham",
                    },
                    new Region
                    {
                        Id = "E08000005",
                        Name = "Rochdale",
                    },
                    new Region
                    {
                        Id = "E08000006",
                        Name = "Salford",
                    },
                    new Region
                    {
                        Id = "E08000007",
                        Name = "Stockport",
                    },
                    new Region
                    {
                        Id = "E08000008",
                        Name = "Tameside",
                    },
                    new Region
                    {
                        Id = "E08000009",
                        Name = "Trafford",
                    },
                    new Region
                    {
                        Id = "E08000010",
                        Name = "Wigan",
                    },
                    new Region
                    {
                        Id = "E08000011",
                        Name = "Knowsley",
                    },
                    new Region
                    {
                        Id = "E08000012",
                        Name = "Liverpool",
                    },
                    new Region
                    {
                        Id = "E08000013",
                        Name = "St Helens",
                    },
                    new Region
                    {
                        Id = "E08000014",
                        Name = "Sefton",
                    },
                    new Region
                    {
                        Id = "E08000015",
                        Name = "Wirral",
                    },
                    new Region
                    {
                        Id = "E10000006",
                        Name = "Cumbria",
                    },
                    new Region
                    {
                        Id = "E10000017",
                        Name = "Lancashire",
                    }
                }
            },
            new Region
            {
                Id = "E12000003",
                Name = "Yorkshire and The Humber",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E06000010",
                        Name = "Kingston upon Hull",
                    },
                    new Region
                    {
                        Id = "E06000011",
                        Name = "East Riding of Yorkshire",
                    },
                    new Region
                    {
                        Id = "E06000012",
                        Name = "North East Lincolnshire",
                    },
                    new Region
                    {
                        Id = "E06000013",
                        Name = "North Lincolnshire",
                    },
                    new Region
                    {
                        Id = "E06000014",
                        Name = "York",
                    },
                    new Region
                    {
                        Id = "E08000016",
                        Name = "Barnsley",
                    },
                    new Region
                    {
                        Id = "E08000017",
                        Name = "Doncaster",
                    },
                    new Region
                    {
                        Id = "E08000018",
                        Name = "Rotherham",
                    },
                    new Region
                    {
                        Id = "E08000019",
                        Name = "Sheffield",
                    },
                    new Region
                    {
                        Id = "E08000032",
                        Name = "Bradford",
                    },
                    new Region
                    {
                        Id = "E08000033",
                        Name = "Calderdale",
                    },
                    new Region
                    {
                        Id = "E08000034",
                        Name = "Kirklees",
                    },
                    new Region
                    {
                        Id = "E08000035",
                        Name = "Leeds",
                    },
                    new Region
                    {
                        Id = "E08000036",
                        Name = "Wakefield",
                    },
                    new Region
                    {
                        Id = "E10000023",
                        Name = "North Yorkshire",
                    }
                }
            },
            new Region
            {
                Id = "E12000004",
                Name = "East Midlands",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E06000015",
                        Name = "Derby",
                    },
                    new Region
                    {
                        Id = "E10000007",
                        Name = "Derbyshire",
                    },
                    new Region
                    {
                        Id = "E06000016",
                        Name = "Leicester",
                    },
                    new Region
                    {
                        Id = "E10000018",
                        Name = "Leicestershire",
                    },
                    new Region
                    {
                        Id = "E10000019",
                        Name = "Lincolnshire",
                    },
                    new Region
                    {
                        Id = "E10000021",
                        Name = "Northamptonshire",
                    },
                    new Region
                    {
                        Id = "E06000018",
                        Name = "Nottingham",
                    },
                    new Region
                    {
                        Id = "E10000024",
                        Name = "Nottinghamshire",
                    },
                    new Region
                    {
                        Id = "E06000017",
                        Name = "Rutland",
                    }
                }
            },
            new Region
            {
                Id = "E12000005",
                Name = "West Midlands",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E06000019",
                        Name = "Herefordshire",
                    },
                    new Region
                    {
                        Id = "E06000020",
                        Name = "Telford and Wrekin",
                    },
                    new Region
                    {
                        Id = "E06000021",
                        Name = "Stoke-on-Trent",
                    },
                    new Region
                    {
                        Id = "E06000051",
                        Name = "Shropshire",
                    },
                    new Region
                    {
                        Id = "E08000025",
                        Name = "Birmingham",
                    },
                    new Region
                    {
                        Id = "E08000026",
                        Name = "Coventry",
                    },
                    new Region
                    {
                        Id = "E08000027",
                        Name = "Dudley",
                    },
                    new Region
                    {
                        Id = "E08000028",
                        Name = "Sandwell",
                    },
                    new Region
                    {
                        Id = "E08000029",
                        Name = "Solihull",
                    },
                    new Region
                    {
                        Id = "E08000030",
                        Name = "Walsall",
                    },
                    new Region
                    {
                        Id = "E08000031",
                        Name = "Wolverhampton",
                    },
                    new Region
                    {
                        Id = "E10000028",
                        Name = "Staffordshire",
                    },
                    new Region
                    {
                        Id = "E10000031",
                        Name = "Warwickshire",
                    },
                    new Region
                    {
                        Id = "E10000034",
                        Name = "Worcestershire",
                    },
                }
            },
            new Region
            {
                Id = "E12000006",
                Name = "East of England",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E06000055",
                        Name = "Bedford",
                    },
                    new Region
                    {
                        Id = "E10000003",
                        Name = "Cambridgeshire",
                    },
                    new Region
                    {
                        Id = "E06000056",
                        Name = "Central Bedfordshire",
                    },
                    new Region
                    {
                        Id = "E10000012",
                        Name = "Essex",
                    },
                    new Region
                    {
                        Id = "E10000015",
                        Name = "Hertfordshire",
                    },
                    new Region
                    {
                        Id = "E06000032",
                        Name = "Luton",
                    },
                    new Region
                    {
                        Id = "E10000020",
                        Name = "Norfolk",
                    },
                    new Region
                    {
                        Id = "E06000031",
                        Name = "Peterborough",
                    },
                    new Region
                    {
                        Id = "E06000033",
                        Name = "Southend-on-Sea",
                    },
                    new Region
                    {
                        Id = "E10000029",
                        Name = "Suffolk",
                    },
                    new Region
                    {
                        Id = "E06000034",
                        Name = "Thurrock",
                    }
                }
            },
            new Region
            {
                Id = "E12000007",
                Name = "London",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E09000001",
                        Name = "City of London",
                    },
                    new Region
                    {
                        Id = "E09000002",
                        Name = "Barking and Dagenham",
                    },
                    new Region
                    {
                        Id = "E09000003",
                        Name = "Barnet",
                    },
                    new Region
                    {
                        Id = "E09000004",
                        Name = "Bexley",
                    },
                    new Region
                    {
                        Id = "E09000005",
                        Name = "Brent",
                    },
                    new Region
                    {
                        Id = "E09000006",
                        Name = "Bromley",
                    },
                    new Region
                    {
                        Id = "E09000007",
                        Name = "Camden",
                    },
                    new Region
                    {
                        Id = "E09000008",
                        Name = "Croydon",
                    },
                    new Region
                    {
                        Id = "E09000009",
                        Name = "Ealing",
                    },
                    new Region
                    {
                        Id = "E09000010",
                        Name = "Enfield",
                    },
                    new Region
                    {
                        Id = "E09000011",
                        Name = "Greenwich",
                    },
                    new Region
                    {
                        Id = "E09000012",
                        Name = "Hackney",
                    },
                    new Region
                    {
                        Id = "E09000013",
                        Name = "Hammersmith and Fulham",
                    },
                    new Region
                    {
                        Id = "E09000014",
                        Name = "Haringey",
                    },
                    new Region
                    {
                        Id = "E09000015",
                        Name = "Harrow",
                    },
                    new Region
                    {
                        Id = "E09000016",
                        Name = "Havering",
                    },
                    new Region
                    {
                        Id = "E09000017",
                        Name = "Hillingdon",
                    },
                    new Region
                    {
                        Id = "E09000018",
                        Name = "Hounslow",
                    },
                    new Region
                    {
                        Id = "E09000019",
                        Name = "Islington",
                    },
                    new Region
                    {
                        Id = "E09000020",
                        Name = "Kensington and Chelsea",
                    },
                    new Region
                    {
                        Id = "E09000021",
                        Name = "Kingston upon Thames",
                    },
                    new Region
                    {
                        Id = "E09000022",
                        Name = "Lambeth",
                    },
                    new Region
                    {
                        Id = "E09000023",
                        Name = "Lewisham",
                    },
                    new Region
                    {
                        Id = "E09000024",
                        Name = "Merton",
                    },
                    new Region
                    {
                        Id = "E09000025",
                        Name = "Newham",
                    },
                    new Region
                    {
                        Id = "E09000026",
                        Name = "Redbridge",
                    },
                    new Region
                    {
                        Id = "E09000027",
                        Name = "Richmond upon Thames",
                    },
                    new Region
                    {
                        Id = "E09000028",
                        Name = "Southwark",
                    },
                    new Region
                    {
                        Id = "E09000029",
                        Name = "Sutton",
                    },
                    new Region
                    {
                        Id = "E09000030",
                        Name = "Tower Hamlets",
                    },
                    new Region
                    {
                        Id = "E09000031",
                        Name = "Waltham Forest",
                    },
                    new Region
                    {
                        Id = "E09000032",
                        Name = "Wandsworth",
                    },
                    new Region
                    {
                        Id = "E09000033",
                        Name = "Westminster",
                    }
                }
            },
            new Region
            {
                Id = "E12000008",
                Name = "South East",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E06000035",
                        Name = "Medway",
                    },
                    new Region
                    {
                        Id = "E06000036",
                        Name = "Bracknell Forest",
                    },
                    new Region
                    {
                        Id = "E06000037",
                        Name = "West Berkshire",
                    },
                    new Region
                    {
                        Id = "E06000038",
                        Name = "Reading",
                    },
                    new Region
                    {
                        Id = "E06000039",
                        Name = "Slough",
                    },
                    new Region
                    {
                        Id = "E06000040",
                        Name = "Windsor and Maidenhead",
                    },
                    new Region
                    {
                        Id = "E06000041",
                        Name = "Wokingham",
                    },
                    new Region
                    {
                        Id = "E06000042",
                        Name = "Milton Keynes",
                    },
                    new Region
                    {
                        Id = "E06000043",
                        Name = "Brighton and Hove",
                    },
                    new Region
                    {
                        Id = "E06000044",
                        Name = "Portsmouth",
                    },
                    new Region
                    {
                        Id = "E06000045",
                        Name = "Southampton",
                    },
                    new Region
                    {
                        Id = "E06000046",
                        Name = "Isle of Wight",
                    },
                    new Region
                    {
                        Id = "E10000002",
                        Name = "Buckinghamshire",
                    },
                    new Region
                    {
                        Id = "E10000011",
                        Name = "East Sussex",
                    },
                    new Region
                    {
                        Id = "E10000014",
                        Name = "Hampshire",
                    },
                    new Region
                    {
                        Id = "E10000016",
                        Name = "Kent",
                    },
                    new Region
                    {
                        Id = "E10000025",
                        Name = "Oxfordshire",
                    },
                    new Region
                    {
                        Id = "E10000030",
                        Name = "Surrey",
                    },
                    new Region
                    {
                        Id = "E10000032",
                        Name = "West Sussex",
                    }
                }
            },
            new Region
            {
                Id = "E12000009",
                Name = "South West",
                SubRegions = new List<Region>
                {
                    new Region
                    {
                        Id = "E06000022",
                        Name = "Bath and North East Somerset",
                    },
                    new Region
                    {
                        Id = "E06000023",
                        Name = "Bristol",
                    },
                    new Region
                    {
                        Id = "E06000024",
                        Name = "North Somerset",
                    },
                    new Region
                    {
                        Id = "E06000025",
                        Name = "South Gloucestershire",
                    },
                    new Region
                    {
                        Id = "E06000026",
                        Name = "Plymouth",
                    },
                    new Region
                    {
                        Id = "E06000027",
                        Name = "Torbay",
                    },
                    new Region
                    {
                        Id = "E06000028",
                        Name = "Bournemouth",
                    },
                    new Region
                    {
                        Id = "E06000029",
                        Name = "Poole",
                    },
                    new Region
                    {
                        Id = "E06000030",
                        Name = "Swindon",
                    },
                    new Region
                    {
                        Id = "E06000052",
                        Name = "Cornwall",
                    },
                    new Region
                    {
                        Id = "E06000053",
                        Name = "Isles of Scilly",
                    },
                    new Region
                    {
                        Id = "E06000054",
                        Name = "Wiltshire",
                    },
                    new Region
                    {
                        Id = "E10000008",
                        Name = "Devon",
                    },
                    new Region
                    {
                        Id = "E10000009",
                        Name = "Dorset",
                    },
                    new Region
                    {
                        Id = "E10000013",
                        Name = "Gloucestershire",
                    },
                    new Region
                    {
                        Id = "E10000027",
                        Name = "Somerset",
                    }
                }
            }
        };

        public string Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<Region> SubRegions { get; set; }
    }
}
