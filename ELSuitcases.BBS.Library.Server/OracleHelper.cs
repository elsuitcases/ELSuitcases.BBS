﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace ELSuitcases.BBS.Library.Server
{
    internal static class OracleHelper
    {
        private const string ORACLE_SERVER_HOST_NAME = "127.0.0.1";
        private const int ORACLE_SERVER_PORT = 1521;
        private const string ORACLE_SERVER_SERVICE_NAME = "ORCL";
        private const string ORACLE_SERVER_SID = "ORCL";
        private const string ORACLE_USER_NAME = "bbs";
        private const string ORACLE_USER_PASSWORD = "bbs";



        internal static string GenerateConnectionStringAsServiceName(
            string hostName,
            int port,
            string serviceName,
            string userId,
            string userPassword)
        {
            return string.Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={2}))));User Id={3};password={4};",
                            hostName,
                            port,
                            serviceName,
                            userId,
                            userPassword);
        }

        internal static string GenerateConnectionStringAsSID(
            string hostName,
            int port,
            string sid,
            string userId,
            string userPassword)
        {
            return string.Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)(SID={2}))));User Id={3};password={4};",
                            hostName,
                            port,
                            sid,
                            userId,
                            userPassword);
        }

        internal static OracleConnection GetConnection()
        {
            return GetConnectionAsSID(
                        ORACLE_SERVER_HOST_NAME,
                        ORACLE_SERVER_PORT,
                        ORACLE_SERVER_SID,
                        ORACLE_USER_NAME,
                        ORACLE_USER_PASSWORD);
        }

        public static OracleConnection GetConnectionAsServiceName(
            string hostName,
            int port,
            string serviceName,
            string userId,
            string userPassword)
        {
            return new OracleConnection(GenerateConnectionStringAsServiceName(hostName, port, serviceName, userId, userPassword));
        }

        public static OracleConnection GetConnectionAsSID(
            string hostName,
            int port,
            string sid,
            string userId,
            string userPassword)
        {
            return new OracleConnection(GenerateConnectionStringAsSID(hostName, port, sid, userId, userPassword));
        }

        public static bool CheckIsDbConnectionReady(OracleConnection dbConnection)
        {
            return ((dbConnection != null) && (dbConnection.State == ConnectionState.Open));
        }

        public static Task<DataTable> Select(
            OracleConnection dbConnection,
            string commandText,
            CommandType commandType,
            List<OracleParameter>? commandParameters)
        {
            return Task.Factory.StartNew(new Func<DataTable>(delegate ()
            {
                DataTable dt = new DataTable();

                if (dbConnection is null || string.IsNullOrEmpty(commandText))
                    return dt;

                using (OracleCommand comm = new OracleCommand())
                {
                    comm.Connection = dbConnection;
                    comm.CommandText = commandText;
                    comm.CommandType = commandType;
                    if ((commandParameters != null) && (commandParameters.Count > 0))
                        comm.Parameters.AddRange(commandParameters.ToArray());

                    try
                    {
                        using (OracleDataAdapter adapter = new OracleDataAdapter(comm))
                        {
                            adapter.Fill(dt);
                        }

                        Common.PrintDebugInfo(commandText, "OracleHelper.Select");
                    }
                    catch (Exception ex)
                    {
                        Common.PrintDebugFail(ex, "OracleHelper.Select");
                        throw;
                    }
                    finally
                    {
                        if (comm.Connection.State != ConnectionState.Closed)
                            comm.Connection.Close();
                    }
                }

                return dt;
            }));
        }

        public static Task<int> Write(
            OracleConnection dbConnection,
            string commandText,
            CommandType commandType,
            List<OracleParameter>? commandParameters,
            string affectedCountParameterName = "")
        {
            return Task.Factory.StartNew(new Func<int>(delegate ()
            {
                int result = -1;

                using (OracleCommand comm = new OracleCommand())
                {
                    comm.Connection = dbConnection;
                    comm.CommandText = commandText;
                    comm.CommandType = commandType;
                    if ((commandParameters != null) && (commandParameters.Count > 0))
                        comm.Parameters.AddRange(commandParameters.ToArray());

                    try
                    {
                        comm.Connection.Open();

                        using (OracleTransaction trans = comm.Connection.BeginTransaction())
                        {
                            try
                            {
                                int affectedCount = comm.ExecuteNonQuery();
                                int affectedRowsCount = 0;

                                trans.Commit();

                                if ((comm.Parameters.Contains(affectedCountParameterName)) &&
                                    (int.TryParse(comm.Parameters[affectedCountParameterName].Value.ToString(), out affectedRowsCount)))
                                    result = affectedRowsCount;
                                else
                                    result = 1;

                                Common.PrintDebugInfo(string.Format("{0} [RESULT={1}][AFFECTED_COUNT={2}]", commandText, result, affectedCount), "OracleHelper.Write");
                            }
                            catch (Exception)
                            {
                                trans.Rollback();
                                result = -1;
                                throw;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = -1;
                        Common.PrintDebugFail(ex, "OracleHelper.Write");
                        throw;
                    }
                    finally
                    {
                        if (comm.Connection.State != ConnectionState.Closed)
                            comm.Connection.Close();
                    }
                }

                return result;
            }));
        }
    }
}
