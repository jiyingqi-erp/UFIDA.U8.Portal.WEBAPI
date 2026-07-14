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
                string verifierStatus = dt.Rows[0]["cVerifier"].ToString();
                if (string.IsNullOrWhiteSpace(verifierStatus))
                {
                    // 未审核 → 写入审核人/审核时间，更新状态为已审核（不动cChanger）
                    sql = " update SO_SOMain set cVerifier='" + so.cChangeVerifier + "',dverifydate='" + ChangeVerifierDate.Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',dverifysystime=getdate(),iStatus='1' where CSOCODE='" + so.cCode + "' ";
                }
                else
                {
                    // 已审核 → 先检查iStatus是否为0（变更待审状态）
                    string status = dt.Rows[0]["iStatus"].ToString();
                    if (status != "0")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"变更审核失败！当前订单状态不是变更待审状态[iStatus≠0]。\",\"U8Code\":\"\"}";
                    }
                    // 再检查变更审核人是否已存在
                    string changeVerifier = dt.Rows[0]["cChangeVerifier"].ToString();
                    if (!string.IsNullOrWhiteSpace(changeVerifier))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"变更审核失败！该订单变更已审核，不可重复审核。\",\"U8Code\":\"\"}";
                    }
                    // 再检查cChanger是否有值
                    string changer = dt.Rows[0]["cChanger"].ToString();
                    if (string.IsNullOrWhiteSpace(changer))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"变更审核失败！订单暂无变更人[cChanger]，请先执行变更操作。\",\"U8Code\":\"\"}";
                    }
                    // 写入变更审核人/变更审核时间，更新状态为已审核（不动cChanger）
                    sql = " update SO_SOMain set cChangeVerifier='" + so.cChangeVerifier + "',dChangeVerifyDate='" + ChangeVerifierDate.Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',dChangeVerifyTime=getdate(),iStatus='1' where CSOCODE='" + so.cCode + "' ";
                }
                U8SqlDBHelper.ExecuteSqlTran(new List<string> { sql });
                return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + so.cCode + "\",\"Items\":\"\"}";
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