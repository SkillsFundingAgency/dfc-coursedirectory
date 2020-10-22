using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertProvidersHandler : ISqlQueryHandler<UpsertProviders, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertProviders query)
        {
            await UpsertProvider();
            await UpsertProviderContacts();

            async Task UpsertProvider()
            {
                var createTableSql = @"
CREATE TABLE #Providers (
	ProviderId UNIQUEIDENTIFIER,
	Ukprn INT,
    ProviderStatus TINYINT,
    ProviderType TINYINT,
    ProviderName NVARCHAR(MAX),
    UkrlpProviderStatusDescription NVARCHAR(MAX),
    MarketingInformation NVARCHAR(MAX),
    CourseDirectoryName NVARCHAR(MAX),
    TradingName NVARCHAR(MAX),
    Alias NVARCHAR(MAX),
    UpdatedOn DATETIME,
    UpdatedBy NVARCHAR(MAX),
    NationalApprenticeshipProvider BIT,
    TribalProviderId INT
)";

                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.Select(r => new
                    {
                        r.ProviderId,
                        r.Ukprn,
                        ProviderStatus = (byte)r.ProviderStatus,
                        ProviderType = (byte)r.ProviderType,
                        r.ProviderName,
                        r.UkrlpProviderStatusDescription,
                        r.MarketingInformation,
                        r.CourseDirectoryName,
                        r.TradingName,
                        r.Alias,
                        r.UpdatedOn,
                        r.UpdatedBy,
                        r.NationalApprenticeshipProvider,
                        r.TribalProviderId
                    }),
                    tableName: "#Providers",
                    transaction);

                var sql = @"
MERGE Pttcd.Providers AS target
USING (SELECT * FROM #Providers) AS source
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
        UpdatedBy,
        NationalApprenticeshipProvider,
        TribalProviderId
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
        source.UpdatedBy,
        source.NationalApprenticeshipProvider,
        source.TribalProviderId
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
        UpdatedBy = source.UpdatedBy,
        NationalApprenticeshipProvider = source.NationalApprenticeshipProvider,
        TribalProviderId = source.TribalProviderId;";

                await transaction.Connection.ExecuteAsync(sql, transaction: transaction);
            }

            async Task UpsertProviderContacts()
            {
                var createTableSql = @"
CREATE TABLE #ProviderContacts (
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
	PersonalDetailsPersonNameFamilyName NVARCHAR(MAX),
	Telephone1 NVARCHAR(MAX),
	Telephone2 NVARCHAR(MAX),
	Fax NVARCHAR(MAX),
	WebsiteAddress NVARCHAR(MAX),
	Email NVARCHAR(MAX)
)";

                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records
                        .SelectMany(p => p.Contacts.Select((c, i) => new
                        {
                            p.ProviderId,
                            ProviderContactIndex = i,
                            c.ContactType,
                            c.ContactRole,
                            c.AddressSaonDescription,
                            c.AddressPaonDescription,
                            c.AddressStreetDescription,
                            c.AddressLocality,
                            c.AddressItems,
                            c.AddressPostTown,
                            c.AddressPostcode,
                            c.PersonalDetailsPersonNameTitle,
                            c.PersonalDetailsPersonNameGivenName,
                            c.PersonalDetailsPersonNameFamilyName,
                            c.Telephone1,
                            c.Telephone2,
                            c.Fax,
                            c.WebsiteAddress,
                            c.Email
                        })),
                    tableName: "#ProviderContacts",
                    transaction);

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
        PersonalDetailsPersonNameFamilyName,
        Telephone1,
        Telephone2,
        Fax,
        WebsiteAddress,
        Email
    ) VALUES (
        source.ProviderId,
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
        source.PersonalDetailsPersonNameFamilyName,
        source.Telephone1,
        source.Telephone2,
        source.Fax,
        source.WebsiteAddress,
        source.Email
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
    PersonalDetailsPersonNameFamilyName = source.PersonalDetailsPersonNameFamilyName,
    Telephone1 = source.Telephone1,
    Telephone2 = source.Telephone2,
    Fax = source.Fax,
    WebsiteAddress = source.WebsiteAddress,
    Email = source.Email
WHEN NOT MATCHED BY SOURCE AND target.ProviderId IN (SELECT ProviderId FROM #Providers) THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
            }

            return new None();
        }
    }
}
