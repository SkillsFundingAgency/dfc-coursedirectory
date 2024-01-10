using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum Sector
    {
        BusinessAndAdministration = 1,
        ConstructionAndTheBuiltEnvironment,
        CreativeAndDesign,
        Digital,
        EducationAndEarlyYears,
        EngineeringAndManufacturing,
        SalesMarketingAndProcurement,
        TransportAndLogistics
    }

    public static class SectorExtensions
    {
        public static string ToDescription(this Sector? sector) => GetSectorDescription(sector);

        public static string ToDescription(this Sector sector) => GetSectorDescription(sector);

        private static string GetSectorDescription(Sector? sector)
        {
            return sector switch
            {
                null => "",
                Sector.BusinessAndAdministration => "Business and Administration",
                Sector.ConstructionAndTheBuiltEnvironment => "Construction and the Built Environment",
                Sector.CreativeAndDesign => "Creative and Design",
                Sector.Digital => "Digital",                
                Sector.EducationAndEarlyYears => "Education and Early Years",
                Sector.EngineeringAndManufacturing => "Engineering and Manufacturing",
                Sector.SalesMarketingAndProcurement => "Sales, Marketing and Procurement",
                Sector.TransportAndLogistics => "Transport and Logistics",
                _ => throw new NotImplementedException($"Unknown value: '{sector}'.")
            };
        }
    }
}
