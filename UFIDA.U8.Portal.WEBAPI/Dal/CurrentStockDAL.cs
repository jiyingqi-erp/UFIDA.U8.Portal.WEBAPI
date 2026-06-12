using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{
    public class CurrentStockDAL
    {
        private static string zt = ConfigurationManager.AppSettings["zt"].ToString();
        private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();

        public static string QueryCurrentStocks(QCurrentStock stock)
        {
            string sql = "";
            try
            {
                int pageIndex = stock.PageIndex <= 0 ? 1 : stock.PageIndex;
                int pageSize = stock.PageSize <= 0 ? 10 : stock.PageSize;
                int startRow = (pageIndex - 1) * pageSize;

                string[] invCodes = !string.IsNullOrEmpty(stock.InvCode) ?
                    stock.InvCode.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries) : null;
                string[] whCodes = !string.IsNullOrEmpty(stock.WhCode) ?
                    stock.WhCode.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries) : null;

                if (invCodes != null && invCodes.Length > 0)
                {
                    string invCodeList = string.Join("','", invCodes);
                    sql = "SELECT cInvCode, cInvName, cInvStd, cComUnitCode FROM Inventory WHERE cInvCode IN ('" + invCodeList + "')";
                    DataTable dtInventory = U8SqlDBHelper.GetDataTable(sql);

                    List<string> notExistInvs = new List<string>();
                    foreach (string inv in invCodes)
                    {
                        bool exists = false;
                        foreach (DataRow row in dtInventory.Rows)
                        {
                            if (row["cInvCode"].ToString().Equals(inv, StringComparison.OrdinalIgnoreCase))
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists)
                        {
                            notExistInvs.Add(inv);
                        }
                    }
                    if (notExistInvs.Count > 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + string.Join(",", notExistInvs) + "]不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }

                if (whCodes != null && whCodes.Length > 0)
                {
                    string whCodeList = string.Join("','", whCodes);
                    sql = "SELECT cWhCode, cWhName FROM Warehouse WHERE cWhCode IN ('" + whCodeList + "')";
                    DataTable dtWarehouse = U8SqlDBHelper.GetDataTable(sql);

                    List<string> notExistWhs = new List<string>();
                    foreach (string wh in whCodes)
                    {
                        bool exists = false;
                        foreach (DataRow row in dtWarehouse.Rows)
                        {
                            if (row["cWhCode"].ToString().Equals(wh, StringComparison.OrdinalIgnoreCase))
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists)
                        {
                            notExistWhs.Add(wh);
                        }
                    }
                    if (notExistWhs.Count > 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + string.Join(",", notExistWhs) + "]不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }

                sql = @"
                    SELECT 
                        stock.AutoID,
                        stock.cWhCode,
                        wh.cWhName,
                        stock.cInvCode,
                        inv.cInvName,
                        inv.cInvStd,
                        inv.cComUnitCode,
                        stock.iQuantity
                    FROM V_CurrentStock stock
                    LEFT JOIN Inventory inv ON stock.cInvCode = inv.cInvCode
                    LEFT JOIN Warehouse wh ON stock.cWhCode = wh.cWhCode
                    WHERE stock.iQuantity <> 0
                ";

                if (invCodes != null && invCodes.Length > 0)
                {
                    string invCodeList = string.Join("','", invCodes);
                    sql += " AND stock.cInvCode IN ('" + invCodeList + "')";
                }
                if (whCodes != null && whCodes.Length > 0)
                {
                    string whCodeList = string.Join("','", whCodes);
                    sql += " AND stock.cWhCode IN ('" + whCodeList + "')";
                }

                string countSql = "SELECT COUNT(*) FROM (" + sql + ") AS Temp";
                int totalCount = Convert.ToInt32(U8SqlDBHelper.ExecuteScalar(countSql));

                if (invCodes != null && invCodes.Length == 1 && totalCount == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + invCodes[0] + "]库存为0！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }

                string pagedSql = sql + " ORDER BY stock.cWhCode, stock.cInvCode, stock.AutoID OFFSET " + startRow + " ROWS FETCH NEXT " + pageSize + " ROWS ONLY";
                DataTable stockTable = U8SqlDBHelper.GetDataTable(pagedSql);

                List<CurrentStock> stockList = new List<CurrentStock>();
                foreach (DataRow row in stockTable.Rows)
                {
                    string GetString(string col) => row[col] == DBNull.Value ? string.Empty : row[col].ToString();
                    decimal GetDecimal(string col) => row[col] == DBNull.Value ? 0m : Convert.ToDecimal(row[col]);

                    CurrentStock item = new CurrentStock
                    {
                        AutoID = GetString("AutoID"),
                        WhCode = GetString("cWhCode"),
                        WhName = GetString("cWhName"),
                        InvCode = GetString("cInvCode"),
                        InvName = GetString("cInvName"),
                        InvStd = GetString("cInvStd"),
                        ComUnitCode = GetString("cComUnitCode"),
                        iQuantity = GetDecimal("iQuantity")
                    };
                    stockList.Add(item);
                }

                string jsonResult = JsonConvert.SerializeObject(stockList);
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return "{\"Code\":\"200\",\"Msg\":\"查询成功\",\"U8Code\":\"\",\"Items\":" + jsonResult +
                       ",\"TotalCount\":" + totalCount + ",\"TotalPages\":" + totalPages + ",\"PageIndex\":" + pageIndex + ",\"PageSize\":" + pageSize + "}";
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
