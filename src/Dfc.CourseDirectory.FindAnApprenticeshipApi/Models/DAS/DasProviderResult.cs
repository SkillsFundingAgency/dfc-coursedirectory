using System;
using System.Collections.Generic;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.DAS;

namespace Dfc.Providerportal.FindAnApprenticeship.Models.DAS
{
    public class DasProviderResult
    {
        public int UKPRN { get; }

        public bool Success { get; }

        public IDasProvider Result { get; }

        public IEnumerable<Exception> Exceptions { get; }

        private DasProviderResult(int ukprn, bool success, IDasProvider result, IEnumerable<Exception> exceptions)
        {
            UKPRN = ukprn;
            Success = success;
            Result = result;
            Exceptions = exceptions;
        }

        public static DasProviderResult Succeeded(int ukPrn, IDasProvider result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new DasProviderResult(ukPrn, true, result, null);
        }

        public static DasProviderResult Failed(int ukPrn, params Exception[] exceptions)
        {
            return new DasProviderResult(ukPrn, false, null, exceptions);
        }
    }
}