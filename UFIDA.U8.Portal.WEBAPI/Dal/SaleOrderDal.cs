using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Xml.Linq;
using UFIDA.U8.Portal.WEBAPI.Dal;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{
    public class SaleOrderDal
    {
        public static string SaleOrderChangeVerify(SaleOrder so)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(so.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[cCode]未传递！\",\"U8Code\":\" }";
                }
                if (string.IsNullOrEmpty(so.cChangeVerifier))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单审核人未传递！\",\"U8Code\":\" }";
                }
                sql = " select * from SO_SOMain where CSOCODE = '" + so.cCode + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + so.cCode + "]不存在！\",\"U8Code\":\"\"}";
                }
                DateTime ChangeVerifierDate;
                try
                {
                    ChangeVerifierDate = Convert.ToDateTime(so.cChangeVerifyDate);
                }
                catch
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核日期[" + so.cChangeVerifyDate + "]格式错误！\",\"U8Code\":\"\"}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单关闭失败！\",\"U8Code\":\"" + so.cCode + "\",\"Items\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }
    }
}