using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using Dfc.Providerportal.FindAnApprenticeship.Helper;

namespace Dfc.ProviderPortal.FindAnApprenticeship.UnitTests.Services
{

    public class ApprenticeshipServiceTests : IDisposable
    {
        public ApprenticeshipServiceTests()
        {
            // common test scaffolding
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
