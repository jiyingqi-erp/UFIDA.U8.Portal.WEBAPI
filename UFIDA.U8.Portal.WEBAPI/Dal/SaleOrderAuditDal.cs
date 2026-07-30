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
    public class SaleOrderAuditDal
    {
        /// <summary>
        /// 销售订单审核
        /// 逻辑：
        ///   1. 校验 voucher_code、person_code 必传
        ///   2. 查询 SO_SOMain 确认订单存在
        ///   3. 检查订单审核状态（iStatus=0 且 cVerifier=NULL 才允许审核）
        ///   4. 用 person_code 查 person 表 → 取 cPersonName + 校验存在
        ///   5. 写入 cVerifier（=cPersonName） / dverifydate / dverifysystime / iStatus=1
        /// </summary>
        public static string Audit(SaleOrderAudit so)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                // ============ 0. 解包嵌套结构 ============
                if (so == null || so.saleorder == null)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"请求参数格式错误！请使用 {\\\"saleorder\\\":{\\\"voucher_code\\\":\\\"...\\\",\\\"person_code\\\":\\\"...\\\"}} 格式。\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                string voucherCode = so.saleorder.voucher_code;
                string personCode = so.saleorder.person_code;

                // ============ 1. 参数必填校验 ============
                if (string.IsNullOrEmpty(voucherCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"销售订单号[voucher_code]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (string.IsNullOrEmpty(personCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核人员工编码[person_code]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 2. 查询订单是否存在 ============
                sql = " select * from SO_SOMain where CSOCODE = '" + voucherCode + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"销售订单号[" + voucherCode + "]不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 3. 检查订单审核状态 ============
                string currentVerifier = dt.Rows[0]["cVerifier"].ToString();
                string currentStatus = dt.Rows[0]["iStatus"].ToString();

                if (!string.IsNullOrWhiteSpace(currentVerifier))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核失败！销售订单[" + voucherCode + "]已审核，审核人[" + currentVerifier + "]，不可重复审核。\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                if (currentStatus != "0")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核失败！销售订单[" + voucherCode + "]当前状态[iStatus=" + currentStatus + "]非待审核状态。\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 4. person_code → cPersonName ============
                //    person 表: cPersonCode（编码）、cPersonName（姓名）
                sql = " select cPersonName from person where cPersonCode = '" + personCode + "' ";
                string cPersonName = U8SqlDBHelper.GetString(sql);
                if (string.IsNullOrEmpty(cPersonName))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + personCode + "]在U8人员表中不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 5. 执行审核 UPDATE ============
                sql = " update SO_SOMain set "
                    + " cVerifier = '" + cPersonName + "', "
                    + " dverifydate = '" + DateTime.Now.ToString("yyyy-MM-dd") + "', "
                    + " dverifysystime = getdate(), "
                    + " iStatus = '1' "
                    + " where CSOCODE = '" + voucherCode + "' ";

                sqlList.Add(sql);
                U8SqlDBHelper.ExecuteSqlTran(sqlList);

                return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + voucherCode + "\",\"Items\":\"\"}";
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
