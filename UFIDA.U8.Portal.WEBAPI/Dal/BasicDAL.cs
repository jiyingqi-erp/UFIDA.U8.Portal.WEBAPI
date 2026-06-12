using System;
using System.Data;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{
    public class BasicDAL
    {
        public static string PreTrans(string Jsons)
        {
            Jsons = Jsons.Replace("%", "%25");
            Jsons = Jsons.Replace("+", "%2B");
            Jsons = Jsons.Replace(" ", "%20");
            Jsons = Jsons.Replace("/", "%2F");
            Jsons = Jsons.Replace("?", "%3F");
            Jsons = Jsons.Replace("#", "%23");
            Jsons = Jsons.Replace("&", "%26");
            Jsons = Jsons.Replace("=", "%3D");
            return Jsons;
        }

        public static decimal ToDec(string str)
        {
            try
            {
                return Convert.ToDecimal(str);
            }
            catch
            {
                return 0m;
            }
        }

        public static int ToInt(string str)
        {
            try
            {
                return Convert.ToInt32(str);
            }
            catch
            {
                return 0;
            }
        }

        public static int GetVouchId(string fcid, string cAcc_Id, string cVouchType)
        {
            string sql = "";
            try
            {
                int num = 0;
                int num2 = 0;
                sql = " select iFatherId,iChildId from UFSystem..UA_Identity where cAcc_Id='" + cAcc_Id + "' and cVouchType='" + cVouchType + "' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    sql = " insert into UFSystem..UA_Identity (cAcc_Id,cVouchType,iFatherId,iChildId)  values('" + cAcc_Id + "','" + cVouchType + "',0,0) ";
                    U8SqlDBHelper.ExecuteSql(sql);
                }
                else
                {
                    num = Convert.ToInt32(dataTable.Rows[0]["iFatherId"].ToString());
                    num2 = Convert.ToInt32(dataTable.Rows[0]["iChildId"].ToString());
                }
                if (fcid == "F")
                {
                    num++;
                    sql = " update UFSystem..UA_Identity set iFatherId=iFatherId+1 where cAcc_Id='" + cAcc_Id + "' and cVouchType='" + cVouchType + "' ";
                    U8SqlDBHelper.ExecuteSql(sql);
                    return num;
                }
                num2++;
                sql = " update UFSystem..UA_Identity set iChildId=iChildId+1 where cAcc_Id='" + cAcc_Id + "' and cVouchType='" + cVouchType + "' ";
                U8SqlDBHelper.ExecuteSql(sql);
                return num2;
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return 0;
            }
        }

        public static decimal GetAssQty(string DBName, string cInvCode, decimal MainQty, out string iNum, out string iRate, out string AssUnit)
        {
            try
            {
                decimal result = default(decimal);
                iNum = "null";
                iRate = "null";
                AssUnit = "null";
                string safeSql = " select * from Inventory (nolock) where cinvcode = '" + cInvCode + "' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
                if (dataTable.Rows.Count == 0)
                {
                    return result;
                }
                string text = dataTable.Rows[0]["cGroupCode"].ToString();
                string errorMsg = dataTable.Rows[0]["cComUnitCode"].ToString();
                string unitCode = dataTable.Rows[0]["cSTComUnitCode"].ToString();
                if (string.IsNullOrEmpty(unitCode))
                {
                    return result;
                }
                safeSql = " select iChangRate from ComputationUnit where cGroupCode = '" + text + "' and cComunitCode = '" + errorMsg + "' ";
                decimal num = ToDec(U8SqlDBHelper.GetString(safeSql));
                safeSql = " select iChangRate from ComputationUnit where cGroupCode = '" + text + "' and cComunitCode = '" + unitCode + "' ";
                decimal num2 = ToDec(U8SqlDBHelper.GetString(safeSql));
                if (num2 == 0m || num == 0m)
                {
                    return result;
                }
                AssUnit = "'" + unitCode + "'";
                iRate = "'" + num2 + "'";
                result = Math.Round(MainQty / num2 * num, 6);
                iNum = result.ToString();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsBatch(string cinvcode, string DBName)
        {
            string safeSql = " select bInvBatch from Inventory where cinvcode = '" + cinvcode + "' ";
            string text = U8SqlDBHelper.GetString(safeSql);
            if (text == "False")
            {
                return false;
            }
            return true;
        }

        public static bool IsPropertyCheck(string cinvcode, string DBName)
        {
            string safeSql = " select bPropertyCheck from Inventory where cinvcode = '" + cinvcode + "' ";
            string text = U8SqlDBHelper.GetString(safeSql);
            if (text == "False")
            {
                return false;
            }
            return true;
        }

        public static bool IsInvQuality(string cinvcode)
        {
            string safeSql = " select bInvQuality from Inventory where cinvcode = '" + cinvcode + "' ";
            string text = U8SqlDBHelper.GetString(safeSql);
            if (text == "False")
            {
                return false;
            }
            return true;
        }

        public static bool bWhPos(string cwhcode)
        {
            string safeSql = " select bWhPos from Warehouse where cwhcode = '" + cwhcode + "' ";
            string text = U8SqlDBHelper.GetString(safeSql);
            if (text == "False")
            {
                return false;
            }
            return true;
        }

        public static bool bTARR()
        {
            string safeSql = " select cValue from AccInformation (nolock) where cName = 'bBigThanArrivalQuantity' ";
            string text = U8SqlDBHelper.GetString(safeSql);
            if (text == "False")
            {
                return false;
            }
            return true;
        }

        public static bool bTOM()
        {
            string safeSql = " select cValue from AccInformation (nolock) where cName = 'bOverOmArrivalIn' ";
            string text = U8SqlDBHelper.GetString(safeSql);
            if (text == "False")
            {
                return false;
            }
            return true;
        }

        public static DateTime GetVDate(string cinvcode, string Mdate)
        {
            try
            {
                DateTime dateTime = Convert.ToDateTime(Mdate);
                DateTime result = dateTime;
                string safeSql = " select * from Inventory where cinvcode = '" + cinvcode + "' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
                int num = ToInt(dataTable.Rows[0]["cMassUnit"].ToString());
                int num2 = ToInt(dataTable.Rows[0]["iMassDate"].ToString());
                switch (num)
                {
                    case 1:
                        result = dateTime.AddYears(num2);
                        break;
                    case 2:
                        result = dateTime.AddMonths(num2);
                        break;
                    case 3:
                        result = dateTime.AddDays(num2);
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetMassdate(string cinvcode)
        {
            try
            {
                string safeSql = " select iMassDate from Inventory where cinvcode = '" + cinvcode + "' ";
                string text = U8SqlDBHelper.GetString(safeSql);
                if (string.IsNullOrEmpty(text))
                {
                    return "null";
                }
                return (text ?? "") ?? "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetMassUnit(string cinvcode)
        {
            try
            {
                string safeSql = " select cMassUnit from Inventory where cinvcode = '" + cinvcode + "' ";
                string text = U8SqlDBHelper.GetString(safeSql);
                if (string.IsNullOrEmpty(text))
                {
                    return "null";
                }
                return (text ?? "") ?? "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
