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
    public class DispatchAuditDal
    {
        /// <summary>
        /// 销售发货单审核
        /// 
        /// ── 与 Dispatch 新增接口的职责划分 ──
        ///   RDSDAL.Dispatch（新增）：INSERT dispatchlist + 反写CurrentStock/SO_SODetails/IA_SA_UnAccountVouch
        ///     - 新增时 cVerifier/dverifydate/dverifysystime 已写入，iverifystate=0
        ///   DispatchAuditDal（审核）：仅改 iverifystate=1 + 更新cVerifier为审核人
        ///     - 不做任何反写！反写已在新增时完成
        /// 
        /// 逻辑：
        ///   1. 校验 voucher_code、person_code 必传
        ///   2. 查询 DispatchList 确认发货单存在
        ///   3. 检查 iverifystate=0（待审核状态）
        ///      ⚠️ 注意：cVerifier 在新增时已写入，不能校验其是否为空！
        ///   4. 用 person_code 查 person 表 → 取 cPersonName + 校验存在
        ///   5. UPDATE cVerifier/dverifydate/dverifysystime/iverifystate=1
        /// </summary>
        public static string Audit(DispatchAudit da)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                // ============ 0. 解包嵌套结构 ============
                if (da == null || da.consignment == null)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"请求参数格式错误！请使用 {\\\"consignment\\\":{\\\"voucher_code\\\":\\\"...\\\",\\\"person_code\\\":\\\"...\\\"}} 格式。\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                string voucherCode = da.consignment.voucher_code;
                string personCode = da.consignment.person_code;

                // ============ 1. 参数必填校验 ============
                if (string.IsNullOrEmpty(voucherCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货单号[voucher_code]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (string.IsNullOrEmpty(personCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核人员工编码[person_code]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 2. 查询发货单是否存在 ============
                sql = " select cDLCode, cVerifier, iverifystate from DispatchList where cDLCode = '" + voucherCode + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货单号[" + voucherCode + "]不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                // ============ 3. 检查发货单审核状态 ============
                // ⚠️ 关键：Dispatch新增接口已写入cVerifier，所以不能校验 cVerifier=NULL ！
                //    只校验 iverifystate 是否为0（待审核）
                string currentVerifyState = dt.Rows[0]["iverifystate"].ToString();
                string existingVerifier = dt.Rows[0]["cVerifier"].ToString();

                if (currentVerifyState == "1")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核失败！发货单[" + voucherCode + "]已审核（审核人：" + existingVerifier + "），不可重复审核。\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                if (currentVerifyState != "0")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核失败！发货单[" + voucherCode + "]当前状态[iverifystate=" + currentVerifyState + "]非待审核状态。\",\"U8Code\":\"\",\"Items\":\"\"}";
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
                //    ⚠️ 只更新审核状态字段：cVerifier（覆盖为本次审核人） / iverifystate=1
                //    反写（CurrentStock/SO_SODetails/IA_SA_UnAccountVouch）在 Dispatch新增时已完成，审核不重复操作！
                sql = " update DispatchList set "
                    + " cVerifier = '" + cPersonName + "', "
                    + " dverifydate = '" + DateTime.Now.ToString("yyyy-MM-dd") + "', "
                    + " dverifysystime = getdate(), "
                    + " iverifystate = 1 "
                    + " where cDLCode = '" + voucherCode + "' ";

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
