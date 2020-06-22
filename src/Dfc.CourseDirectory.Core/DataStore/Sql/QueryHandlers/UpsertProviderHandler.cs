using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertProviderHandler : ISqlQueryHandler<UpsertProvider, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertProvider query)
        {
            await UpsertProvider();
            await UpsertProviderContacts();

            Task UpsertProvider()
            {
                var sql = @"
MERGE Pttcd.Providers AS target
USING (
    SELECT
        @ProviderId ProviderId,
        @Ukprn Ukprn,
        @ProviderStatus ProviderStatus,
        @ProviderType ProviderType,
        @ProviderName ProviderName,
        @UkrlpProviderStatusDescription UkrlpProviderStatusDescription,
        @MarketingInformation MarketingInformation,
        @CourseDirectoryName CourseDirectoryName,
        @TradingName TradingName,
        @Alias Alias,
        @UpdatedOn UpdatedOn,
        @UpdatedBy UpdatedBy
) AS source
ON target.ProviderId = source.ProviderId
WHEN NOT MATCHED THEN
    INSERT (
        ProviderId,
        Ukprn,
        ProviderStatus,
        ProviderType,
        ProviderName,
        UkrlpProviderStatusDescription,
        MarketingInformation,
        CourseDirectoryName,
        TradingName,
        Alias,
        UpdatedOn,
        UpdatedBy
    ) VALUES (
        source.ProviderId,
        source.Ukprn,
        source.ProviderStatus,
        source.ProviderType,
        source.ProviderName,
        source.UkrlpProviderStatusDescription,
        source.MarketingInformation,
        source.CourseDirectoryName,
        source.TradingName,
        source.Alias,
        source.UpdatedOn,
        source.UpdatedBy
    )
WHEN MATCHED THEN
    UPDATE SET
        Ukprn = source.Ukprn,
        ProviderStatus = source.ProviderStatus,
        ProviderType = source.ProviderType,
        ProviderName = source.ProviderName,
        UkrlpProviderStatusDescription = source.UkrlpProviderStatusDescription,
        MarketingInformation = source.MarketingInformation,
        CourseDirectoryName = source.CourseDirectoryName,
        TradingName = source.TradingName,
        Alias = source.Alias,
        UpdatedOn = source.UpdatedOn,
        UpdatedBy = source.UpdatedBy;";

                var paramz = new
                {
                    query.ProviderId,
                    query.Ukprn,
                    query.ProviderStatus,
                    query.ProviderType,
                    query.ProviderName,
                    query.UkrlpProviderStatusDescription,
                    query.MarketingInformation,
                    query.CourseDirectoryName,
                    query.TradingName,
                    query.Alias,
                    query.UpdatedOn,
                    query.UpdatedBy
                };

                return transaction.Connection.ExecuteAsync(sql, paramz, transaction);
            }

            async Task UpsertProviderContacts()
            {
                var createVariableSql = @"
CREATE TABLE #ProviderContacts
(
	ProviderId UNIQUEIDENTIFIER,
	ProviderContactIndex INT,
	ContactType CHAR,
	ContactRole NVARCHAR(MAX),
	AddressSaonDescription NVARCHAR(MAX),
	AddressPaonDescription NVARCHAR(MAX),
	AddressStreetDescription NVARCHAR(MAX),
	AddressLocality NVARCHAR(MAX),
	AddressItems NVARCHAR(MAX),
	AddressPostTown NVARCHAR(MAX),
	AddressPostcode NVARCHAR(MAX),
	PersonalDetailsPersonNameTitle NVARCHAR(MAX),
	PersonalDetailsPersonNameGivenName NVARCHAR(MAX),
	PersonalDetailsPersonNameFamilyName NVARCHAR(MAX)
)";

                await transaction.Connection.ExecuteAsync(createVariableSql, transaction: transaction);

                var providerContactIndex = 0;
                foreach (var contact in query.Contacts)
                {
                    var insertContactSql = @"
INSERT INTO #ProviderContacts (
    ProviderContactIndex,
    ContactType,
    ContactRole,
    AddressSaonDescription,
    AddressPaonDescription,
    AddressStreetDescription,
    AddressLocality,
    AddressItems,
    AddressPostTown,
    AddressPostcode,
    PersonalDetailsPersonNameTitle,
    PersonalDetailsPersonNameGivenName,
    PersonalDetailsPersonNameFamilyName)
VALUES (
    @ProviderContactIndex,
    @ContactType,
    @ContactRole,
    @AddressSaonDescription,
    @AddressPaonDescription,
    @AddressStreetDescription,
    @AddressLocality,
    @AddressItems,
    @AddressPostTown,
    @AddressPostcode,
    @PersonalDetailsPersonNameTitle,
    @PersonalDetailsPersonNameGivenName,
    @PersonalDetailsPersonNameFamilyName)";

                    await transaction.Connection.ExecuteAsync(
                        insertContactSql,
                        new
                        {
                            ProviderContactIndex = ++providerContactIndex,
                            contact.ContactType,
                            contact.ContactRole,
                            contact.AddressSaonDescription,
                            contact.AddressPaonDescription,
                            contact.AddressStreetDescription,
                            contact.AddressLocality,
                            contact.AddressItems,
                            contact.AddressPostTown,
                            contact.AddressPostcode,
                            contact.PersonalDetailsPersonNameTitle,
                            contact.PersonalDetailsPersonNameGivenName,
                            contact.PersonalDetailsPersonNameFamilyName
                        },
                        transaction);
                }

                var mergeSql = @"
MERGE Pttcd.ProviderContacts AS target
USING (SELECT * FROM #ProviderContacts) AS source
ON target.ProviderId = source.ProviderId AND target.ProviderContactIndex = source.ProviderContactIndex
WHEN NOT MATCHED THEN
    INSERT (
        ProviderId,
        ProviderContactIndex,
        ContactType,
        ContactRole,
        AddressSaonDescription,
        AddressPaonDescription,
        AddressStreetDescription,
        AddressLocality,
        AddressItems,
        AddressPostTown,
        AddressPostcode,
        PersonalDetailsPersonNameTitle,
        PersonalDetailsPersonNameGivenName,
        PersonalDetailsPersonNameFamilyName
    ) VALUES (
        @ProviderId,
        source.ProviderContactIndex,
        source.ContactType,
        source.ContactRole,
        source.AddressSaonDescription,
        source.AddressPaonDescription,
        source.AddressStreetDescription,
        source.AddressLocality,
        source.AddressItems,
        source.AddressPostTown,
        source.AddressPostcode,
        source.PersonalDetailsPersonNameTitle,
        source.PersonalDetailsPersonNameGivenName,
        source.PersonalDetailsPersonNameFamilyName
    )
WHEN MATCHED THEN UPDATE SET
    ProviderContactIndex = source.ProviderContactIndex,
    ContactType = source.ContactType,
    ContactRole = source.ContactRole,
    AddressSaonDescription = source.AddressSaonDescription,
    AddressPaonDescription = source.AddressPaonDescription,
    AddressStreetDescription = source.AddressStreetDescription,
    AddressLocality = source.AddressLocality,
    AddressItems = source.AddressItems,
    AddressPostTown = source.AddressPostTown,
    AddressPostcode = source.AddressPostcode,
    PersonalDetailsPersonNameTitle = source.PersonalDetailsPersonNameTitle,
    PersonalDetailsPersonNameGivenName = source.PersonalDetailsPersonNameGivenName,
    PersonalDetailsPersonNameFamilyName = source.PersonalDetailsPersonNameFamilyName
WHEN NOT MATCHED BY SOURCE AND target.ProviderId = @ProviderId THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, new { query.ProviderId }, transaction: transaction);
            }

            return new None();
        }
    }
}
