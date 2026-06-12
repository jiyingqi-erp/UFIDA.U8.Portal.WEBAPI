using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.DBHelp
{
    public class InventoryValidator
    {
        public static InventoryValidationResult ValidateInventory(string dbName, string inventoryCode, int rowIndex, string batchNo, params string[] freeItems)
        {
            InventoryValidationResult inventoryValidationResult = new InventoryValidationResult();
            if (string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(inventoryCode))
            {
                inventoryValidationResult.ErrorCount++;
                inventoryValidationResult.ErrorMessage = "数据库名称或存货编码不能为空";
                return inventoryValidationResult;
            }
            try
            {
                string safeSql = $"\r\n                SELECT \r\n                    ISNULL(bInvBatch, 0) AS bInvBatch,\r\n                    ISNULL(bFree1, 0) AS bFree1,\r\n                    ISNULL(bFree2, 0) AS bFree2,\r\n                    ISNULL(bFree3, 0) AS bFree3,\r\n                    ISNULL(bFree4, 0) AS bFree4,\r\n                    ISNULL(bFree5, 0) AS bFree5,\r\n                    ISNULL(bFree6, 0) AS bFree6,\r\n                    ISNULL(bFree7, 0) AS bFree7,\r\n                    ISNULL(bFree8, 0) AS bFree8,\r\n                    ISNULL(bFree9, 0) AS bFree9,\r\n                    ISNULL(bFree10, 0) AS bFree10\r\n                FROM {dbName}..Inventory \r\n                WHERE cinvcode = '" + inventoryCode + "'";
                var anon = new { inventoryCode };
                DataTable dataTable = SQLHelper.GetDataTable(safeSql);
                if (dataTable.Rows.Count == 0)
                {
                    inventoryValidationResult.ErrorCount++;
                    inventoryValidationResult.ErrorMessage += $"第[{rowIndex}]行存货编码为{inventoryCode}的存货不存在，";
                    return inventoryValidationResult;
                }
                DataRow configRow = dataTable.Rows[0];
                StringBuilder stringBuilder = new StringBuilder();
                ValidateBatchNo(configRow, batchNo, inventoryCode, rowIndex, inventoryValidationResult, stringBuilder);
                for (int i = 0; i < 10; i++)
                {
                    int freeNo = i + 1;
                    string freeValue = ((freeItems.Length > i) ? freeItems[i] : string.Empty);
                    ValidateFreeItem(dbName, configRow, freeNo, freeValue, inventoryCode, rowIndex, inventoryValidationResult, stringBuilder);
                }
                inventoryValidationResult.WhereCondition = ((stringBuilder.Length > 0) ? (" AND " + stringBuilder.ToString() + " 1=1 ") : string.Empty);
            }
            catch (Exception ex)
            {
                inventoryValidationResult.ErrorCount++;
                inventoryValidationResult.ErrorMessage += $"第[{rowIndex}]行校验失败：{ex.Message}，";
            }
            if (inventoryValidationResult.ErrorMessage.EndsWith(","))
            {
                inventoryValidationResult.ErrorMessage = inventoryValidationResult.ErrorMessage.TrimEnd(',');
            }
            return inventoryValidationResult;
        }

        private static void ValidateBatchNo(DataRow configRow, string batchNo, string inventoryCode, int rowIndex, InventoryValidationResult result, StringBuilder whereBuilder)
        {
            if (batchNo != "pomain")
            {
                bool flag = Convert.ToInt32(configRow["bInvBatch"]) == 1;
                if (flag && string.IsNullOrEmpty(batchNo))
                {
                    result.ErrorCount++;
                    result.ErrorMessage += $"第[{rowIndex}]行存货编码为{inventoryCode}的存货需要批号，";
                }
                else if (flag && !string.IsNullOrEmpty(batchNo))
                {
                    whereBuilder.Append($" cbatch='{EscapeSqlValue(batchNo)}' and ");
                }
            }
        }

        private static void ValidateFreeItem(string dbName, DataRow configRow, int freeNo, string freeValue, string inventoryCode, int rowIndex, InventoryValidationResult result, StringBuilder whereBuilder)
        {
            string columnName = $"bFree{freeNo}";
            bool flag = Convert.ToInt32(configRow[columnName]) == 1;
            if (flag && string.IsNullOrEmpty(freeValue))
            {
                result.ErrorCount++;
                result.ErrorMessage += $"第[{rowIndex}]行存货编码为{inventoryCode}的存货需要自由项{freeNo}，";
            }
            else if (!flag && !string.IsNullOrEmpty(freeValue))
            {
                result.ErrorCount++;
                result.ErrorMessage += $"第[{rowIndex}]行存货编码为{inventoryCode}的存货不需要自由项{freeNo}，";
            }
            else if (flag && !string.IsNullOrEmpty(freeValue))
            {
                string safeSql = $"\r\n                SELECT COUNT(1) \r\n                FROM {dbName}..userdefine \r\n                WHERE cValue = '" + freeValue + "' AND cID IN (SELECT cID FROM " + dbName + "..Userdef WHERE cItem = '自由项" + freeNo + "')";
                if (Convert.ToInt32(SQLHelper.ExecuteScalar(safeSql)) == 0)
                {
                    result.ErrorCount++;
                    result.ErrorMessage += $"第[{rowIndex}]行存货编码为{inventoryCode}的存货自由项{freeNo}输入不合法，";
                }
                else
                {
                    whereBuilder.Append($" cfree{freeNo}='{EscapeSqlValue(freeValue)}' and ");
                }
            }
        }

        private static string EscapeSqlValue(string value)
        {
            return value?.Replace("'", "''") ?? string.Empty;
        }
    }
}
