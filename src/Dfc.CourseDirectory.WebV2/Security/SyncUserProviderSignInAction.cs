using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Helpers.Interfaces;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class SyncUserProviderSignInAction : ISignInAction
    {
        private readonly IUkrlpSyncHelper _ukrlpSyncHelper;


        public SyncUserProviderSignInAction(
            IUkrlpSyncHelper ukrlpSyncHelper)
        {
            _ukrlpSyncHelper = ukrlpSyncHelper;
        }

        public async Task OnUserSignedIn(SignInContext context)
        {
            if(context.ProviderUkprn.HasValue)
            {
                await _ukrlpSyncHelper.SyncProviderData(context.Provider.Id, context.Provider.Ukprn, context.UserInfo.Email);
            }
        }
    }
}
