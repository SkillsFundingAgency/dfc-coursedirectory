using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Auth;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Services.Interfaces.AuthService;
using Dfc.CourseDirectory.Services.Interfaces.BaseDataAccess;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.ProviderService;

namespace Dfc.CourseDirectory.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IBaseDataAccess BaseDataAccess;
        private readonly ILogger<AuthService> _logger;
        private readonly IProviderService _providerService;
        public AuthService(
            IBaseDataAccess baseDataAccess,
            ILogger<AuthService> logger, IProviderService providerService)
        {
            BaseDataAccess = baseDataAccess;
            _logger = logger;
            _providerService = providerService;
        }
        public async Task<AuthUserDetails> GetDetailsByEmail(string email)
        {
            _logger.LogCritical("Getting auth tokens for " + email);
            SqlParameter param = new SqlParameter()
            {
                ParameterName = "@Email",
                Value = email

            };
            List<SqlParameter> dbParameters = new List<SqlParameter>
            {
                param

            };
            AuthUserDetails details = null;
            try
            {
                var dt = BaseDataAccess.GetDataReader("dbo.dfc_GetUserAuthorisationDetailsByEmail", dbParameters, CommandType.StoredProcedure);
                details = await ExtractUserDetails(dt);
            }
            catch(Exception ex)
            {
                _logger.LogCritical("Login failed for " + email, ex);
            }

            return details;
        }
        private async Task<AuthUserDetails> ExtractUserDetails(DataTable dt)
        {
            var ukprn = dt.Rows[0]["UKPRN"].ToString();
            var roleName = dt.Rows[0]["RoleName"].ToString();
            
            _logger.LogWarning("Extracting User Data");
            AuthUserDetails details = new AuthUserDetails(
                userId: (((string)dt.Rows[0]["UserId"] != string.Empty) ? Guid.Parse(dt.Rows[0]["UserId"].ToString()) : Guid.Empty),
                email: dt.Rows[0]["Email"].ToString(),
                userName: dt.Rows[0]["UserName"].ToString(),
                nameOfUser: dt.Rows[0]["NameOfUser"].ToString(),
                roleId: ((string)dt.Rows[0]["RoleId"] != string.Empty) ? Guid.Parse(dt.Rows[0]["RoleId"].ToString()) : Guid.Empty,
                roleName: roleName,
                ukPrn: ukprn,
                providerType: await GetProviderType(ukprn,roleName)
                );

            return details;
        }

        public async Task<string> GetProviderType(string UKPRN, string roleName)
        {
            if (roleName == "Provider User" || roleName == "Provider Superuser")
            {
                var providerDetails =
                    await _providerService.GetProviderByPRNAsync(new ProviderSearchCriteria(UKPRN));

                var provider = providerDetails.Value?.Value?.FirstOrDefault();
                if (providerDetails.IsSuccess && provider != null)
                {
                    return provider.ProviderType.ToString();
                }
            }

            return ProviderType.Both.ToString();
        }

    }
}
