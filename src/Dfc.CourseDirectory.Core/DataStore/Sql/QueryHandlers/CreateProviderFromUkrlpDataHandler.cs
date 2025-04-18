﻿using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateProviderFromUkrlpDataHandler : ISqlQueryHandler<CreateProviderFromUkrlpData, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateProviderFromUkrlpData query)
        {
            var sqlProviders = $@"
                INSERT INTO [Pttcd].[Providers]([ProviderId],[Ukprn],[ProviderName],[Alias],[ProviderStatus],[ProviderType],[UkrlpProviderStatusDescription],[UpdatedOn],[UpdatedBy])
                            VALUES (@ProviderId,@Ukprn,@ProviderName,@Alias,@Status,@ProviderType, @ProviderStatus,@UpdatedOn,@UpdatedBy); ";

            var sqlProviderContact = $@"

                INSERT INTO [Pttcd].[ProviderContacts]
                        ([ProviderId]
                          ,[ProviderContactIndex]
                          ,[ContactType]
                          ,[ContactRole]
                          ,[AddressSaonDescription]
                          ,[AddressPaonDescription]
                          ,[AddressStreetDescription]
                          ,[AddressLocality]
                          ,[AddressItems]
                          ,[AddressPostTown]
                          ,[AddressCounty]
                          ,[AddressPostcode]
                          ,[PersonalDetailsPersonNameTitle]
                          ,[PersonalDetailsPersonNameGivenName]
                          ,[PersonalDetailsPersonNameFamilyName]
                          ,[Telephone1]
                          ,[Telephone2]
                          ,[Fax]
                          ,[WebsiteAddress]
                          ,[Email])
                    VALUES (@ProviderId
                          ,@ProviderContactIndex
                          ,@ContactType
                          ,@ContactRole
                          ,@AddressSaonDescription
                          ,@AddressPaonDescription
                          ,@AddressStreetDescription
                          ,@AddressLocality
                          ,@AddressItems
                          ,@AddressPostTown
                          ,@AddressCounty
                          ,@AddressPostcode
                          ,@PersonalDetailsPersonNameTitle
                          ,@PersonalDetailsPersonNameGivenName
                          ,@PersonalDetailsPersonNameFamilyName
                          ,@Telephone1
                          ,@Telephone2
                          ,@Fax
                          ,@WebsiteAddress
                          ,@Email);";

            var paramz = new
            {
                query.ProviderId,
                query.Ukprn,
                query.ProviderName,
                query.Alias,
                query.Status,
                query.ProviderStatus,
                query.ProviderType,
                UpdatedOn = query.DateUpdated,
                query.UpdatedBy
            };

            await transaction.Connection.ExecuteAsync(sqlProviders, paramz, transaction);

            var providerContact = query?.Contact;

            if (providerContact != null)
            {
                var paramzContacts = new
                {
                    query.ProviderId,
                    providerContact.ProviderContactIndex,
                    providerContact.ContactType,
                    providerContact.ContactRole,
                    providerContact.AddressSaonDescription,
                    providerContact.AddressPaonDescription,
                    providerContact.AddressStreetDescription,
                    providerContact.AddressLocality,
                    providerContact.AddressItems,
                    providerContact.AddressPostTown,
                    providerContact.AddressCounty,
                    providerContact.AddressPostcode,
                    providerContact.PersonalDetailsPersonNameTitle,
                    providerContact.PersonalDetailsPersonNameGivenName,
                    providerContact.PersonalDetailsPersonNameFamilyName,
                    providerContact.Telephone1,
                    providerContact.Telephone2,
                    providerContact.Fax,
                    providerContact.WebsiteAddress,
                    providerContact.Email
                };

                await transaction.Connection.ExecuteAsync(sqlProviderContact, paramzContacts, transaction);
            }

            return new Success();
        }
    }
}
