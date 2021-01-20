using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.DAS;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.ViewModels
{
    public class DasProviderResultViewModel
    {
        public bool Success { get; set; }

        public IDasProvider Result { get; set; }

        public IEnumerable<string> Messages { get; set; }

        public static DasProviderResultViewModel FromDasProviderResult(DasProviderResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new DasProviderResultViewModel
            {
                Success = result.Success,
                Result = result.Result,
                Messages = result.Exceptions?.Select(e =>
                    e is ProviderExportException && e.InnerException != null
                        ? e.InnerException.Message
                        : e.Message).ToArray()
            };
        }
    }
}