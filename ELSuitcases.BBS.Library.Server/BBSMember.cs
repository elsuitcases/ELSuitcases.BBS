using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace ELSuitcases.BBS.Library.Server
{
    public class BBSMember
    {
        #region [SELECT]
        public static async Task<DataTable> GetByAccountIDAndPassword(string accountID, string password)
        {
            List<OracleParameter> parameters = new List<OracleParameter>()
            {
                new OracleParameter()
                {
                    ParameterName = "p_account_id",
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Varchar2,
                    Value = accountID ?? string.Empty
                },
                new OracleParameter()
                {
                    ParameterName = "p_account_pw",
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Varchar2,
                    Value = password ?? string.Empty
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.RefCursor
                }
            };

            return await OracleHelper.Select(OracleHelper.GetConnection(), "PKG_BBS_Member.Get_By_Account_IDPW", CommandType.StoredProcedure, parameters);
        }
        #endregion
    }
}
