using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{
    public class PoDAL
    {
        private static string zt = ConfigurationManager.AppSettings["zt"].ToString();

        private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();
        public static string PoMain(Pomain p)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(p.cmaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cmaker]未传递！\"}";
                }
                if (string.IsNullOrEmpty(p.cverifier))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核人[cverifier]未传递！\"}";
                }
                if (string.IsNullOrEmpty(p.ccode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据编号[ccode]未传递！\"}";
                }
                sql = " select 1 from po_pomain where cpoid = '" + p.ccode + "' ";
                DataTable checkResult = U8SqlDBHelper.GetDataTable(sql);
                if (checkResult.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + p.ccode + "]已存在！\"}";
                }
                if (string.IsNullOrEmpty(p.Ddate))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据日期[Ddate]未传递！\",\"U8Code\":\"\"}";
                }
                DateTime dateTime;
                try
                {
                    dateTime = Convert.ToDateTime(p.Ddate);
                }
                catch
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + p.Ddate + "]格式错误！\",\"U8Code\":\"\"}";
                }
                string yearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
                sql = " select bflag_PU from GL_mend where iYPeriod = '" + yearMonth + "' ";
                string closeFlag = U8SqlDBHelper.GetString(sql);
                if (closeFlag == "True")
                {
                    return "{\"Code\":\"400\",\"Msg\":\" 日期[" + p.Ddate + "]采购已关账！\"}";
                }
                if (!string.IsNullOrEmpty(p.cvencode))
                {
                    sql = " select 1 from Vendor where isnull(dEndDate,'2099-01-01')>getdate() and bVenCargo=1 and cVenCode = '" + p.cvencode + "' ";
                    checkResult = U8SqlDBHelper.GetDataTable(sql);
                    if (checkResult.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"供应商编码[" + p.cvencode + "]无效！\"}";
                    }
                }
                if (string.IsNullOrEmpty(p.cdepcode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[cdepcode]未传递！\"}";
                }
                sql = " select 1 from Department where bDepEnd = '1' and cDepCode = '" + p.cdepcode + "' ";
                checkResult = U8SqlDBHelper.GetDataTable(sql);
                if (checkResult.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + p.cdepcode + "]无数据或不是末级部门！\"}";
                }
                if (!string.IsNullOrEmpty(p.cpersoncode))
                {
                    sql = " select 1 from person where cPersonCode = '" + p.cpersoncode + "' ";
                    checkResult = U8SqlDBHelper.GetDataTable(sql);
                    if (checkResult.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + p.cpersoncode + "]无数据或已停用！\"}";
                    }
                }
                if (string.IsNullOrEmpty(p.cexch_name))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"币种[cexch_name]未传递！\"}";
                }
                if (string.IsNullOrEmpty(p.nflat))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"汇率[nflat]未传递！\"}";
                }
                decimal exchangeRate = BasicDAL.ToDec(p.nflat);
                if (string.IsNullOrEmpty(p.cDefine14))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"付款方式[cDefine14]未传递！\"}";
                }
                List<PoDetails> items = p.Items;
                string venCodeFromAppVouch = "";
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].cpoid))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"请购单号[cpoid]未传递！\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].vouchrowno))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"请购单行号[vouchrowno]未传递！\"}";
                    }
                    sql = "select cVenCode from PU_AppVouchs a (nolock) left join PU_AppVouch b (nolock) on a.ID=b.ID  where b.cCode = '" + items[i].cpoid + "' and ivouchrowno = '" + items[i].vouchrowno + "' and isnull(cbcloser,'')=''";
                    checkResult = U8SqlDBHelper.GetDataTable(sql);
                    if (checkResult.Rows.Count == 0)
                    {
                        LogException.WriteJSlog("Po_Pomain", sql, "请购单号[" + items[i].cpoid + "]行号[" + items[i].vouchrowno + "]无数据或已停用！\"");
                        return "{\"Code\":\"400\",\"Msg\":\"请购单号[" + items[i].cpoid + "]行号[" + items[i].vouchrowno + "]无数据或已停用！\"}";
                    }
                    if (string.IsNullOrEmpty(p.cvencode))
                    {
                        if (i == 0)
                        {
                            venCodeFromAppVouch = checkResult.Rows[0][0].ToString();
                        }
                        else if (venCodeFromAppVouch != checkResult.Rows[0][0].ToString())
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"请购单明细供应商编码不一致！\"}";
                        }
                    }
                    if (string.IsNullOrEmpty(items[i].dArriveDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"计划到货日期[dArriveDate]未传递！\",\"U8Code\":\"\"}";
                    }
                    try
                    {
                        DateTime dateTime2 = Convert.ToDateTime(items[i].dArriveDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"计划到货日期[" + items[i].dArriveDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].cinvcode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[cinvcode]未传递！\"}";
                    }
                    sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cinvcode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                    DataTable inventoryResult = U8SqlDBHelper.GetDataTable(sql);
                    if (inventoryResult.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cinvcode + "]无数据或已停用！\"}";
                    }
                    if (items[i].iquantity <= 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"出库数量[iquantity]不可为0！\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].irowno))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"行号[irowno]未传递！\"}";
                    }
                }
                int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "Pomain");
                string pomainId = "1" + $"{vouchId:D9}";
                string busTypeName = "普通采购";
                if (!string.IsNullOrEmpty(p.cBusType))
                {
                    busTypeName = p.cBusType;
                }
                sql = "select a.cVenCode,b.cCode from PU_AppVouchs a(nolock) left join PU_AppVouch b (nolock) on a.ID=b.ID  where b.cCode = '" + items[0].cpoid + "' and ivouchrowno = '" + items[0].vouchrowno + "'  ";
                DataTable appVouchResult = U8SqlDBHelper.GetDataTable(sql);
                string finalVenCode = "";
                finalVenCode = (string.IsNullOrEmpty(p.cvencode) ? appVouchResult.Rows[0]["cVenCode"].ToString() : p.cvencode);
                sql = string.Concat(" insert into po_pomain ( poid,cpoid,dpodate,cvencode,cdepcode,cappcode, cptcode,cexch_name,nflat,itaxrate,cmemo,cstate,cperiod,cmaker,ivtid,cbustype,   idiscounttaxtype,iswfcontrolled,cmaketime,iflowid,iprintcount,ccontactcode,cvenperson,cvenbank,cvenaccount,csysbarcode,  cAuditTime,cAuditDate,cVerifier,iverifystateex,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select '", pomainId, "','", p.ccode, "','", dateTime.ToString("yyyy-MM-dd"), "','", finalVenCode, "','", p.cdepcode, "' , ccode , '01' ,'", p.cexch_name, "',", exchangeRate, ", 13,'", p.remark, "', 1 ,null,'", p.cmaker, "','8173','", busTypeName, "',  0,0,getdate(),0,0, null, null, null, null, '||pupo|", p.ccode, "',  getdate(), '", dateTime.ToString("yyyy-MM-dd"), "' , '", p.cverifier, "' , 2,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,'", p.cDefine14, "',cDefine15,cDefine16   from PU_AppVouch where ccode = '", appVouchResult.Rows[0]["cCode"], "' ");
                sqlList.Add(sql);
                List<ReItems> reItemsList = new List<ReItems>();
                int rowNo = 0;
                for (int j = 0; j < items.Count; j++)
                {
                    ReItems reItems = new ReItems();
                    decimal iPerTaxRate = items[j].iPerTaxRate;
                    decimal iTaxPrice = items[j].iTaxPrice;
                    decimal iSum = items[j].iSum;
                    decimal iUnitPrice = items[j].iUnitPrice;
                    decimal iMoney = items[j].iMoney;
                    decimal iTax = items[j].iTax;
                    decimal iNatUnitPrice = items[j].iNatUnitPrice;
                    decimal natTaxPrice = iTaxPrice * exchangeRate;
                    decimal iNatMoney = items[j].iNatMoney;
                    decimal iNatSum = items[j].iNatSum;
                    decimal iNatTax = items[j].iNatTax;
                    sql = "select a.autoid from PU_AppVouchs a(nolock) left join PU_AppVouch b (nolock) on a.ID=b.ID  where b.cCode = '" + items[j].cpoid + "' and ivouchrowno = '" + items[j].vouchrowno + "'  ";
                    string appVouchsAutoId = U8SqlDBHelper.GetString(sql);
                    DateTime arriveDate = Convert.ToDateTime(items[j].dArriveDate);
                    rowNo = BasicDAL.ToInt(items[j].irowno);
                    int vouchId2 = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
                    string podetailId = "1" + $"{vouchId2:D9}";
                    sql = " insert into PO_Podetails (ID,POID,cinvcode,iquantity,iunitprice,iMoney,iTax,  iSum,iNatUnitPrice,inatmoney,inattax,inatsum,ipertaxrate,bgsp,itaxprice,  iAppIds,btaxcost,ivouchrowno,cbsysbarcode,bgift,cSource,SoType,iordertype,cupsocode,  cItem_class,cItemCode,cItemName ,dArriveDate,   cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )  select '" + podetailId + "','" + pomainId + "','" + items[j].cinvcode + "'," + items[j].iquantity + ", " + iUnitPrice + ", " + iMoney + "," + iTax + ",  " + iSum + " ," + iNatUnitPrice + ", " + iNatMoney + "," + iNatTax + "," + iNatSum + ",  " + iPerTaxRate + ",0, " + iTaxPrice + ",  autoid ,1, " + rowNo + ",'||pupo|" + p.ccode + "|" + rowNo.ToString() + "' ,0, 'app', '0','0','" + items[j].cpoid + "' ,  cItem_class,cItemCode,cItemName , '" + arriveDate.ToString("yyyy-MM-dd") + "' , cDefine22,'" + items[j].cinvversion + "',cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37   from PU_AppVouchs (nolock) where autoid = '" + appVouchsAutoId + "' ";
                    sqlList.Add(sql);
                    sql = "update PU_AppVouchs set iReceivedQTY=isnull(iReceivedQTY,0)+" + items[j].iquantity + " where autoid = '" + appVouchsAutoId + "' ";
                    sqlList.Add(sql);
                    reItems.FID = appVouchsAutoId;
                    reItems.U8IDS = podetailId;
                    reItemsList.Add(reItems);
                }
                int executeResult = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (executeResult > 0)
                {
                    string reItemsJson = JsonConvert.SerializeObject(reItemsList);
                    return "{\"Code\":\"200\",\"Msg\":\"U8采购订单[" + p.ccode + "]创建成功！\",\"data\": {\"U8Code\":\"" + p.ccode + "\",\"U8ID\":\"" + pomainId + "\",\"Items\": " + reItemsJson + "  } }";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U8采购订单创建失败！\",\"data\":\"\" }";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
            }
        }

        public static string Upt_Po(Pomain p)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(p.ccode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据编号[ccode]未传递！\"}";
                }
                sql = " select POID from po_pomain where cpoid = '" + p.ccode + "' ";
                DataTable checkResult = U8SqlDBHelper.GetDataTable(sql);
                if (checkResult.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + p.ccode + "]无数据！\"}";
                }
                string poid = checkResult.Rows[0][0].ToString();
                int rowNo = BasicDAL.ToInt(p.irowno);
                if (rowNo == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"行号[" + p.irowno + "]无效！\"}";
                }
                sql = "select ID from PO_Podetails where POID = '" + poid + "' and ivouchrowno = " + rowNo + " ";
                checkResult = U8SqlDBHelper.GetDataTable(sql);
                if (checkResult.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + p.ccode + "]行号[" + rowNo + "]无数据！\"}";
                }
                string podetailId = checkResult.Rows[0][0].ToString();
                if (!string.IsNullOrEmpty(p.dArriveDate))
                {
                    DateTime dateTime;
                    try
                    {
                        dateTime = Convert.ToDateTime(p.dArriveDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"预计到货日期[" + p.dArriveDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                    sql = "update PO_Podetails set dArriveDate = '" + dateTime.ToString("yyyy-MM-dd") + "' where ID = '" + podetailId + "' ";
                    sqlList.Add(sql);
                }
                if (!string.IsNullOrEmpty(p.cbCloseDate))
                {
                    DateTime dateTime2;
                    try
                    {
                        dateTime2 = Convert.ToDateTime(p.cbCloseDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"关闭日期[" + p.cbCloseDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                    if (string.IsNullOrEmpty(p.cbCloser))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"关闭人[cbCloser]未传递！\"}";
                    }
                    sql = "update PO_Podetails set cbCloseDate = '" + dateTime2.ToString("yyyy-MM-dd") + "',cbCloseTime=getdate(),cbCloser='" + p.cbCloser + "' where ID = '" + podetailId + "' ";
                    sqlList.Add(sql);
                    sql = "select 1 from PO_Podetails where poid = '" + poid + "' and ID<>'" + podetailId + "' and isnull(cbCloser,'')='' ";
                    checkResult = U8SqlDBHelper.GetDataTable(sql);
                    if (checkResult.Rows.Count == 0)
                    {
                        sql = "update po_pomain set dCloseDate='" + dateTime2.ToString("yyyy-MM-dd") + "',dCloseTime=getdate(),cCloser='" + p.cbCloser + "' where poid = '" + poid + "'";
                        sqlList.Add(sql);
                    }
                }
                int executeResult = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (executeResult > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"U8采购订单[" + p.ccode + "]更新成功！\", ,\"data\": {\"U8Code\":\"" + p.ccode + "\",\"U8ID\":\"" + poid + "\" }  }";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U8采购订单更新失败！\",\"data\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
            }
        }

        public static string PurBill(PurBill p)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(p.cPBVMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cPBVMaker]未传递！\"}";
                }
                if (string.IsNullOrEmpty(p.invoicecode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发票号[invoicecode]未传递！\"}";
                }
                if (string.IsNullOrEmpty(p.SRMID))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"组合单号[SRMID]未传递！\"}";
                }

                // 根据发票类型生成最终发票号（前缀 + U8入库单号）
                string prefix = p.invoicetype == "ZP" ? "PS-" : (p.invoicetype == "PP" ? "PT-" : "");
                if (string.IsNullOrEmpty(prefix))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发票类型[invoicetype]不正确（应为ZP或PP）！\"}";
                }
                string finalInvoiceCode = prefix + p.cInCode;


                sql = " select 1 from PurBillVouch where cPBVCode = '" + p.invoicecode + "' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发票号[" + p.invoicecode + "]已存在！\"}";
                }
                if (string.IsNullOrEmpty(p.ddate))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"开票日期[ddate]未传递！\",\"U8Code\":\"\"}";
                }
                DateTime dateTime;
                try
                {
                    dateTime = Convert.ToDateTime(p.ddate);
                }
                catch
                {
                    return "{\"Code\":\"400\",\"Msg\":\"开票日期[" + p.ddate + "]格式错误！\",\"U8Code\":\"\"}";
                }
                string yearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
                sql = " select bflag_AP from GL_mend where iYPeriod = '" + yearMonth + "' ";
                string closeFlag = U8SqlDBHelper.GetString(sql);
                if (closeFlag == "True")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"开票日期[" + p.ddate + "]应收已关账！\"}";
                }
                if (string.IsNullOrEmpty(p.invoicetype))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发票类型[invoicetype]未传递！\"}";
                }
                if (string.IsNullOrEmpty(p.csource))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据来源[csource]未传递！\"}";
                }
                if (p.invoicetype != "ZP" && p.invoicetype != "PP")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发票类型[" + p.invoicetype + "]不正确！\"}";
                }
                if (p.csource != "采购" && p.csource != "委外")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据来源[" + p.csource + "]不正确！\"}";
                }
                if (string.IsNullOrEmpty(p.cvencode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"供应商编号[cvencode]未传递！\"}";
                }
                sql = " select * from Vendor (nolock) where cVenCode = '" + p.cvencode + "'  ";
                DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable2.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"供应商编号[" + p.cvencode + "]无数据！\"}";
                }
                if (!string.IsNullOrEmpty(p.cdepcode))
                {
                    sql = " select bDepEnd from department where cdepcode = '" + p.cdepcode + "' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"部门[" + p.cdepcode + "]不存在！\"}";
                    }
                    if (dataTable.Rows[0]["bDepEnd"].ToString() == "False")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"部门[" + p.cdepcode + "]不是末级部门！\"}";
                    }
                }
                if (!string.IsNullOrEmpty(p.cpersoncode))
                {
                    sql = " select 1 from Person where cPersonCode = '" + p.cpersoncode + "' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"业务员[" + p.cpersoncode + "]不存在！\"}";
                    }
                }
                if (string.IsNullOrEmpty(p.cexch_name))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"币种[cexch_name]未传递！\"}";
                }
                if (string.IsNullOrEmpty(p.nflat))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"汇率[nflat]未传递！\"}";
                }
                decimal num = BasicDAL.ToDec(p.nflat);
                List<PurBills> items = p.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].cinvcode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[cinvcode]未传递！\"}";
                    }
                    sql = " select 1 from inventory (nolock) where cInvCode = '" + items[i].cinvcode + "' ";
                    DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable3.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 存货编码[" + items[i].cinvcode + "]不存在！\"}";
                    }
                    if (items[i].iquantity == 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]不可为0！\"}";
                    }
                    if (p.bNegative == "1" && items[i].iquantity > 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"红票出库数量[iQuantity]不可大于0！\"}";
                    }
                    sql = " select b.cbustype from rdrecords01 a (nolock) left join rdrecord01 b (nolock) on a.id=b.id  where a.autoid = '" + items[i].rdids + "' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 入库单明细ID[" + items[i].cinvcode + "]不存在！\"}";
                    }
                    if (p.csource == "委外" && dataTable.Rows[0][0].ToString() != "委外加工")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 入库单明细ID[" + items[i].cinvcode + "]不是委外来源！\"}";
                    }
                    if (p.csource == "采购" && dataTable.Rows[0][0].ToString() == "委外加工")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 入库单明细ID[" + items[i].cinvcode + "]不是采购来源！\"}";
                    }
                }
                sql = " select ID from rdrecords01 (nolock) where autoid = '" + items[0].rdids + "' ";
                DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                string billType = "01";
                string iDiscountTaxType = "0";
                if (p.invoicetype == "PP")
                {
                    billType = "02";
                    iDiscountTaxType = "1";
                }
                string depCode = "null";
                if (!string.IsNullOrEmpty(p.cdepcode))
                {
                    depCode = "'" + p.cdepcode + "'";
                }
                string personCode = "null";
                if (!string.IsNullOrEmpty(p.cpersoncode))
                {
                    personCode = "'" + p.cpersoncode + "'";
                }
                string bNegative = "0";
                if (p.bNegative == "1")
                {
                    bNegative = "1";
                }
                int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "PURBILL");
                string pbvId = "1" + $"{vouchId:D9}";
                sql = string.Concat(" insert into PurB  illVouch (\r\n                    PBVID,cPBVBillType,cPBVCode,cPTCode,dPBVDate,cVenCode,cUnitCode,\r\n                    cDepCode,cPersonCode,cPayCode,cexch_name,cExchRate,iPBVTaxRate,cPBVMemo,cOrderCode,dVouDate,\r\n                    cBusType,cPBVMaker,bNegative,bOriginal,bFirst,citem_class,citemcode,\r\n                    iNetLock,iVTid,cSource,iDiscountTaxType,cVenPUOMProtocol,cInCode,\r\n                    iPrintCount,cVerifier,cAuditDate,cAuditTime,iverifystateex,IsWfControlled,csysbarcode,cmaketime,bMerger,\r\n                    cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8, \r\n                    cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 ) \r\n                    select \r\n '", pbvId, "','", billType, "','", p.invoicecode, "',cPTCode ,convert(varchar(10),getdate(),121),'", p.cvencode, "','", p.cvencode, "', \r\n ", depCode, ",", personCode, ",null,'", p.cexch_name, "',", num, ",", items[0].iTaxRate, ",'", p.cPBVMemo, "',null,'", dateTime.ToString("yyyy-MM-dd"), "',  \r\n cBusType,'", p.cPBVMaker, "' ,", bNegative, ",0,0,null,null, \r\n  0, '8163','", p.csource, "','", iDiscountTaxType, "' , null,'", finalInvoiceCode, "',  \r\n 0,null , null , null ,null , 0, '||puzl|", p.invoicecode, "',getdate(),0,   cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16   from rdrecord01 where ID = '", dataTable4.Rows[0]["ID"], "' ");
                sqlList.Add(sql);
                sql = " INSERT INTO EF_OM_MOMain (cCode,MOID)VALUES('" + p.invoicecode + "','" + pbvId + "') ";
                sqlList.Add(sql);
                int num2 = 0;
                for (int j = 0; j < items.Count; j++)
                {
                    string cinvcode = items[j].cinvcode;
                    decimal iquantity = items[j].iquantity;
                    string iNum = "";
                    string iRate;
                    string AssUnit;
                    decimal assQty = BasicDAL.GetAssQty(zt, cinvcode, iquantity, out iNum, out iRate, out AssUnit);
                    decimal iOriTaxPrice = items[j].iOriTaxPrice;
                    decimal iOriSum = items[j].iOriSum;
                    decimal iOriCost = items[j].iOriCost;
                    decimal iOriTaxCost = items[j].iOriTaxCost;
                    decimal iOriMoney = items[j].iOriMoney;
                    decimal iTaxRate = items[j].iTaxRate;
                    decimal num3 = Math.Round(iOriCost * num, 6);
                    decimal num4 = Math.Round(iOriMoney * num, 2);
                    decimal num5 = Math.Round(iOriTaxPrice * num, 2);
                    decimal num6 = Math.Round(iOriSum * num, 2);
                    num2++;
                    int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "PURBILL");
                    string pbvsId = "1" + $"{vouchId2:D9}";
                    sql = "insert into PurBillVouchs ( \r\n                        ID,PBVID,cInvCode,bExBill,iPBVQuantity,iNum,iOriCost,iOriMoney,\r\n                        iOriTaxPrice,iOriSum,iCost,iMoney,iTaxPrice,iSum,\r\n                        iExMoney,iLostQuan,iNLostQuan,iNLostMoney,iOriTotal,iTotal,cDebitHead,iTaxRate,iPOsID,\r\n                        cItem_class,cItemCode,cItemName, mNLostTax,iOriTaxCost,RdsId,UpSoType,dInDate,\r\n                        bCosting,bTaxCost,iinvexchrate,brettax,ivouchrowno,cbsysbarcode,bgift,\r\n                        cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29, \r\n                        cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )\r\n                        select  \r\n '" + pbvsId + "','" + pbvId + "','" + cinvcode + "',0 ," + iquantity + "," + iNum + "," + iOriCost + "," + iOriMoney + ",  \r\n " + iOriTaxPrice + "," + iOriSum + "," + num3 + ", " + num4 + "," + num5 + "," + num6 + ", \r\n 0, 0, 0, 0, 0, 0, null, " + iTaxRate + ", iPOsID, \r\n cItem_class,cItemCode,cName, 0, " + iOriTaxCost + ", autoid , 'rd', b.ddate, \r\n 0, 1, null,null, " + num2 + ",'||puzl|" + p.invoicecode + "|" + num2 + "', 0 ,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37   from rdrecords01 a left join rdrecord01 b on a.id=b.id where autoid = '" + items[j].rdids + "' ";
                    sqlList.Add(sql);
                    sql = " update rdrecords01 set iSumBillQuantity = isnull(iSumBillQuantity,0)+ " + iquantity + " where autoid = " + items[j].rdids + " ";
                    sqlList.Add(sql);
                    if (p.csource == "采购")
                    {
                        string iPOsID = U8SqlDBHelper.GetString("select iPOsID from rdrecords01 where autoid = '" + items[j].rdids + "'");
                        sql = " update PO_Podetails set iInvQTY =isnull(iInvQTY,0)+ " + iquantity + ",   iInvMoney=isnull(iInvMoney,0)+" + num6 + "  , iNatInvMoney=isnull(iNatInvMoney,0)+" + iOriSum + "  where ID = " + iPOsID + " ";
                        sqlList.Add(sql);
                    }
                    if (p.csource == "委外")
                    {
                        string iOMoMID = U8SqlDBHelper.GetString("select iomodid from rdrecords01 where autoid = '" + items[j].rdids + "'");
                        sql = " update OM_MODetails set iInvQTY=isnull(iInvQTY,0)+" + iquantity + ",iInvMoney=isnull(iInvMoney,0)+" + num6 + ",  iNatInvMoney=isnull(iNatInvMoney,0)+" + iOriSum + " where MODetailsID='" + iOMoMID + "' ";
                        sqlList.Add(sql);
                    }
                }
                int num7 = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (num7 > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"U8采购发票[" + p.invoicecode + "]创建成功！\", \"data\": {\"U8Code\":\"" + p.invoicecode + "\",\"U8ID\":\"" + pbvId + "\" }  }";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U8采购发票创建失败！\",\"data\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"data\":\"\"}";
            }
        }

        public static string AddPriceJust(PuPriceJust p)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(p.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"调价单号[cCode]未传递！\"}";
                }
                if (string.IsNullOrEmpty(p.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
                }
                sql = " select 1 from PU_PriceJustMain (nolock) where ccode = '" + p.cCode + "' and isnull(cverifier,'')='' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"调价单号[" + p.cCode + "]已存在！\"}";
                }
                if (string.IsNullOrEmpty(p.dDate))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
                }
                DateTime dateTime;
                try
                {
                    dateTime = Convert.ToDateTime(p.dDate);
                }
                catch
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + p.dDate + "]格式错误！\",\"U8Code\":\"\"}";
                }
                string yearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
                sql = " select bflag_PU from GL_mend where iYPeriod = '" + yearMonth + "' ";
                string closeFlag = U8SqlDBHelper.GetString(sql);
                if (closeFlag == "True")
                {
                    return "{\"Code\":\"400\",\"Msg\":\" 日期[" + p.dDate + "]采购已关账！\"}";
                }
                if (!string.IsNullOrEmpty(p.iSupplyType) && p.iSupplyType != "1" && p.iSupplyType != "2" && p.iSupplyType != "3" && p.iSupplyType != "4")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"供应类型[" + p.iSupplyType + "]值无效！\"}";
                }
                if (!string.IsNullOrEmpty(p.cDepCode))
                {
                    sql = " select 1 from Department where bDepEnd = '1' and cDepCode = '" + p.cDepCode + "' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + p.cDepCode + "]无数据或不是末级部门！\"}";
                    }
                }
                List<PriceJustDetails> items = p.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                    }
                    sql = " select 1 from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                    DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable2.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].cVenCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"供应商[cVenCode]未传递！\"}";
                    }
                    sql = " select 1 from Vendor where isnull(dEndDate,'2099-01-01')>getdate() and cVenCode = '" + items[i].cVenCode + "' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"供应商编码[" + items[i].cVenCode + "]无效！\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].cexch_name))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"币种[cexch_name]未传递！\"}";
                    }
                    if (BasicDAL.ToDec(items[i].iTaxUnitPrice) == 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"含税单价[" + items[i].iTaxUnitPrice + "]无效！\"}";
                    }
                    if (BasicDAL.ToDec(items[i].iUnitPrice) == 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"无税单价[" + items[i].iUnitPrice + "]无效！\"}";
                    }
                    if (BasicDAL.ToDec(items[i].iTaxRate) == 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"税率[" + items[i].iTaxRate + "]无效！\"}";
                    }
                    if (!string.IsNullOrEmpty(items[i].dstartdate))
                    {
                        try
                        {
                            DateTime dateTime2 = Convert.ToDateTime(items[i].dstartdate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"生效日期[" + items[i].dstartdate + "]格式错误！\",\"U8Code\":\"\"}";
                        }
                    }
                    if (!string.IsNullOrEmpty(items[i].denddate))
                    {
                        try
                        {
                            DateTime dateTime2 = Convert.ToDateTime(items[i].denddate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"失效日期[" + items[i].denddate + "]格式错误！\",\"U8Code\":\"\"}";
                        }
                    }
                }
                int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "puprice");
                string priceJustId = "1" + $"{vouchId:D9}";
                string iSupplyType = "1";
                if (!string.IsNullOrEmpty(p.iSupplyType))
                {
                    iSupplyType = p.iSupplyType;
                }
                string depCode = "NULL";
                if (!string.IsNullOrEmpty(p.cDepCode))
                {
                    depCode = "'" + p.cDepCode + "'";
                }
                string finalMemo = (p.cMemo != null && p.cMemo.Length > 255) ? "" : p.cMemo;

                sql = " insert into PU_PriceJustMain (  id,ddate,ccode,cmaker,cmainmemo,  cverifier,dverifydate,ivtid,iverifystate,iswfcontrolled,bTaxCost,iSupplyType,  cMakeTime,cAuditTime,iPrintCount,csysbarcode,cDepCode)  values (  '" + priceJustId + "','" + dateTime.ToString("yyyy-MM-dd") + "','" + p.cCode + "','" + p.cMaker + "','" + finalMemo + "',  null,null,'30963', 0, 0, 1, " + iSupplyType + ",  getdate(), null, 0, '||putj|" + p.cCode + "', " + depCode + " ) ";

                sqlList.Add(sql);
                int num = 0;
                for (int j = 0; j < items.Count; j++)
                {
                    DateTime dateTime3 = DateTime.Now;
                    DateTime dateTime4 = Convert.ToDateTime("2099-12-31");
                    if (!string.IsNullOrEmpty(items[j].dstartdate))
                    {
                        dateTime3 = Convert.ToDateTime(items[j].dstartdate);
                    }
                    if (!string.IsNullOrEmpty(items[j].denddate))
                    {
                        dateTime4 = Convert.ToDateTime(items[j].denddate);
                    }
                    decimal num2 = BasicDAL.ToDec(items[j].fminquantity);
                    decimal num3 = BasicDAL.ToDec(items[j].iTaxUnitPrice);
                    decimal num4 = BasicDAL.ToDec(items[j].iUnitPrice);
                    decimal num5 = BasicDAL.ToDec(items[j].iTaxRate);
                    string finalcbMemo = (items[j].cbMemo != null && items[j].cbMemo.Length > 255) ? "" : items[j].cbMemo;
                    num++;
                    int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "puprice");
                    string detailAutoId = "1" + $"{vouchId2:D9}";
                    sql = " insert into PU_PriceJustDetail (  autoid,id,operationtype,cvencode,cinvcode,cbodymemo,fminquantity,  dstartdate,bsales,iunitprice,itaxrate,itaxunitprice,cexch_name,  ivouchrowno,cbsysbarcode,bEndPriceList,denddate,cDefine23 )  values (  '" + detailAutoId + "','" + priceJustId + "','0','" + items[j].cVenCode + "','" + items[j].cInvCode + "','" + finalcbMemo + "'," + num2 + ",  '" + dateTime3.ToString("yyyy-MM-dd") + "',0, '" + num4 + "','" + num5 + "','" + num3 + "','" + items[j].cexch_name + "',  '" + num + "','||putj|" + p.cCode + "|" + num + "',1,'" + dateTime4.ToString("yyyy-MM-dd") + "','" + items[j].cDefine23 + "' ) ";
                    sqlList.Add(sql);
                    sql = " select itaxunitprice from Ven_Inv_Price (nolock) where cvencode ='" + items[j].cVenCode + "' and cinvcode='" + items[j].cInvCode + "'  and iLowerLimit = " + num2 + " and isnull(ddisabledate,'2099-01-01') > getdate()  ";
                    DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable3.Rows.Count > 0)
                    {
                        decimal num6 = BasicDAL.ToDec(dataTable3.Rows[0]["itaxunitprice"].ToString());
                        sql = " update PU_PriceJustDetail set ijusttaxprice = " + num6 + " where autoid = '" + detailAutoId + "' ";
                        sqlList.Add(sql);
                    }
                }
                int num7 = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (num7 > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"调价单[" + p.cCode + "]创建成功！\", \"U8Code\":\"" + p.cCode + "\" }";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U调价单创建失败！\",\"U8Code\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"data\":\"\"}";
            }
        }

        public static string VerifyPrice(PriceJust p)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                sql = " select * from PU_PriceJustMain (nolock) where ccode = '" + p.cCode + "' and isnull(cverifier,'')='' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"调价单号[" + p.cCode + "]无效！\"}";
                }
                if (string.IsNullOrEmpty(p.cVerifier))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
                }
                string iSupplyType = dataTable.Rows[0]["iSupplyType"].ToString();
                sql = " update PU_PriceJustMain set cAuditTime=getdate() , cverifier='" + p.cVerifier + "' ,  dverifydate=convert(varchar(10),getdate(),121),iverifystate=2   where ccode = '" + p.cCode + "' ";
                sqlList.Add(sql);
                sql = string.Concat(" select * from PU_PriceJustDetail where id = '", dataTable.Rows[0]["id"], "' ");
                DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                for (int i = 0; i < dataTable2.Rows.Count; i++)
                {
                    string cvencode = dataTable2.Rows[i]["cvencode"].ToString();
                    string cinvcode = dataTable2.Rows[i]["cinvcode"].ToString();
                    decimal num = BasicDAL.ToDec(dataTable2.Rows[i]["fminquantity"].ToString());
                    DateTime dateTime = Convert.ToDateTime(dataTable2.Rows[i]["dstartdate"]);
                    DateTime dateTime2 = Convert.ToDateTime(dataTable2.Rows[i]["denddate"]);
                    decimal num2 = BasicDAL.ToDec(dataTable2.Rows[i]["iunitprice"].ToString());
                    decimal num3 = BasicDAL.ToDec(dataTable2.Rows[i]["itaxrate"].ToString());
                    decimal num4 = BasicDAL.ToDec(dataTable2.Rows[i]["iTaxUnitPrice"].ToString());
                    sql = " select itaxunitprice from Ven_Inv_Price (nolock) where cvencode ='" + cvencode + "' and cinvcode='" + cinvcode + "'  and isnull(ddisabledate,'2099-01-01') > getdate() and isnull(ilowerlimit,0)=" + num + "  ";
                    DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable3.Rows.Count > 0)
                    {
                        sql = " update Ven_Inv_Price set ddisabledate = '" + dateTime.ToString("yyyy-MM-dd") + "'  where cvencode ='" + cvencode + "' and cinvcode='" + cinvcode + "'  and isnull(ddisabledate,'2099-01-01') > getdate() and isnull(ilowerlimit,0)=" + num + " ";
                        sqlList.Add(sql);
                    }
                    sql = string.Concat(" insert into Ven_Inv_Price (  cvencode,cinvcode,denabledate,dDisableDate,cexch_name,bpromotion,  cmemo,isupplytype,btaxcost,ilowerlimit,iunitprice,itaxrate,itaxunitprice,ipriceautoid  ) values (  '", cvencode, "','", cinvcode, "','", dateTime.ToString("yyyy-MM-dd"), "','", dateTime2.ToString("yyyy-MM-dd"), "','", dataTable2.Rows[i]["cexch_name"], "', 0,  '", dataTable2.Rows[i]["cbodymemo"], "',", iSupplyType, ",1, ", num, " , '", num2, "','", num3, "','", num4, "','", dataTable2.Rows[i]["autoid"], "' ) ");
                    sqlList.Add(sql);
                }
                int num5 = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (num5 > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"调价单[" + p.cCode + "]审核成功！\", \"U8Code\":\"" + p.cCode + "\" }";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U调价单审核失败！\",\"U8Code\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"data\":\"\"}";
            }
        }

        public static string VenAndInv(VenAndInv vai)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                List<Inv> items = vai.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].iSuppProperty))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"供应类型[iSuppProperty]未传递！\",\"U8Code\":\"\"}";
                    }
                    if (items[i].iSuppProperty != "采购供货" && items[i].iSuppProperty != "委外供货")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"供应类型[" + items[i].iSuppProperty + "]值无效！\",\"U8Code\":\"\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\",\"U8Code\":\"\"}";
                    }
                    sql = " select 1 from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                    if (items[i].iSuppProperty == "采购供货")
                    {
                        sql += " and bPurchase=1 ";
                    }
                    if (items[i].iSuppProperty == "委外供货")
                    {
                        sql += " and bProxyForeign=1 ";
                    }
                    DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无效！\",\"U8Code\":\"\"}";
                    }
                    List<Ven> vens = items[i].Vens;
                    if (vens.Count == 0 || vens == null)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"供应商信息[Vens]未传递！\",\"U8Code\":\"\"}";
                    }
                    decimal num = default(decimal);
                    for (int j = 0; j < vens.Count; j++)
                    {
                        if (string.IsNullOrEmpty(vens[j].CVenCode))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"供应商[CVenCode]未传递！\",\"U8Code\":\"\"}";
                        }
                        sql = " select 1 from Vendor where isnull(dEndDate,'2099-01-01')>getdate() and cVenCode = '" + vens[j].CVenCode + "' ";
                        if (items[i].iSuppProperty == "采购供货")
                        {
                            sql += " and bVenCargo=1 ";
                        }
                        if (items[i].iSuppProperty == "委外供货")
                        {
                            sql += " and bProxyForeign=1 ";
                        }
                        dataTable = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"供应商[" + vens[j].CVenCode + "]无效！\",\"U8Code\":\"\"}";
                        }
                        if (vens[j].fQuota == 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"配额%[fQuota]不可为0！\",\"U8Code\":\"\"}";
                        }
                        num += vens[j].fQuota;
                        if (num > 100m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"配额%总和不可超过100！\",\"U8Code\":\"\"}";
                        }
                    }
                }
                for (int k = 0; k < items.Count; k++)
                {
                    sql = " update Inventory_Sub set bPUQuota=1 where cInvSubCode = '" + items[k].cInvCode + "' ";
                    sqlList.Add(sql);
                    sql = "delete from VenAndInv where cinvcode = '" + items[k].cInvCode + "' ";
                    sqlList.Add(sql);
                    List<Ven> vens2 = items[k].Vens;
                    for (int l = 0; l < vens2.Count; l++)
                    {
                        int num2 = 1;
                        if (items[k].iSuppProperty == "委外供货")
                        {
                            num2 = 2;
                        }
                        string fMinSuppNum = "null";
                        string fSupplyBatch = "null";
                        string fAdvDate = "null";
                        if (!string.IsNullOrEmpty(vens2[l].fMinSuppNum))
                        {
                            fMinSuppNum = BasicDAL.ToDec(vens2[l].fMinSuppNum).ToString();
                        }
                        if (!string.IsNullOrEmpty(vens2[l].fSupplyBatch))
                        {
                            fSupplyBatch = BasicDAL.ToDec(vens2[l].fSupplyBatch).ToString();
                        }
                        if (!string.IsNullOrEmpty(vens2[l].fAdvDate))
                        {
                            fAdvDate = BasicDAL.ToInt(vens2[l].fAdvDate).ToString();
                        }
                        sql = "insert into VenAndInv(cvencode,cinvcode,fQuota,isuppproperty,ftotalquota,fFinishRateUp,fMinSuppNum,fSupplyBatch,fAdvDate)  values('" + vens2[l].CVenCode + "','" + items[k].cInvCode + "'," + vens2[l].fQuota + "," + num2 + ",0,100 ," + fMinSuppNum + "," + fSupplyBatch + "," + fAdvDate + ") ";
                        sqlList.Add(sql);
                        if (vens2[l].MainVen == "Y")
                        {
                            sql = " update Inventory set cVenCode = '" + vens2[l].CVenCode + "',iInvAdvance=" + fAdvDate + ",  fMinSupply=" + fMinSuppNum + ",fSupplyMulti=" + fSupplyBatch + " where cinvcode='" + items[k].cInvCode + "' ";
                            sqlList.Add(sql);
                        }
                    }
                }
                int num3 = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (num3 > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"存货供应商对照更新成功！\", \"U8Code\":\"\" }";
                }
                return "{\"Code\":\"400\",\"Msg\":\"存货供应商对照更新失败！\",\"U8Code\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"data\":\"\"}";
            }
        }

        public static string SO_upt(SoMain so)
        {
            List<string> list = new List<string>();
            string text = "";
            try
            {
                if (string.IsNullOrEmpty(so.cSoCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"订单号[cSoCode]未传递！\"}";
                }
                if (string.IsNullOrEmpty(so.cChanger))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"变更人[cChanger]未传递！\"}";
                }
                if (string.IsNullOrEmpty(so.cCusCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"客户编码[cCusCode]未传递！\"}";
                }
                text = " select * from so_somain where cSoCode = '" + so.cSoCode + "' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(text);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]无数据！\"}";
                }
                if (dataTable.Rows[0]["cCloser"].ToString() != "")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]已关闭，不可变更！\"}";
                }
                text = " select 1 from dispatchlists where cSoCode = '" + so.cSoCode + "' ";
                DataTable dataTable2 = U8SqlDBHelper.GetDataTable(text);
                if (dataTable2.Rows.Count > 0)
                {
                    if (so.cCusCode != dataTable.Rows[0]["cCusCode"].ToString())
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]已发货，不可变更客户编码！\"}";
                    }
                    if (so.cexch_name != dataTable.Rows[0]["cexch_name"].ToString())
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]已发货，不可变更币种！\"}";
                    }
                    if (so.iExchRate != BasicDAL.ToDec(dataTable.Rows[0]["cexch_name"].ToString()))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]已发货，不可变更汇率！\"}";
                    }
                }
                if (!string.IsNullOrEmpty(so.cPersonCode))
                {
                    text = " select * from person where cpersoncode = '" + so.cPersonCode + "' ";
                    dataTable2 = U8SqlDBHelper.GetDataTable(text);
                    if (dataTable2.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"业务员编码[" + so.cPersonCode + "]无数据！\"}";
                    }
                }
                List<SODetail> items = so.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].cType))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"操作类型[cType]未传递！\"}";
                    }
                    if (items[i].cType == "新增")
                    {
                        text = " select * from SO_SODetails where csocode='" + so.cSoCode + "' and iRowNo = " + items[i].iRowNo + " ";
                        DataTable dataTable3 = U8SqlDBHelper.GetDataTable(text);
                        if (dataTable3.Rows.Count > 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]已有数据，不可新增！\"}";
                        }
                        if (string.IsNullOrEmpty(items[i].cInvCode))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                        }
                        if (BasicDAL.ToDec(items[i].iQuantity) <= 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"订单数量[" + BasicDAL.ToDec(items[i].iQuantity) + "]错误！\"}";
                        }
                        if (string.IsNullOrEmpty(items[i].dPreDate))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"预发货日期[dPreDate]未传递！\"}";
                        }
                        try
                        {
                            DateTime dateTime = Convert.ToDateTime(items[i].dPreDate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"预完工日期[" + items[i].dPreDate + "]格式错误！\"}";
                        }
                        if (string.IsNullOrEmpty(items[i].dPreMoDate))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"预完工日期[dPreMoDate]未传递！\"}";
                        }
                        try
                        {
                            DateTime dateTime2 = Convert.ToDateTime(items[i].dPreMoDate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"预完工日期[" + items[i].dPreMoDate + "]格式错误！\"}";
                        }
                        if (BasicDAL.ToDec(items[i].iTaxRate) <= 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"税率[" + BasicDAL.ToDec(items[i].iTaxRate) + "]错误！\"}";
                        }
                        if (BasicDAL.ToDec(items[i].iTaxUnitPrice) <= 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"原币含税单价[" + BasicDAL.ToDec(items[i].iTaxUnitPrice) + "]错误！\"}";
                        }
                    }
                    else if (items[i].cType == "修改")
                    {
                        text = " select * from SO_SODetails where csocode='" + so.cSoCode + "' and iRowNo = " + items[i].iRowNo + " ";
                        DataTable dataTable4 = U8SqlDBHelper.GetDataTable(text);
                        if (dataTable4.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]无数据！\"}";
                        }
                        if (Convert.ToString(dataTable4.Rows[0]["cSCloser"]) != "")
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]已关闭，不可修改！\"}";
                        }
                        if (string.IsNullOrEmpty(items[i].cInvCode))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                        }
                        if (BasicDAL.ToDec(items[i].iQuantity) <= 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"订单数量[" + BasicDAL.ToDec(items[i].iQuantity) + "]错误！\"}";
                        }
                        if (string.IsNullOrEmpty(items[i].dPreDate))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"预发货日期[dPreDate]未传递！\"}";
                        }
                        try
                        {
                            DateTime dateTime3 = Convert.ToDateTime(items[i].dPreDate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"预完工日期[" + items[i].dPreDate + "]格式错误！\"}";
                        }
                        if (string.IsNullOrEmpty(items[i].dPreMoDate))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"预完工日期[dPreMoDate]未传递！\"}";
                        }
                        try
                        {
                            DateTime dateTime4 = Convert.ToDateTime(items[i].dPreMoDate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"预完工日期[" + items[i].dPreMoDate + "]格式错误！\"}";
                        }
                        if (BasicDAL.ToDec(items[i].iTaxRate) <= 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"税率[" + BasicDAL.ToDec(items[i].iTaxRate) + "]错误！\"}";
                        }
                        if (BasicDAL.ToDec(items[i].iTaxUnitPrice) <= 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"原币含税单价[" + BasicDAL.ToDec(items[i].iTaxUnitPrice) + "]错误！\"}";
                        }
                        text = string.Concat("select 1 from dispatchlists where iSOsID = '", dataTable4.Rows[0]["iSOsID"], "' ");
                        dataTable2 = U8SqlDBHelper.GetDataTable(text);
                        if (dataTable2.Rows.Count > 0)
                        {
                            if (items[i].cInvCode != dataTable4.Rows[0]["cinvcode"].ToString())
                            {
                                return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]已发货，不可变更存货编码！\"}";
                            }
                            decimal num = BasicDAL.ToDec(dataTable4.Rows[0]["iFHQuantity"].ToString());
                            if (BasicDAL.ToDec(items[i].iQuantity) < num)
                            {
                                return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]订单数量不可小于已发货数量！\"}";
                            }
                            if (BasicDAL.ToDec(items[i].iTaxRate) != BasicDAL.ToDec(dataTable4.Rows[0]["iTaxRate"].ToString()))
                            {
                                return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]已发货，不可变更税率！\"}";
                            }
                            if (BasicDAL.ToDec(items[i].iTaxUnitPrice) != BasicDAL.ToDec(dataTable4.Rows[0]["iTaxUnitPrice"].ToString()))
                            {
                                return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]已发货，不可变更单价！\"}";
                            }
                        }
                    }
                    else
                    {
                        if (!(items[i].cType == "删除"))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"操作类型[" + items[i].cType + "]错误！\"}";
                        }
                        text = " select * from SO_SODetails where csocode='" + so.cSoCode + "' and iRowNo = " + items[i].iRowNo + " ";
                        DataTable dataTable5 = U8SqlDBHelper.GetDataTable(text);
                        if (dataTable5.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]无数据！\"}";
                        }
                        if (Convert.ToString(dataTable5.Rows[0]["cSCloser"]) != "")
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]已关闭，不可删除！\"}";
                        }
                        text = string.Concat("select 1 from dispatchlists where iSOsID = '", dataTable5.Rows[0]["iSOsID"], "' ");
                        dataTable2 = U8SqlDBHelper.GetDataTable(text);
                        if (dataTable2.Rows.Count > 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"订单号[" + so.cSoCode + "]行[" + items[i].iRowNo + "]已发货，不可删除！\"}";
                        }
                    }
                }
                string text2 = "";
                if (!string.IsNullOrEmpty(so.cPersonCode))
                {
                    text2 = text2 + " ,cPersonCode='" + so.cPersonCode + "' ";
                }
                text2 = ((!(dataTable.Rows[0]["cVerifier"].ToString() != "")) ? (text2 + " ,cmodifier='" + so.cChanger + "',dmodifysystime=getdate(),dmoddate=CONVERT(varchar(10),getdate(),121) ") : (text2 + " ,cChanger='" + so.cChanger + "',cChangeVerifier='" + so.cChanger + "',dChangeVerifyTime=getdate(),dChangeVerifyDate=CONVERT(varchar(10),getdate(),121) "));
                if (so.cCusCode != dataTable.Rows[0]["cCusCode"].ToString())
                {
                    string text3 = U8SqlDBHelper.GetString("select ccusname from customer where ccuscode = '" + so.cCusCode + "'");
                    string text4 = U8SqlDBHelper.GetString("select cCusOAddress from customer where ccuscode = '" + so.cCusCode + "'");
                    text2 = text2 + " ,ccusname='" + text3 + "',cinvoicecompany='" + so.cCusCode + "',cCusOAddress='" + text4 + "'";
                }
                text = " update SO_SOMain set cCusCode='" + so.cCusCode + "',cexch_name='" + so.cexch_name + "',  iExchRate=" + so.iExchRate + ",cMemo='" + so.cMemo + "' " + text2 + " where cSoCode = '" + so.cSoCode + "' ";
                list.Add(text);
                for (int j = 0; j < items.Count; j++)
                {
                    if (items[j].cType == "删除")
                    {
                        text = " delete from SO_SODetails where csocode='" + so.cSoCode + "' and iRowNo = " + items[j].iRowNo + " ";
                        list.Add(text);
                        continue;
                    }
                    decimal num2 = BasicDAL.ToDec(items[j].iQuantity);
                    decimal num3 = BasicDAL.ToDec(items[j].iTaxRate);
                    decimal num4 = (100m + num3) / 100m;
                    string cInvCode = items[j].cInvCode;
                    string text5 = U8SqlDBHelper.GetString("select cinvname from Inventory where cinvcode = '" + items[j].cInvCode + "'");
                    DateTime dateTime5 = Convert.ToDateTime(items[j].dPreDate);
                    DateTime dateTime6 = Convert.ToDateTime(items[j].dPreMoDate);
                    decimal num5 = BasicDAL.ToDec(items[j].iTaxUnitPrice);
                    decimal num6 = Math.Round(num5 * num2, 2);
                    decimal num7 = Math.Round(num6 / num4, 2);
                    decimal num8 = Math.Round(num7 / num2, 6);
                    decimal num9 = num6 - num7;
                    decimal num10 = Math.Round(num6 * so.iExchRate, 2);
                    decimal num11 = Math.Round(num7 * so.iExchRate, 2);
                    decimal num12 = num10 - num11;
                    decimal num13 = Math.Round(num11 / num2, 2);
                    if (items[j].cType == "新增")
                    {
                        string text6 = U8SqlDBHelper.GetString("select ID from so_somain where cSoCode = '" + so.cSoCode + "'");
                        string text7 = "1" + BasicDAL.GetVouchId("C", cAcc_Id, "Somain").ToString().PadLeft(9, '0');
                        text = "insert into SO_SODetails ( \r\n                                ID,iSOsID,cSOCode,cInvCode,dPreDate,iQuantity,\r\n                                iUnitPrice,iTaxUnitPrice,iMoney,iTax,iSum,iDisCount,iNatUnitPrice,iNatMoney,iNatTax,iNatSum,iNatDisCount,cMemo,\r\n                                KL,KL2,cInvName,iTaxRate,dPreMoDate,iRowNo,bOrderBOM,bOrderBOMOver,idemandtype,busecusbom,bsaleprice,bgift\r\n                                ) values ( '" + text6 + "','" + text7 + "','" + so.cSoCode + "','" + items[j].cInvCode + "','" + dateTime5.ToString("yyyy-MM-dd") + "'," + num2 + ",  " + num8 + "," + num5 + "," + num7 + "," + num9 + "," + num6 + ",0," + num13 + "," + num11 + "," + num12 + "," + num10 + ",0,'" + items[j].cMemo + "',  100,100,'" + text5 + "'," + num3 + ",'" + dateTime6.ToString("yyyy-MM-dd") + "'," + items[j].iRowNo + ", 0 ,0,1,0,1,0  ) ";
                        list.Add(text);
                    }
                    if (items[j].cType == "修改")
                    {
                        text = " select * from SO_SODetails where csocode='" + so.cSoCode + "' and iRowNo = " + items[j].iRowNo + " ";
                        DataTable dataTable6 = U8SqlDBHelper.GetDataTable(text);
                        string text8 = "";
                        if (items[j].cInvCode != dataTable6.Rows[0]["cinvcode"].ToString())
                        {
                            text8 = text8 + " ,cInvCode='" + items[j].cInvCode + "' ";
                        }
                        if (BasicDAL.ToDec(items[j].iQuantity) < BasicDAL.ToDec(dataTable6.Rows[0]["iQuantity"].ToString()))
                        {
                            text8 = text8 + " ,iQuantity='" + items[j].iQuantity + "' ";
                        }
                        if (BasicDAL.ToDec(items[j].iTaxRate) != BasicDAL.ToDec(dataTable6.Rows[0]["iTaxRate"].ToString()))
                        {
                            text8 = text8 + " ,iTaxRate='" + items[j].iTaxRate + "' ";
                        }
                        if (BasicDAL.ToDec(items[j].iTaxUnitPrice) != BasicDAL.ToDec(dataTable6.Rows[0]["iTaxUnitPrice"].ToString()))
                        {
                            text8 = string.Concat(text8, " ,iUnitPrice=" + num8 + ",iTaxUnitPrice=" + num5 + ",iMoney=" + num7 + ",iSum=" + num6 + "  ,iTax=" + num9 + ",iNatUnitPrice=" + num13 + ",iNatMoney=" + num11 + ",iNatTax=" + num12 + ",iNatSum=" + num10);
                        }
                        text = " update SO_SODetails set dPreDate='" + dateTime5.ToString("yyyy-MM-dd") + "',  dPreMoDate='" + dateTime6.ToString("yyyy-MM-dd") + "',cMemo='" + items[j].cMemo + "' " + text8 + " where csocode='" + so.cSoCode + "' and iRowNo = " + items[j].iRowNo + " ";
                        list.Add(text);
                    }
                }
                dataTable?.Dispose();
                dataTable2?.Dispose();
                int num14 = U8SqlDBHelper.ExecuteSqlTran(list);
                if (num14 > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"U8销售订单[" + so.cSoCode + "]变更完成！\",\"U8Code\":\"" + so.cSoCode + "\" }";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U8销售订单变更失败！\",\"U8Code\":\"\" }";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, text);
                string text9 = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + text9 + "\",\"U8Code\":\"\"}";
            }
        }
    }
}