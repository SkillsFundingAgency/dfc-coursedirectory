
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Dfc.ProviderPortal.FindACourse.Models;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface IProviderServiceWrapper
    {
        IEnumerable<AzureSearchProviderModel> GetLiveProvidersForAzureSearch();
    }
}
