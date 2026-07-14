using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{
    public class InvoiceDal
    {
        /// <summary>
        /// 已开发票号回传（写入SaleBillVouch.cDefine12，逗号分隔叠加）
        /// </summary>
        public static string SaleBillInvoiceBackfill(SaleBillInvoice sbi)
        {
            string sql = "";
            try
            {
                if (string.IsNullOrWhiteSpace(sbi.cSBVCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发票单据号[cSBVCode]未传递！\",\"U8Code\":\"\"}";
                }
                if (string.IsNullOrWhiteSpace(sbi.cInvoiceNo))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"已开发票号[cInvoiceNo]未传递！\",\"U8Code\":\"\"}";
                }

                // 1. 查询发票是否存在
                sql = " SELECT cSBVCode,cDefine12 FROM SaleBillVouch WHERE cSBVCode = '" + sbi.cSBVCode + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发票单据[" + sbi.cSBVCode + "]不存在！\",\"U8Code\":\"\"}";
                }

                string currentDefine12 = dt.Rows[0]["cDefine12"].ToString();

                // 2. 检查发票号是否已存在（避免重复）
                if (!string.IsNullOrWhiteSpace(currentDefine12))
                {
                    string[] existingInvoices = currentDefine12.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string inv in existingInvoices)
                    {
                        if (inv.Trim() == sbi.cInvoiceNo.Trim())
                        {
                            return "{\"Code\":\"200\",\"Msg\":\"已开发票号[" + sbi.cInvoiceNo + "]已存在，不可重复录入！\",\"U8Code\":\"\"}";
                        }
                    }
                }

                // 3. 计算叠加后的长度（cDefine12为nvarchar(120)）
                string newDefine12;
                if (string.IsNullOrWhiteSpace(currentDefine12))
                {
                    newDefine12 = sbi.cInvoiceNo.Trim();
                }
                else
                {
                    newDefine12 = currentDefine12 + "," + sbi.cInvoiceNo.Trim();
                }

                if (newDefine12.Length > 120)
                {
                    int remaining = 120 - currentDefine12.Length;
                    return "{\"Code\":\"400\",\"Msg\":\"已开发票号超出长度限制（120字符）！当前已用" + currentDefine12.Length + "字符，剩余" + remaining + "字符，无法容纳[" + sbi.cInvoiceNo + "]。\",\"U8Code\":\"\"}";
                }

                // 4. 更新cDefine12（叠加写入）
                sql = " UPDATE SaleBillVouch SET cDefine12 = '" + newDefine12 + "' WHERE cSBVCode = '" + sbi.cSBVCode + "' ";
                U8SqlDBHelper.ExecuteSqlTran(new List<string> { sql });

                return "{\"Code\":\"200\",\"Msg\":\"已开发票号回传成功！\",\"U8Code\":\"\",\"Items\":\"\"}";
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
