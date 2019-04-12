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
                    Description = "Health, Public Services and Care",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"1.10", "Medicine and Dentistry"},
                        {"1.20", "Nursing and Subjects and Vocations Allied to Medicine"},
                        {"1.30", "Health and Social Care"},
                        {"1.40", "Public Services"},
                        {"1.50", "Child Development and Well Being"}
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 2.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id= "2.00",
                    Description = "Science and Mathematics",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"2.10", "Science" },
                        {"2.20", "Mathematics and Statistics" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 3.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "3.00",
                    Description = "Agriculture, Horticulture and Animal Care",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"3.10", "Agriculture" },
                        {"3.20", "Horticulture and Forestry" },
                        {"3.30", "Animal Care and Veterinary Science" },
                        {"3.40", "Environmental Conservation" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 4.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "4.00",
                    Description = "Engineering and Manufacturing Technologies",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"4.10", "Engineering" },
                        {"4.20", "Manufacturing Technologies" },
                        {"4.30", "Transportation Operations and Maintenance" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 5.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "5.00",
                    Description = "Construction, Planning and the Built Environment",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"5.10", "Architecture" },
                        {"5.20", "Building and Construction" },
                        {"5.30", "Urban, Rural and Regional Planning" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 6.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "6.00",
                    Description = "Information and Communication Technology",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"6.10", "ICT Practitioners" },
                        {"6.20", "ICT for Users" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 7.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "7.00",
                    Description = "Retail and Commercial Enterprise",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"7.10", "Retailing and Wholesaling" },
                        {"7.20", "Warehousing and Distribution" },
                        {"7.30", "Service Enterprises" },
                        {"7.40", "Hospitality and Catering" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 8.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "8.00",
                    Description = "Leisure, Travel and Tourism",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"8.10", "Sport, Leisure and Recreation" },
                        {"8.20", "Travel and Tourism" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 9.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "9.00",
                    Description = "Arts, Media and Publishing",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"9.10", "Performing Arts" },
                        {"9.20", "Crafts, Creative Arts and Design" },
                        {"9.30", "Media and Communication" },
                        {"9.40", "Publishing and Information Services" }
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
                        {"10.20", "Archaeology and Archaeological Sciences" },
                        {"10.30", "Philosophy" },
                        {"10.40", "Theology and Religious Studies" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                // 11.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "11.00",
                    Description = "Social Sciences",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"11.10", "Geography" },
                        {"11.20", "Sociology and Social Policy" },
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
                    Description = "Languages, Literature and Culture",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"12.10", "Languages, Literature and Culture of the British Isles" },
                        {"12.20", "Other Languages, Literature and Culture" },
                        {"12.30", "Linguistics" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 13.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "13.00",
                    Description = "Education and Training",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"13.10", "Teaching and Lecturing" },
                        {"13.20", "Direct Learning Support" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 14.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "14.00",
                    Description = "Preparation for Life and Work",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"14.10", "Foundations for Learning and Life" },
                        {"14.20", "Preparation for Work" }
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);


                // 15.00
                ssat1 = new SectorSubjectAreaTier1
                {
                    Id = "15.00",
                    Description = "Business, Administration and Law",
                    SectorSubjectAreaTier2 = new Dictionary<string, string>
                    {
                        {"15.10", "Accounting and Finance" },
                        {"15.20", "Administration" },
                        {"15.30", "Business Management" },
                        {"15.40", "Marketing and Sales" },
                        {"15.50", "Law and Legal Services" },
                    }
                };
                _sectorSubjectAreaTierAll.Add(ssat1);

                return _sectorSubjectAreaTierAll;
            }
        }

    }
}
