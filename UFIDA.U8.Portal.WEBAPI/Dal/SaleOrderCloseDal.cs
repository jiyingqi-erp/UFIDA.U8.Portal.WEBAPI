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
    public class SaleOrderCloseDal
    {
        /// <summary>
        /// 销售订单关闭
        /// 逻辑：
        ///   1. 校验 voucher_code、person_code 必传
        ///   2. 查询 SO_SOMain 确认订单存在
        ///   3. 检查订单状态（已审核 + 未关闭 才允许关闭）
        ///      - iStatus=0 未审核 → 不可关闭
        ///      - cCloser 非空 → 已关闭，不可重复关闭
        ///   4. 用 person_code 查 person 表 → 取 cPersonName + 校验存在
        ///   5. 写入 cCloser（=cPersonName） / dCloseDate（关闭日期） / dCloseSystime（关闭时间）
        ///      注意：iStatus 保持 1 不变，U8 通过 cCloser 是否为空判断关闭状态
        /// </summary>
        public static string Close(SaleOrderClose so)
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
                    return "{\"Code\":\"400\",\"Msg\":\"关闭人员工编码[person_code]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 2. 查询订单是否存在 ============
                sql = " select * from SO_SOMain where CSOCODE = '" + voucherCode + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"销售订单号[" + voucherCode + "]不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 3. 检查订单状态 ============
                string currentStatus = dt.Rows[0]["iStatus"].ToString();
                string currentCloser = dt.Rows[0]["cCloser"].ToString();

                if (currentStatus == "0")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"关闭失败！销售订单[" + voucherCode + "]当前为未审核状态，请先审核后再关闭。\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                if (currentStatus != "1")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"关闭失败！销售订单[" + voucherCode + "]当前状态[iStatus=" + currentStatus + "]非已审核状态，不可关闭。\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                if (!string.IsNullOrEmpty(currentCloser))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"关闭失败！销售订单[" + voucherCode + "]已关闭，关闭人[" + currentCloser + "]，不可重复关闭。\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 4. person_code → cPersonName ============
                //    person 表: cPersonCode（编码）、cPersonName（姓名）
                sql = " select cPersonName from person where cPersonCode = '" + personCode + "' ";
                string cPersonName = U8SqlDBHelper.GetString(sql);
                if (string.IsNullOrEmpty(cPersonName))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + personCode + "]在U8人员表中不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 5. 执行关闭 UPDATE ============
                sql = " update SO_SOMain set "
                    + " cCloser = '" + cPersonName + "', "
                    + " dCloseDate = '" + DateTime.Now.ToString("yyyy-MM-dd") + "', "
                    + " dCloseSystime = getdate() "
                    + " where CSOCODE = '" + voucherCode + "' ";

                sqlList.Add(sql);
                U8SqlDBHelper.ExecuteSqlTran(sqlList);

                return "{\"Code\":\"200\",\"Msg\":\"关闭成功！\",\"U8Code\":\"" + voucherCode + "\",\"Items\":\"\"}";
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
