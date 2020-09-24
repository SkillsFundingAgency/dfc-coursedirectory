using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ProviderType
    {
        Undefined = 0,
        FE = 1,
        Apprenticeships = 2,
        Both = FE | Apprenticeships,
    }

    public static class ProviderTypeExtensions
    {
        public static string ToDescription(this ProviderType providerType) =>
            providerType switch
            {
                ProviderType.Undefined => "",
                ProviderType.Apprenticeships => "Apprenticeships",
                ProviderType.FE => "F.E.",
                ProviderType.Both => "Both",
                _ => throw new NotImplementedException($"Unknown value: '{providerType}'.")
            };
    }
}
