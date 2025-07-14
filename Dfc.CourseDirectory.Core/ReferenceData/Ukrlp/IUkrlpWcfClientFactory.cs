using UkrlpService;

namespace Dfc.CourseDirectory.Core.ReferenceData.Ukrlp
{
    public interface IUkrlpWcfClientFactory
    {
        ProviderQueryPortTypeClient Build(WcfConfiguration configuration = null);
    }
}
