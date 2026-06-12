using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.DBHelp
{
    public class VoucherCode
    {
        public static string GetVoucherCode(string DBNAME, string CardNumber, out int cNumber, out string sqlStatement)
        {
            int num = 10;
            cNumber = 0;
            string safeSql = "select isnull(GlideLen,1) iSize from  " + DBNAME + "..VoucherNumber where CardNumber='" + CardNumber + "'";
            DataTable dataTable = SQLHelper.GetDataTable(safeSql);
            if (dataTable.Rows.Count > 0)
            {
                num = Convert.ToInt32(dataTable.Rows[0]["iSize"]);
            }
            safeSql = "select cNumber from " + DBNAME + "..VoucherHistory where CardNumber='" + CardNumber + "'";
            dataTable = SQLHelper.GetDataTable(safeSql);
            if (dataTable.Rows.Count > 0)
            {
                cNumber = Convert.ToInt32(dataTable.Rows[0]["cNumber"]);
                sqlStatement = "update " + DBNAME + "..VoucherHistory set cNumber=cNumber+1 where CardNumber='" + CardNumber + "'";
            }
            else
            {
                cNumber++;
                sqlStatement = "insert into " + DBNAME + "..VoucherHistory(CardNumber,cNumber) values('" + CardNumber + "'," + cNumber + ")";
            }
            return string.Format("{0:D" + num + "}", cNumber);
        }

        public static string GetVoucherID(string cAcc_Id, string cVouchType, out int iFatherId, out int iChildId, out string sqlment)
        {
            iFatherId = 0;
            iChildId = 0;
            string safeSql = "select isnull(iFatherId,0) iFatherId,isnull(iChildId,0) iChildId from UFSystem..UA_Identity where cAcc_Id = '" + cAcc_Id + "' and cVouchType='" + cVouchType + "'";
            DataTable dataTable = SQLHelper.GetDataTable(safeSql);
            if (dataTable.Rows.Count > 0)
            {
                iFatherId = Convert.ToInt32(dataTable.Rows[0]["iFatherId"]);
                iChildId = Convert.ToInt32(dataTable.Rows[0]["iChildId"]);
                sqlment = "";
            }
            else
            {
                sqlment = "insert into UFSystem..UA_Identity(cAcc_Id,cVouchType,iFatherId,iChildId) values('" + cAcc_Id + "','" + cVouchType + "'," + iFatherId + "," + iChildId + ")";
            }
            return sqlment;
        }
    }
}
