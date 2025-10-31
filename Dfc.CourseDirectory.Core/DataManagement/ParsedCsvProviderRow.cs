using System;
using System.Globalization;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ParsedCsvProviderRow : CsvProviderRow
    {
        private const string DateFormat = "dd/MM/yyyy";

        private ParsedCsvProviderRow()
        {

        }
        public DateTime? ResolvedFundStartDate { get; private set; }
        public static ParsedCsvProviderRow FromCsvProviderRow(CsvProviderRow row)
        {
            var parsedRow = row.Adapt(new ParsedCsvProviderRow());
            parsedRow.OrgUPIN  = row.OrgUPIN;
            parsedRow.OrgUKPRN = row.OrgUKPRN;
            parsedRow.OrgLegalName = row.OrgLegalName;
            parsedRow.OrgStatus = row.OrgStatus;
            parsedRow.OrgTradingName = row.OrgTradingName;
            parsedRow.FundName = row.FundName;
            parsedRow.FundStartDate = row.FundStartDate;
            parsedRow.ResolvedFundStartDate = ResolveStartDate(row.FundStartDate);
            return parsedRow;
        }
        public static DateTime? ResolveStartDate(string value) =>
            DateTime.TryParseExact(value, DateFormat, null, DateTimeStyles.None, out var dt) ? dt : (DateTime?)null;

    }

    public class ParsedCsvInactiveProviderRow : CsvInactiveProviderRow
    {
        private const string DateFormat = "dd/MM/yyyy";

        private ParsedCsvInactiveProviderRow()
        {

        }
        public DateTime? ResolvedOrgStatusDate { get; private set; }
        public static ParsedCsvInactiveProviderRow FromCsvProviderRow(CsvInactiveProviderRow row)
        {
            var parsedRow = row.Adapt(new ParsedCsvInactiveProviderRow());
            parsedRow.OrgUPIN = row.OrgUPIN;
            parsedRow.OrgUKPRN = row.OrgUKPRN;
            parsedRow.OrgLegalName = row.OrgLegalName;
            parsedRow.OrgStatus = row.OrgStatus;
            parsedRow.OrgTradingName = row.OrgTradingName;
            parsedRow.ResolvedOrgStatusDate = ResolveOrgStatusDate(row.OrgStatusDate);
            return parsedRow;
        }
        public static DateTime? ResolveOrgStatusDate(string value) =>
            DateTime.TryParseExact(value, DateFormat, null, DateTimeStyles.None, out var dt) ? dt : (DateTime?)null;

    }
}
