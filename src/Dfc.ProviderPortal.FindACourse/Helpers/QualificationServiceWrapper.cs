
using System;
using System.Collections.Generic;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using Dfc.ProviderPortal.Packages;
using Dfc.ProviderPortal.FindACourse.Models;
using Dfc.ProviderPortal.FindACourse.Settings;
using Dfc.ProviderPortal.FindACourse.Interfaces;


namespace Dfc.ProviderPortal.FindACourse.Helpers
{
    public class QualificationServiceWrapper : IQualificationServiceWrapper
    {
        private readonly ILogger _log;
        private readonly IQualificationServiceSettings _settings;
        private static SearchServiceClient _service;
        private static ISearchIndexClient _index;

        public QualificationServiceWrapper(IQualificationServiceSettings settings)
        {
            Throw.IfNull(settings, nameof(settings));
            _settings = settings;

            _service = new SearchServiceClient(settings.SearchService, new SearchCredentials(settings.QueryKey));
            _index = _service?.Indexes?.GetClient(_settings.Index);
        }

        public dynamic GetQualificationById(string LARSRef)
        {
            try {
                //_log.LogInformation($"Searching by {LARSRef}");
                SearchParameters parms = new SearchParameters() { Top = 10 };
                DocumentSearchResult<dynamic> results =
                    _index.Documents.Search<dynamic>(LARSRef, parms);
                //_log.LogInformation($"{results.Count ?? 0} matches found");
                if (results.Results.Count > 0)
                    return results.Results[0].Document;
                else
                    return null;

            } catch (Exception ex) {
                //_log.LogError(ex, "Error in GetQualificationById", LARSRef);
                throw ex;
            }
        }
    }
}
