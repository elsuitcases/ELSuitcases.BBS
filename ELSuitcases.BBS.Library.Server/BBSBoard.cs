using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace ELSuitcases.BBS.Library.Server
{
    public class BBSBoard
    {
        #region [SELECT]
        public static Task<DataTable> List()
        {
            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.RefCursor
                }
            };

            return OracleHelper.Select(OracleHelper.GetConnection(), "PKG_BBS_Board.List_All", CommandType.StoredProcedure, parameters);
        }
        #endregion

        #region [WRITE]
        public static Task<int> Create_Board(string bbsId, string bbsName, Constants.BBSBoardType bbsType)
        {
            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_bbs_id",
                    Value = bbsId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_bbs_name",
                    Value = bbsName,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_bbs_type",
                    Value = Convert.ToInt32(bbsType),
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_created_time",
                    Value = DateTime.Now,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Int32
                }
            };

            return OracleHelper.Write(OracleHelper.GetConnection(), "PKG_BBS_Board.Create_Board", CommandType.StoredProcedure, parameters, "p_result");
        }

        public static Task<int> Delete_Board(string bbsId)
        {
            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_bbs_id",
                    Value = bbsId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Value = -1,
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Int32
                }
            };

            return OracleHelper.Write(OracleHelper.GetConnection(), "PKG_BBS_Board.Delete_Board", CommandType.StoredProcedure, parameters, "p_result");
        }

        public static Task<int> Update_Board(string bbsId, string bbsName, Constants.BBSBoardType bbsType, bool isEnabled = true)
        {
            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_bbs_id",
                    Value = bbsId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_bbs_name",
                    Value = bbsName,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_bbs_type",
                    Value = Convert.ToInt32(bbsType),
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_is_enabled",
                    Value = isEnabled ? "1" : "0",
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Value = -1,
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Int32
                }
            };

            return OracleHelper.Write(OracleHelper.GetConnection(), "PKG_BBS_Board.Update_Board", CommandType.StoredProcedure, parameters, "p_result");
        }
        #endregion
    }
}
