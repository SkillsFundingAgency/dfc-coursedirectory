using Dfc.CourseDirectory.Services.BaseDataAccess;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class BaseDataAccessTests
    {
        //[Fact]
        //public void ExecuteReader()
        //{
        //    BaseDataAccessSettings baseDataAccessSettings = new BaseDataAccessSettings
        //    {
        //        ConnectionString = ""
        //    };
        //    List<SqlParameter> param = new List<SqlParameter>();
        //    SqlParameter sqlParameter = new SqlParameter
        //    {
        //        ParameterName = "@RegionCode",
        //        Value = "E06000057"
        //    };

        //    param.Add(sqlParameter);
        //    BaseDataAccess dataAccess = new BaseDataAccess(Options.Create(baseDataAccessSettings));
        //    string command = "SELECT * FROM [dbo].[Subregion] WHERE SubregionId = @RegionCode";
        //    var query = dataAccess.GetDataReader(command, param);


        //}
        //[Fact]
        //public void RunStoredProc()
        //{
        //    BaseDataAccessSettings baseDataAccessSettings = new BaseDataAccessSettings
        //    {
        //        ConnectionString = ""
        //    };
        //    List<SqlParameter> param = new List<SqlParameter>();
        //    BaseDataAccess dataAccess = new BaseDataAccess(Options.Create(baseDataAccessSettings));
        //    var data = dataAccess.GetDataReader(
        //        procedureName: "",
        //        parameters: param,
        //        commandType: System.Data.CommandType.StoredProcedure);

        //}

    }
}
