using System;

namespace Dfc.CourseDirectory.Core.Models
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
                ProviderType.FE => "F.E.",
                ProviderType.Both => "Both",
                _ => throw new NotImplementedException($"Unknown value: '{providerType}'.")
            };
    }
}
