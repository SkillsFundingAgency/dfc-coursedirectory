using Dfc.CourseDirectory.WebV2.MultiPageTransaction;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModel : IMptxState
    {
        public string ProviderMarketingInformation { get; set; }

        public bool GotProviderDetail => !string.IsNullOrEmpty(ProviderMarketingInformation);

        public void SetProviderDetail(string marketingInformation) =>
            ProviderMarketingInformation = marketingInformation;
    }
}
