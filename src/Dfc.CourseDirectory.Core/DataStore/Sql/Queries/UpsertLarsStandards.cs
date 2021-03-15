using System;
using System.Collections.Generic;
using CsvHelper.Configuration;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsStandards : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsStandardRecord> Records { get; set; }
    }

    public class UpsertLarsStandardRecord
    {
        public int StandardCode { get; set; }
        public int Version { get; set; }
        public string StandardName { get; set; }
        public string StandardSectorCode { get; set; }
        public string NotionalEndLevel { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string UrlLink { get; set; }
        public decimal SectorSubjectAreaTier1 { get; set; }
        public decimal SectorSubjectAreaTier2 { get; set; }
        public bool? OtherBodyApprovalRequired { get; set; }

        public class ClassMap : ClassMap<UpsertLarsStandardRecord>
        {
            public ClassMap()
            {
                Map(m => m.StandardCode).Name(nameof(StandardCode));
                Map(m => m.Version).Name(nameof(Version));
                Map(m => m.StandardName).Name(nameof(StandardName));
                Map(m => m.StandardSectorCode).Name(nameof(StandardSectorCode));
                Map(m => m.NotionalEndLevel).Name(nameof(NotionalEndLevel));
                Map(m => m.EffectiveFrom).Name(nameof(EffectiveFrom));
                Map(m => m.EffectiveTo).Name(nameof(EffectiveTo));
                Map(m => m.UrlLink).Name("URLLink");
                Map(m => m.SectorSubjectAreaTier1).Name(nameof(SectorSubjectAreaTier1));
                Map(m => m.SectorSubjectAreaTier2).Name(nameof(SectorSubjectAreaTier2));
                Map(m => m.OtherBodyApprovalRequired)
                    .TypeConverterOption.BooleanValues(true, true, "Y")
                    .TypeConverterOption.BooleanValues(false, true, "N");
            }
        }
    }
}
