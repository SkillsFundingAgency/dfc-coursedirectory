﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper
{
    // TODO: Polly Polly Polly!
    public class ReferenceDataServiceClient : IReferenceDataServiceClient
    {
        private readonly IReferenceDataService _service;

        public ReferenceDataServiceClient(IReferenceDataService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<IEnumerable<FeChoice>> GetAllFeChoiceData()
        {
            try
            {
                return await _service.GetAllFeChoicesAsync();
            }
            catch (HttpRequestException e)
            {
                // add polly retry
                throw new ReferenceDataServiceException(e);
            }
            catch (Exception e)
            {
                throw new ReferenceDataServiceException(e);
            }
        }
    }
}