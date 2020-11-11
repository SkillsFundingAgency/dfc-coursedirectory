using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ProviderType
    {
        None = 0,
        FE = 1,
        Apprenticeships = 2
    }

    public static class ProviderTypeExtensions
    {
        public static string ToDescription(this ProviderType providerType) =>
            providerType switch
            {
                ProviderType.None => "None",
                ProviderType.Apprenticeships => "Apprenticeships",
                ProviderType.FE => "F.E.",
                ProviderType.Apprenticeships | ProviderType.FE => "Both",
                _ => throw new NotImplementedException($"Unknown value: '{providerType}'.")
            };
    }
}
