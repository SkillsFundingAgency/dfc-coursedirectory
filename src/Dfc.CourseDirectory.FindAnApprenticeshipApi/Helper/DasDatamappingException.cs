using System;

namespace Dfc.Providerportal.FindAnApprenticeship.Helper
{
    public class ExportException : Exception
    {
        public ExportException()
        {
        }

        public ExportException(string message)
            : base(message)
        {
        }

        public ExportException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ProviderExportException : ExportException
    {
        public ProviderExportException(int id, Exception inner)
            : base($"Error exporting provider {id} to DAS", inner)
        {
        }
    }

    public class LocationExportException : ExportException
    {
        public LocationExportException(string id)
            : base($"Error exporting location {id} to DAS")
        {
        }

        public LocationExportException(string id, Exception inner)
            : base($"Error exporting location {id} to DAS", inner)
        {
        }
    }

    public class StandardExportException : ExportException
    {
        public StandardExportException(string id, Exception inner)
            : base($"Error exporting standard {id} to DAS", inner)
        {
        }
    }

    public class FrameworkExportException : ExportException
    {
        public FrameworkExportException(string id, Exception inner)
            : base($"Error exporting framework {id} to DAS", inner)
        {
        }
    }

    public class ProviderNotFoundException : ExportException
    {
        public ProviderNotFoundException(int id)
            : base($"Could not find provider details for UKPRN {id}")
        {
        }
    }

    public class InvalidUkprnException : ExportException
    {
        public InvalidUkprnException(string id)
            : base($"Provider UKPRN missing or invalid")
        {
        }
    }

    public class MissingContactException : ExportException
    {
        public MissingContactException()
            : base($"Cannot process a provider without contact information")
        {
        }
    }

    public class ReferenceDataServiceException : Exception
    {
        public ReferenceDataServiceException(Exception e)
            : base($"Reference data service error: {e.Message}", e)
        {
        }

        public ReferenceDataServiceException(string id, Exception e)
            : base($"Could not find FE Choices data for UKPRN {id}", e)
        {
        }
    }

    public class ProviderServiceException : Exception
    {
        public ProviderServiceException(Exception e)
            : base($"Provider service error: {e.Message}", e)
        {
        }

        public ProviderServiceException(int id, Exception e)
            : base($"Could not find FE Choices data for UKPRN {id}", e)
        {
        }
    }
}