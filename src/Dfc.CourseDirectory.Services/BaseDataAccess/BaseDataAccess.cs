using Dfc.CourseDirectory.Services.Interfaces.BaseDataAccess;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Dfc.CourseDirectory.Services.BaseDataAccess
{
    public class BaseDataAccess : IBaseDataAccess
    {
        public string ConnectionString { get; set; }
        private readonly ILogger<BaseDataAccess> _logger;
        public BaseDataAccess(IOptions<BaseDataAccessSettings> settings,
            ILoggerFactory logFactory)
        {
            this.ConnectionString = settings.Value.ConnectionString;
            _logger = logFactory.CreateLogger<BaseDataAccess>();
        }

        public SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(this.ConnectionString);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }

        public DbCommand GetCommand(DbConnection connection, string commandText, CommandType commandType)
        {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            SqlCommand command = new SqlCommand(commandText, connection as SqlConnection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.CommandType = commandType;
            return command;
        }

        public SqlParameter GetParameter(string parameter, object value)
        {
            SqlParameter parameterObject = new SqlParameter(parameter, value != null ? value : DBNull.Value);
            parameterObject.Direction = ParameterDirection.Input;
            return parameterObject;
        }

        public SqlParameter GetParameterOut(string parameter, SqlDbType type, object value = null, ParameterDirection parameterDirection = ParameterDirection.InputOutput)
        {
            SqlParameter parameterObject = new SqlParameter(parameter, type); ;

            if (type == SqlDbType.NVarChar || type == SqlDbType.VarChar || type == SqlDbType.NText || type == SqlDbType.Text)
            {
                parameterObject.Size = -1;
            }

            parameterObject.Direction = parameterDirection;

            if (value != null)
            {
                parameterObject.Value = value;
            }
            else
            {
                parameterObject.Value = DBNull.Value;
            }

            return parameterObject;
        }

        public int ExecuteNonQuery(string procedureName, List<DbParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            int returnValue = -1;

            try
            {
                using (SqlConnection connection = this.GetConnection())
                {
                    DbCommand cmd = this.GetCommand(connection, procedureName, commandType);

                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    returnValue = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                //LogException("Failed to ExecuteNonQuery for " + procedureName, ex, parameters);
                throw;
            }

            return returnValue;
        }

        public object ExecuteScalar(string command, List<SqlParameter> parameters)
        {
            object returnValue = null;

            try
            {
                using (SqlConnection connection = this.GetConnection())
                {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    SqlCommand cmd = new SqlCommand(command, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities


                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    returnValue = cmd.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                //LogException("Failed to ExecuteScalar for " + procedureName, ex, parameters);
                throw;
            }

            return returnValue;
        }

        public DataTable GetDataReader(string procedureName, List<SqlParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            _logger.LogCritical("Running: " + procedureName + " to get user tokens");
            DataTable values = new DataTable();
            try
            {
                DbConnection connection = this.GetConnection();
                {
                    _logger.LogCritical("Opening DB Connection");
                    DbCommand cmd = this.GetCommand(connection, procedureName, commandType);
                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }
                    _logger.LogCritical("Executing data reader");
                    values.Load(cmd.ExecuteReader(CommandBehavior.CloseConnection));
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Failed to GetDataReader for " + procedureName, ex, parameters);
                throw;
            }

            return values;
        }
    }

}
