using Dfc.CourseDirectory.Models.Interfaces.Providers;

namespace Dfc.CourseDirectory.Models.Models.Providers
{
    public class Provideralias : IProvideralias
    {
        public object ProviderAlias { get; set; }
        public object LastUpdated { get; set; }

        public Provideralias()
        {
        }
    }
}