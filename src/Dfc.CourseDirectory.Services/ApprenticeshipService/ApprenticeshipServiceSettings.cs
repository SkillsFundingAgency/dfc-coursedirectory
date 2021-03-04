using System;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class ApprenticeshipServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }

        public Uri AddApprenticeshipUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/AddApprenticeship");
        }

        public Uri AddApprenticeshipsUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/AddApprenticeships");
        }

        public Uri DeleteBulkUploadApprenticeshipsUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/DeleteBulkUploadApprenticeships");
        }

        public Uri ToGetAllDfcReports()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetAllDfcReports");
        }

        public Uri UpdateAprrenticeshipUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateApprenticeship");
        }

        public Uri ChangeApprenticeshipStatusesForUKPRNSelectionUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/ChangeApprenticeshipStatusForUKPRNSelection");
        }
    }
}
