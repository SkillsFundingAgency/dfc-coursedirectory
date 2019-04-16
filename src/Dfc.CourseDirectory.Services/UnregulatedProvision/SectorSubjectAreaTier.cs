using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Services.UnregulatedProvision
{
    public class SectorSubjectAreaTier
    {
        private List<SectorSubjectAreaTier1> _sectorSubjectAreaTierAll = new List<SectorSubjectAreaTier1>();
        public List<SectorSubjectAreaTier1> SectorSubjectAreaTierAll
        {
            get
            {
                if (_sectorSubjectAreaTierAll != null && _sectorSubjectAreaTierAll.Any())
                {
                    return _sectorSubjectAreaTierAll;
                }

                _sectorSubjectAreaTierAll = new List<SectorSubjectAreaTier1>();
                // 1.00
                var ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "1.00",
                    Description = "Health, public services, care",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"1.10", "Medicine and Dentistry"},
                        {"1.20", "Nursing and medicine"},
                        {"1.30", "Health and social Care"},
                        {"1.40", "Public services"},
                        {"1.50", "Child development and wellbeing"}
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 2.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id= "2.00",
                    Description = "Science mathematics",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"2.10", "Science" },
                        {"2.20", "Mathematics, statistics" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 3.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "3.00",
                    Description = "Agriculture, horticulture, animals",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"3.10", "Agriculture" },
                        {"3.20", "Horticulture, forestry" },
                        {"3.30", "Animal care and veterinary science" },
                        {"3.40", "Environmental conservation" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 4.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "4.00",
                    Description = "Engineering and manufacturing",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"4.10", "Engineering" },
                        {"4.20", "Manufacturing technologies" },
                        {"4.30", "Transportation ops and maintenance" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 5.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "5.00",
                    Description = "Construction, planning, built environment",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"5.10", "Architecture" },
                        {"5.20", "Building and construction" },
                        {"5.30", "Urban, rural and regional planning" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 6.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "6.00",
                    Description = "ICT",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"6.10", "ICT practitioners" },
                        {"6.20", "ICT for users" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 7.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "7.00",
                    Description = "Retail and commercial enterprise",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"7.10", "Retailing, wholesaling" },
                        {"7.20", "Warehousing, distribution" },
                        {"7.30", "Service enterprises" },
                        {"7.40", "Hospitality, catering" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 8.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "8.00",
                    Description = "Leisure, travel and tourism",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"8.10", "Sport, leisure, recreation" },
                        {"8.20", "Travel, tourism" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 9.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "9.00",
                    Description = "Arts, media and publishing",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"9.10", "Performing arts" },
                        {"9.20", "Crafts, creative arts, design" },
                        {"9.30", "Media and communication" },
                        {"9.40", "Publishing, information services" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 10.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "10.00",
                    Description = "History, philosophy, theology",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"10.10", "History" },
                        {"10.20", "Archaeology and its science" },
                        {"10.30", "Philosophy" },
                        {"10.40", "Theology and religious studies" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 11.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "11.00",
                    Description = "Social sciences",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"11.10", "Geography" },
                        {"11.20", "Sociology and social policy" },
                        {"11.30", "Politics" },
                        {"11.40", "Economics" },
                        {"11.50", "Anthropology" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 12.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "12.00",
                    Description = "Languages, literature culture",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"12.10", "Language, lit, culture (British)" },
                        {"12.20", "Language, lit, culture (other)" },
                        {"12.30", "Linguistics" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 13.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "13.00",
                    Description = "Education and training",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"13.10", "Teaching, lecturing" },
                        {"13.20", "Direct learning support" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 14.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "14.00",
                    Description = "Preparation for life and work",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"14.10", "Foundations for learning and life" },
                        {"14.20", "Preparation for work" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 15.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "15.00",
                    Description = "Business, administration, law",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"15.10", "Accounting and finance" },
                        {"15.20", "Administration" },
                        {"15.30", "Business management" },
                        {"15.40", "Marketing and sales" },
                        {"15.50", "Law and legal services" },
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                return _sectorSubjectAreaTierAll;
            }
        }

    }
}
