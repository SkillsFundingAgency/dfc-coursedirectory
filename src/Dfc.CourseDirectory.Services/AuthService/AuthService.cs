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
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IBaseDataAccess BaseDataAccess;
        private readonly ILogger _logger;
        public AuthService(
            IBaseDataAccess baseDataAccess,
            ILoggerFactory logFactory)
        {
            BaseDataAccess = baseDataAccess;
            _logger = logFactory.CreateLogger<AuthService>();
        }
        public AuthUserDetails GetDetailsByEmail(string email)
        {
            _logger.LogWarning("Getting auth tokens for " + email);
            SqlParameter param = new SqlParameter()
            {
                ParameterName = "@Email",
                Value = email

            };
            List<SqlParameter> dbParameters = new List<SqlParameter>
            {
                param

            };
            var dt = BaseDataAccess.GetDataReader("dbo.dfc_GetUserAuthorisationDetailsByEmail", dbParameters, CommandType.StoredProcedure);
            AuthUserDetails details = ExtractUserDetails(dt);
            return details;
        }
        private AuthUserDetails ExtractUserDetails(DataTable dt)
        {
            _logger.LogWarning("Extracting User Data");
            AuthUserDetails details = new AuthUserDetails(
                userId: (((string)dt.Rows[0]["UserId"] != string.Empty) ? Guid.Parse(dt.Rows[0]["UserId"].ToString()) : Guid.Empty),
                email: dt.Rows[0]["Email"].ToString(),
                userName: dt.Rows[0]["UserName"].ToString(),
                nameOfUser: dt.Rows[0]["NameOfUser"].ToString(),
                roleId: ((string)dt.Rows[0]["RoleId"] != string.Empty) ? Guid.Parse(dt.Rows[0]["RoleId"].ToString()) : Guid.Empty,
                roleName: dt.Rows[0]["RoleName"].ToString(),
                ukPrn: dt.Rows[0]["UKPRN"].ToString()
                );

            return details;
        }

    }
}
