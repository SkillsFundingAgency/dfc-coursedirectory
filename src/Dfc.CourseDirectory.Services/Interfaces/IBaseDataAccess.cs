using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IBaseDataAccess
    {
        string ConnectionString { get; set; }
        SqlConnection GetConnection();
        DbCommand GetCommand(DbConnection connection, string commandText, CommandType commandType);
        SqlParameter GetParameter(string parameter, object value);
        SqlParameter GetParameterOut(string parameter, SqlDbType type, object value = null, ParameterDirection parameterDirection = ParameterDirection.InputOutput);
        int ExecuteNonQuery(string procedureName, List<DbParameter> parameters, CommandType commandType = CommandType.StoredProcedure);
        object ExecuteScalar(string procedureName, List<SqlParameter> parameters);
        DbDataReader GetDataReader(string procedureName, List<DbParameter> parameters, CommandType commandType = CommandType.StoredProcedure);
    }
}
