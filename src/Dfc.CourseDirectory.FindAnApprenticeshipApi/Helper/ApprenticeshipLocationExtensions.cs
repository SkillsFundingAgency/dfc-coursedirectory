using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Dfc.Providerportal.FindAnApprenticeship.Models;

namespace Dfc.Providerportal.FindAnApprenticeship.Helper
{
    public static class ApprenticeshipLocationExtensions
    {
        public static int ToAddressHash(this ApprenticeshipLocation location)
        {
            var propertiesToHash = new List<string> {location.Name};

            if (location.National.HasValue)
            {
                propertiesToHash.Add($"{location.National}");
            }

            if (location.Regions != null && location.Regions.Length > 0)
            {
                propertiesToHash.Add(string.Join(",", location.Regions));
            }

            if (location.Address != null)
            {
                propertiesToHash.Add($"{location.Address?.Latitude}");
                propertiesToHash.Add($"{location.Address?.Longitude}");
                propertiesToHash.Add($"{location.Address?.Postcode}");
            }

            return GenerateHash(string.Join(", ", propertiesToHash));
        }

        private static int GenerateHash(string value)
        {
            using (var crypto = new SHA256CryptoServiceProvider())
            {
                var hash = 0;
                if (string.IsNullOrEmpty(value)) return (hash);

                var byteContents = Encoding.Unicode.GetBytes(value);

                var hashText = crypto.ComputeHash(byteContents);

                var α = BitConverter.ToInt64(hashText, 0);
                var β = BitConverter.ToInt64(hashText, 8);
                var γ = BitConverter.ToInt64(hashText, 24);

                hash = (int) (α ^ β ^ γ);
                return (hash);
            }
        }
    }

}