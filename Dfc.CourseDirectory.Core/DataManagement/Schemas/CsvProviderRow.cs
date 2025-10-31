using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvInactiveProviderRow
    {
        [Index(0), Name("Org UPIN")]
        public string OrgUPIN { get; set; }

        [Index(1), Name("Org UKPRN")]
        public int OrgUKPRN { get; set; }

        [Index(2), Name("Org Legal Name")]
        public string OrgLegalName { get; set; }

        [Index(3), Name("Org Trading Name")]
        public string OrgTradingName { get; set; }

        [Index(4), Name("Org Status")]
        public string OrgStatus { get; set; }

        [Index(5), Name("Org Status Date")]
        public string OrgStatusDate { get; set; }

        public static CsvInactiveProviderRow[][] GroupRows(IEnumerable<CsvInactiveProviderRow> rows) =>
            rows.GroupBy(r => r, new CsvInactiveProviderRowProviderComparer())
                .Select(g => g.ToArray())
                .ToArray();

        private class CsvInactiveProviderRowProviderComparer : IEqualityComparer<CsvInactiveProviderRow>
        {
            public bool Equals(CsvInactiveProviderRow x, CsvInactiveProviderRow y)
            {
                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(x.OrgUPIN) || string.IsNullOrEmpty(y.OrgUPIN))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(x.OrgTradingName) || string.IsNullOrEmpty(y.OrgTradingName))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(x.OrgLegalName) || string.IsNullOrEmpty(y.OrgLegalName))
                {
                    return false;
                }

                return
                    x.OrgUPIN == y.OrgUPIN &&
                    x.OrgUKPRN == y.OrgUKPRN &&
                    x.OrgLegalName == y.OrgLegalName &&
                    x.OrgTradingName == y.OrgTradingName &&
                    x.OrgStatus == y.OrgStatus &&
                    x.OrgStatusDate == y.OrgStatusDate;
            }

            public int GetHashCode(CsvInactiveProviderRow obj) =>
                HashCode.Combine(
                    obj.OrgUPIN,
                    obj.OrgUKPRN,
                    obj.OrgLegalName,
                    obj.OrgTradingName,
                    obj.OrgStatus,
                    obj.OrgStatusDate);
        }
    }
    public class CsvProviderRow
    {
        [Index(0), Name("Org UPIN")]
        public string OrgUPIN { get; set; }

        [Index(1), Name("Org UKPRN")]
        public int OrgUKPRN { get; set; }

        [Index(2), Name("Org Legal Name")]
        public string OrgLegalName { get; set; }

        [Index(3), Name("Org Trading Name")]
        public string OrgTradingName { get; set; }

        [Index(4), Name("Org Status")]
        public string OrgStatus { get; set; }

        [Index(5), Name("Fund Name")]
        public string FundName { get; set; }

        [Index(6), Name("Fund Start Date")]
        public string FundStartDate { get; set; }

        public static CsvProviderRow[][] GroupRows(IEnumerable<CsvProviderRow> rows) =>
            rows.GroupBy(r => r, new CsvProviderRowProviderComparer())
                .Select(g => g.ToArray())
                .ToArray();

        private class CsvProviderRowProviderComparer : IEqualityComparer<CsvProviderRow>
        {
            public bool Equals(CsvProviderRow x, CsvProviderRow y)
            {
                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(x.OrgUPIN) || string.IsNullOrEmpty(y.OrgUPIN))
                {
                    return false;
                }
               
                if (string.IsNullOrEmpty(x.OrgTradingName) || string.IsNullOrEmpty(y.OrgTradingName))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(x.OrgLegalName) || string.IsNullOrEmpty(y.OrgLegalName))
                {
                    return false;
                }

                return
                    x.OrgUPIN == y.OrgUPIN &&
                    x.OrgUKPRN == y.OrgUKPRN &&
                    x.OrgLegalName == y.OrgLegalName &&
                    x.OrgTradingName == y.OrgTradingName &&
                    x.OrgStatus == y.OrgStatus &&
                    x.FundName == y.FundName &&
                    x.FundStartDate == y.FundStartDate;
            }

            public int GetHashCode(CsvProviderRow obj) =>
                HashCode.Combine(
                    obj.OrgUPIN,
                    obj.OrgUKPRN,
                    obj.OrgLegalName,
                    obj.OrgTradingName,
                    obj.OrgStatus,
                    obj.FundName,
                    obj.FundStartDate);
        }
    }
}
