using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    [Flags]
    public enum ProviderType
    {
        FE = 1,
        Apprenticeships = 2,
        Both = FE | Apprenticeships,
    }

    public static class ProviderTypeExtensions
    {
        public static string ToDisplayName(this ProviderType providerType) =>
            providerType switch
            {
                ProviderType.Apprenticeships => "Apprenticeships",
                ProviderType.FE => "FE",
                ProviderType.Both => "FE & Apprenticeships",
                _ => throw new NotImplementedException($"Unknown value: '{providerType}'.")
            };
    }
}
