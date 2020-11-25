using System;

namespace Dfc.Providerportal.FindAnApprenticeship.Models
{
    public class ExportKey
    {
        private readonly DateTimeOffset _keyDateTime;

        public ExportKey(DateTimeOffset keyDateTime)
        {
            _keyDateTime = keyDateTime.Date;
        }

        public static ExportKey FromUtcNow()
        {
            return new ExportKey(DateTimeOffset.UtcNow);
        }

        public override string ToString()
        {
            return $"providers-{_keyDateTime:yyyyMMdd}.json";
        }

        public static implicit operator string(ExportKey exportKey)
        {
            return exportKey?.ToString();
        }
    }
}