using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Newtonsoft.Json;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{

public class PuArrDAL
{
	private static string zt = ConfigurationManager.AppSettings["zt"].ToString();

	private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();

        public static string PostPuArr(PuArr pu)
        {
            List<string> list = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(pu.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单号[cCode]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (string.IsNullOrEmpty(pu.cBusType))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"业务类型[cBusType]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (pu.cBusType == "普通采购")
                {
                    sql = " select * from PO_Pomain (nolock) where cPOID = '" + pu.cCode + "' and isnull(cCloser,'')='' ";
                    DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"采购订单号[" + pu.cCode + "]不存在或已关闭！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }
                else
                {
                    if (!(pu.cBusType == "委外加工"))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"业务类型[" + pu.cBusType + "]不正确！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    sql = " select * from OM_MOMain (nolock) where cCode = '" + pu.cCode + "' and isnull(cCloser,'')='' ";
                    DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable2.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"委外订单号[" + pu.cCode + "]不存在或已关闭！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }
                if (string.IsNullOrEmpty(pu.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (string.IsNullOrEmpty(pu.cHandler))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cHandler]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (!string.IsNullOrEmpty(pu.cDepCode))
                {
                    sql = " select * from department where cDepCode='" + pu.cDepCode + "' and bDepEnd = 1 ";
                    DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable3.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + pu.cDepCode + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }
                List<PuArrs> items = pu.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].SRMID))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"[SRMID]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    if (pu.cBusType == "普通采购")
                    {
                        sql = " select 1 from PO_Podetails where cDefine24 = '" + items[i].SRMID + "' ";
                        DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable4.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"SRMID[" + items[i].SRMID + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                        }
                    }
                    if (pu.cBusType == "委外加工")
                    {
                        sql = " select 1 from OM_MODetails where cDefine24 = '" + items[i].SRMID + "' ";
                        DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable5.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"SRMID[" + items[i].SRMID + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                        }
                    }
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                    DataTable dataTable6 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable6.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    if (!string.IsNullOrEmpty(items[i].cWhCode))
                    {
                        sql = " select 1 from warehouse  where cwhcode = '" + items[i].cWhCode + "' ";
                        DataTable dataTable7 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable7.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + items[i].cWhCode + "]无数据！\"}";
                        }
                    }
                    string bInvBatch = dataTable6.Rows[0]["bInvBatch"].ToString();
                    string bInvQuality = dataTable6.Rows[0]["bInvQuality"].ToString();
                    if (bInvBatch == "True" && string.IsNullOrEmpty(items[i].cBatch))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cBatch + "]已启用批次管理，需要传递批次[cbatch]！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    try
                    {
                        Convert.ToDecimal(items[i].iQuantity);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"到货数量[" + items[i].iQuantity + "]无效！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }
                string seedYearMonth = DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0');
                int num = 0;
                sql = " select cNumber from VoucherHistory where CardNumber ='26' and cSeed = '" + seedYearMonth + "' ";
                DataTable dataTable8 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable8.Rows.Count > 0)
                {
                    num = Convert.ToInt32(dataTable8.Rows[0]["cNumber"].ToString());
                }
                else
                {
                    U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('26',null,'单据日期','月','" + seedYearMonth + "',0,0) ");
                }
                num++;
                U8SqlDBHelper.ExecuteSql("update VoucherHistory set cNumber=cNumber+1 where CardNumber ='26' and cSeed = '" + seedYearMonth + "' ");
                string voucherCode = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + num.ToString().PadLeft(4, '0');
                int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "PuArrival");
                string vouchIdStr = "1" + $"{vouchId:D9}";
                string depCodeValue = "cDepCode";
                if (!string.IsNullOrEmpty(pu.cDepCode))
                {
                    depCodeValue = "'" + pu.cDepCode + "'";
                }
                if (pu.cBusType == "普通采购")
                {
                    sql = string.Concat(" insert into pu_arrivalvouch(iVTid,ID,cCode,cPTCode,dDate,cVenCode,cDepCode,cPersonCode,cPayCode, cSCCode,cexch_name,iTaxRate,cMemo,cBusType,cMaker,bNegative,  iDiscountTaxType,iBillType,cvouchtype,cMakeTime,iExchRate,  cAuditDate,caudittime,cverifier,iverifystateex,IsWfControlled,iPrintCount,cpocode,csysbarcode,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select '8169','", vouchIdStr, "','", voucherCode, "',cPTCode,'", pu.dDate, "',cVenCode,", depCodeValue, ",cPersonCode,cPayCode,  cSCCode,cexch_name,iTaxRate,'", pu.cMemo, "',cBusType,'", pu.cMaker, "', 0 ,  iDiscountTaxType,0,null,getdate(), nflat, '", pu.dDate, "',getdate(),'", pu.cHandler, "',2,0,0,cPOID, '||pudh|", voucherCode, "' ,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16   from PO_Pomain where cPOID = '", pu.cCode, "' ");
                    list.Add(sql);
                }
                if (pu.cBusType == "委外加工")
                {
                    sql = string.Concat(" insert into pu_arrivalvouch(iVTid,ID,cCode,cPTCode,dDate,cVenCode,cDepCode,cPersonCode,cPayCode, cSCCode,cexch_name,iTaxRate,cMemo,cBusType,cMaker,bNegative,  iDiscountTaxType,iBillType,cvouchtype,cMakeTime,iExchRate,  cAuditDate,caudittime,cverifier,iverifystateex,IsWfControlled,iPrintCount,cpocode,csysbarcode,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select '8169','", vouchIdStr, "','", voucherCode, "',cPTCode,'", pu.dDate, "',cVenCode,", depCodeValue, ",cPersonCode,cPayCode,  cSCCode,cexch_name,iTaxRate,'", pu.cMemo, "',cBusType,'", pu.cMaker, "', 0 ,  0 ,0,null,getdate(), nflat, '", pu.dDate, "',getdate(),'", pu.cHandler, "',2,0,0,cCode, '||omdh|", voucherCode, "' ,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16   from OM_MOMain where cCode = '", pu.cCode, "' ");
                    list.Add(sql);
                }
                List<RetItems> list2 = new List<RetItems>();
                int num2 = 0;
                for (int j = 0; j < items.Count; j++)
                {
                    RetItems retItems = new RetItems();
                    DataTable dataTable9 = new DataTable();
                    DataTable dataTable10 = new DataTable();
                    decimal num3 = default(decimal);
                    decimal num4 = default(decimal);
                    decimal num5 = default(decimal);
                    decimal num6 = default(decimal);
                    if (pu.cBusType == "普通采购")
                    {
                        sql = " select b.cpoid cCode,a.* from PO_Podetails a left join po_pomain b on a.POID=b.POID where cDefine24 = '" + items[j].SRMID + "' ";
                        dataTable9 = U8SqlDBHelper.GetDataTable(sql);
                        num3 = BasicDAL.ToDec(dataTable9.Rows[0]["iPerTaxRate"].ToString());
                        num4 = BasicDAL.ToDec(dataTable9.Rows[0]["iUnitPrice"].ToString());
                        num5 = BasicDAL.ToDec(dataTable9.Rows[0]["iTaxPrice"].ToString());
                        num6 = BasicDAL.ToDec(dataTable9.Rows[0]["iNatUnitPrice"].ToString());
                    }
                    if (pu.cBusType == "委外加工")
                    {
                        sql = " select b.cCode,a.* from OM_MODetails a left join OM_MOMain b on a.MOID=b.MOID where cDefine24 = '" + items[j].SRMID + "' ";
                        dataTable10 = U8SqlDBHelper.GetDataTable(sql);
                        num3 = BasicDAL.ToDec(dataTable10.Rows[0]["iPerTaxRate"].ToString());
                        num4 = BasicDAL.ToDec(dataTable10.Rows[0]["iUnitPrice"].ToString());
                        num5 = BasicDAL.ToDec(dataTable10.Rows[0]["iTaxPrice"].ToString());
                        num6 = BasicDAL.ToDec(dataTable10.Rows[0]["iNatUnitPrice"].ToString());
                    }
                    string cInvCode = items[j].cInvCode;
                    decimal iQuantity = items[j].iQuantity;
                    string iNum = "";
                    string iRate;
                    string AssUnit;
                    decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                    string batchValue = "null";
                    if (BasicDAL.IsBatch(cInvCode, zt) && !string.IsNullOrEmpty(items[j].cBatch))
                    {
                        batchValue = "'" + items[j].cBatch + "'";
                    }
                    string datePValue = "null";
                    string dateVValue = "null";
                    string expDateValue = "null";
                    string massDateValue = "null";
                    string massUnitValue = "null";
                    int num7 = 0;
                    if (BasicDAL.IsPropertyCheck(cInvCode, zt))
                    {
                        num7 = 1;
                    }
                    string whCodeValue = "null";
                    if (!string.IsNullOrEmpty(items[j].cWhCode))
                    {
                        whCodeValue = "'" + items[j].cWhCode + "'";
                    }
                    decimal num8 = Math.Round(num4 * iQuantity, 2);
                    decimal num9 = Math.Round(num5 * iQuantity, 2);
                    decimal num10 = num9 - num8;
                    decimal num11 = Math.Round(num6 * iQuantity, 2);
                    decimal num12 = Math.Round(num11 * (1m + num3 / 100m));
                    decimal num13 = num12 - num11;
                    num2++;
                    int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "PuArrival");
                    string detailId = "1" + $"{vouchId2:D9}";
                    if (pu.cBusType == "普通采购")
                    {
                        sql = string.Concat(" insert into pu_arrivalvouchs(Autoid,ID,cWhCode,cInvCode,iNum,iQuantity,iOriCost,iOriTaxCost,iOriMoney,  ioritaxprice,iorisum,icost,imoney,itaxprice,iSum,iTaxRate,  iPOsID,cunitid,fValidInQuan,fRealQuantity,fValidQuantity,iCorId,fretquantity,fInValidInQuan,bgsp,  cBatch,fValidNum,btaxcost,sotype,iinvexchrate,cordercode,RejectSource,iordertype,ivouchrowno,carrivalcode,bgift,  dPDate,dVDate,cExpirationdate,imassdate,cmassunit,cbsysbarcode,  cItemCode,cItemName,cItem_class,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )  select '", detailId, "','", vouchIdStr, "',", whCodeValue, ",'", cInvCode, "',", iNum, ",", iQuantity, ",", num4, ",", num5, ",", num8, ",  ", num10, ",", num9, ",", num6, ",", num11, ",", num13, ",", num12, ",", num3, ",  ID,'", dataTable9.Rows[0]["cunitid"], "',NULL,NULL,NULL,NULL,NULL,NULL,", num7, ", ", batchValue, ", NULL,btaxcost,sotype,0,'", dataTable9.Rows[0]["cCode"], "',0,0,'", num2, "',NULL,0, ", datePValue, ",", dateVValue, ",", expDateValue, ",", massDateValue, ",", massUnitValue, ",'||pudh|", voucherCode, "|", num2.ToString(), "',  cItemCode,cItemName,cItem_class,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37   from po_podetails (nolock) where ID = '", dataTable9.Rows[0]["ID"], "' ");
                        list.Add(sql);
                        sql = string.Concat(" update po_podetails set iArrQTY=isnull(iArrQTY,0)+", iQuantity, " ,iArrMoney=isnull(iArrMoney,0)+", num8, " ,   iNatArrMoney=isnull(iNatArrMoney,0)+", num11, "  ,fPoArrQuantity=isnull(fPoArrQuantity,0)+", iQuantity, " ,   fPoArrNum=isnull(fPoArrNum,0)+", assQty, "   where ID = '", dataTable9.Rows[0]["ID"], "' ");
                        list.Add(sql);
                    }
                    if (pu.cBusType == "委外加工")
                    {
                        sql = string.Concat(" insert into pu_arrivalvouchs(Autoid,ID,cWhCode,cInvCode,iNum,iQuantity,iOriCost,iOriTaxCost,iOriMoney,  ioritaxprice,iorisum,icost,imoney,itaxprice,iSum,iTaxRate,  iPOsID,cunitid,fValidInQuan,fRealQuantity,fValidQuantity,iCorId,fretquantity,fInValidInQuan,bgsp,  cBatch,fValidNum,btaxcost,sotype,iinvexchrate,cordercode,RejectSource,iordertype,ivouchrowno,carrivalcode,bgift,  dPDate,dVDate,cExpirationdate,imassdate,cmassunit,cbsysbarcode,  cItemCode,cItemName,cItem_class,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )  select '", detailId, "','", vouchIdStr, "',", whCodeValue, ",'", cInvCode, "',", iNum, ",", iQuantity, ",", num4, ",", num5, ",", num8, ",  ", num10, ",", num9, ",", num6, ",", num11, ",", num13, ",", num12, ",", num3, ",  MODetailsID, cunitid,NULL,NULL,NULL,NULL,NULL,NULL,", num7, ", ", batchValue, ", NULL,btaxcost,sotype,0,'", dataTable10.Rows[0]["cCode"], "',0,0,'", num2, "',NULL,0, ", datePValue, ",", dateVValue, ",", expDateValue, ",", massDateValue, ",", massUnitValue, ",'||omdh|", voucherCode, "|", num2.ToString(), "',  cItemCode,cItemName,cItem_class,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37   from po_podetails (nolock) where MODetailsID = '", dataTable10.Rows[0]["MODetailsID"], "' ");
                        list.Add(sql);
                        sql = string.Concat(" update OM_MODetails set iArrQTY=isnull(iArrQTY,0)+", iQuantity, ", iArrNum=isnull(iArrNum,0)+", assQty, ",  iArrMoney=isnull(iArrMoney,0)+", num8, " ,iNatArrMoney=isnull(iNatArrMoney,0)+", num11, "    where MODetailsID = '", dataTable10.Rows[0]["MODetailsID"], "' ");
                        list.Add(sql);
                    }
                    retItems.RowNo = 0;
                    retItems.SRMID = items[j].SRMID;
                    retItems.U8ID = detailId;
                    retItems.U8RowNo = num2;
                    list2.Add(retItems);
                }
                int num14 = U8SqlDBHelper.ExecuteSqlTran(list);
                if (num14 > 0)
                {
                    string jsonResult = JsonConvert.SerializeObject((object)list2);
                    return "{\"Code\":\"200\",\"Msg\":\"U8到货单[" + voucherCode + "]创建成功！\",\"U8Code\":\"" + voucherCode + "\",\"Items\": " + jsonResult + "}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U8到货单创建失败！\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        public static string PostPuRej(PuArr pu)
        {
            List<string> list = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(pu.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"到货单号[cCode]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                sql = " select * from pu_arrivalvouch (nolock) where cCode = '" + pu.cCode + "' and iBillType = 0 ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"到货单号[" + pu.cCode + "]不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (string.IsNullOrEmpty(pu.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (string.IsNullOrEmpty(pu.cHandler))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cHandler]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (!string.IsNullOrEmpty(pu.cDepCode))
                {
                    sql = " select * from department where cDepCode='" + pu.cDepCode + "' and bDepEnd = 1 ";
                    DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable2.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + pu.cDepCode + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }
                List<PuArrs> items = pu.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].Autoid) && string.IsNullOrEmpty(items[i].RowNo))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"到货单ID[Autoid]或到货单行号[RowNo]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    if (!string.IsNullOrEmpty(items[i].Autoid))
                    {
                        sql = " select 1 from pu_arrivalvouchs a left join pu_arrivalvouch b on a.ID=b.ID where a.autoid = '" + items[i].Autoid + "' and b.cCode = '" + pu.cCode + "' ";
                        DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable3.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"到货单ID[" + items[i].Autoid + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                        }
                    }
                    if (!string.IsNullOrEmpty(items[i].RowNo))
                    {
                        sql = " select 1 from pu_arrivalvouchs a left join pu_arrivalvouch b on a.ID=b.ID where ivouchrowno = '" + items[i].RowNo + "' and b.cCode = '" + pu.cCode + "' ";
                        DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable4.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"到货单行号[" + items[i].RowNo + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                        }
                    }
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                    }
                    sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                    DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable5.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    if (!string.IsNullOrEmpty(items[i].cWhCode))
                    {
                        sql = " select 1 from warehouse  where cwhcode = '" + items[i].cWhCode + "' ";
                        DataTable dataTable6 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable6.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + items[i].cWhCode + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                        }
                    }
                    string bInvBatch = dataTable5.Rows[0]["bInvBatch"].ToString();
                    string bInvQuality = dataTable5.Rows[0]["bInvQuality"].ToString();
                    if (bInvBatch == "True" && string.IsNullOrEmpty(items[i].cBatch))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cBatch + "]已启用批次管理，需要传递批次[cbatch]！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    try
                    {
                        Convert.ToDecimal(items[i].iQuantity);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"拒收数量[" + items[i].iQuantity + "]无效！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }
                string seedYearMonth = DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0');
                int num = 0;
                sql = " select cNumber from VoucherHistory where CardNumber ='26' and cSeed = '" + seedYearMonth + "' ";
                DataTable dataTable7 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable7.Rows.Count > 0)
                {
                    num = Convert.ToInt32(dataTable7.Rows[0]["cNumber"].ToString());
                }
                else
                {
                    U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('26',null,'单据日期','月','" + seedYearMonth + "',0,0) ");
                }
                num++;
                U8SqlDBHelper.ExecuteSql("update VoucherHistory set cNumber=cNumber+1 where CardNumber ='26' and cSeed = '" + seedYearMonth + "' ");
                string voucherCode = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + num.ToString().PadLeft(4, '0');
                int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "PuArrival");
                string vouchIdStr = "1" + $"{vouchId:D9}";
                string depCodeValue = "cDepCode";
                if (!string.IsNullOrEmpty(pu.cDepCode))
                {
                    depCodeValue = "'" + pu.cDepCode + "'";
                }
                sql = string.Concat(" insert into pu_arrivalvouch (  iVTid,ID,cCode,cptcode,ddate,cvencode,cdepcode,cpersoncode,cexch_name,iexchrate,  itaxrate,cmemo,cBusType,cMaker,bnegative,idiscounttaxtype,ibilltype,cmaketime,  cAuditDate,caudittime,cverifier,iverifystateex,iswfcontrolled,iflowid,iprintcount,csysbarcode,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select '8169','", vouchIdStr, "','", voucherCode, "',cptcode,'", pu.dDate, "',cvencode,", depCodeValue, ",cpersoncode,cexch_name,iexchrate,  itaxrate,'", pu.cMemo, "',cBusType,'", pu.cMaker, "',1,0,2,getdate(),   '',getdate(),'", pu.cHandler, "',2,0,iflowid,0, '||pudh|", voucherCode, "' ,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16   from pu_arrivalvouch where ccode = '", pu.cCode, "' ");
                list.Add(sql);
                List<RetItems> list2 = new List<RetItems>();
                int num2 = 0;
                for (int j = 0; j < items.Count; j++)
                {
                    RetItems retItems = new RetItems();
                    DataTable dataTable8 = new DataTable();
                    if (!string.IsNullOrEmpty(items[j].RowNo))
                    {
                        sql = " select a.* from pu_arrivalvouchs a left join pu_arrivalvouch b on a.ID=b.ID where ivouchrowno = '" + items[j].RowNo + "' and b.cCode = '" + pu.cCode + "' ";
                        dataTable8 = U8SqlDBHelper.GetDataTable(sql);
                    }
                    if (!string.IsNullOrEmpty(items[j].Autoid))
                    {
                        sql = " select a.* from pu_arrivalvouchs a left join pu_arrivalvouch b on a.ID=b.ID where a.autoid = '" + items[j].Autoid + "' and b.cCode = '" + pu.cCode + "' ";
                        dataTable8 = U8SqlDBHelper.GetDataTable(sql);
                    }
                    string cInvCode = items[j].cInvCode;
                    decimal iQuantity = items[j].iQuantity;
                    string iNum = "";
                    string iRate;
                    string AssUnit;
                    decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                    string batchValue = "null";
                    if (BasicDAL.IsBatch(cInvCode, zt) && !string.IsNullOrEmpty(items[j].cBatch))
                    {
                        batchValue = "'" + items[j].cBatch + "'";
                    }
                    string datePValue = "null";
                    string dateVValue = "null";
                    string expDateValue = "null";
                    string massDateValue = "null";
                    string massUnitValue = "null";
                    int num3 = 0;
                    if (BasicDAL.IsPropertyCheck(cInvCode, zt))
                    {
                        num3 = 1;
                    }
                    string whCodeValue = "null";
                    if (!string.IsNullOrEmpty(items[j].cWhCode))
                    {
                        whCodeValue = "'" + items[j].cWhCode + "'";
                    }
                    decimal num4 = BasicDAL.ToDec(dataTable8.Rows[0]["iTaxRate"].ToString());
                    decimal num5 = BasicDAL.ToDec(dataTable8.Rows[0]["iOriCost"].ToString());
                    decimal num6 = BasicDAL.ToDec(dataTable8.Rows[0]["iOriTaxCost"].ToString());
                    decimal num7 = Math.Round(num5 * -iQuantity, 2);
                    decimal num8 = Math.Round(num6 * -iQuantity, 2);
                    decimal num9 = num8 - num7;
                    decimal num10 = BasicDAL.ToDec(dataTable8.Rows[0]["iCost"].ToString());
                    decimal num11 = Math.Round(num10 * -iQuantity, 2);
                    decimal num12 = Math.Round(num11 * (1m + num4 / 100m));
                    decimal num13 = num12 - num11;
                    num2++;
                    int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "PuArrival");
                    string detailId = "1" + $"{vouchId2:D9}";
                    sql = string.Concat(" insert into pu_arrivalvouchs(Autoid,ID,cWhCode,cInvCode,iNum,iQuantity,iOriCost,iOriTaxCost,iOriMoney,  ioritaxprice,iorisum,icost,imoney,itaxprice,iSum,iTaxRate,  iPOsID,cunitid,fValidInQuan,fRealQuantity,fValidQuantity,iCorId,fretquantity,fInValidInQuan,bgsp,  cBatch,fValidNum,btaxcost,sotype,iinvexchrate,cordercode,RejectSource,iordertype,ivouchrowno,carrivalcode,bgift,  dPDate,dVDate,cExpirationdate,imassdate,cmassunit,cbsysbarcode,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )  select '", detailId, "','", vouchIdStr, "',", whCodeValue, ",'", cInvCode, "',-", iNum, ",-", iQuantity, ",", num5, ",", num6, ",", num7, ",  ", num9, ",", num8, ",", num10, ",", num11, ",", num13, ",", num12, ",", num4, ",  iPOsID,cunitid,NULL,NULL,NULL, Autoid ,NULL,NULL,", num3, ", ", batchValue, ", NULL,btaxcost,sotype,0, cordercode ,0,0,'", num2, "','", pu.cCode, "',0, ", datePValue, ",", dateVValue, ",", expDateValue, ",", massDateValue, ",", massUnitValue, ",'||pudh|", voucherCode, "|", num2.ToString(), "',  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37   from pu_arrivalvouchs (nolock) where Autoid = '", dataTable8.Rows[0]["autoid"], "' ");
                    list.Add(sql);
                    sql = string.Concat(" update pu_arrivalvouchs set fSumRefuseQuantity=ISNULL(fSumRefuseQuantity,0)+", iQuantity, ",fRefuseQuantity=ISNULL(fRefuseQuantity,0)+", iQuantity, "  where Autoid = '", dataTable8.Rows[0]["autoid"], "' ");
                    list.Add(sql);
                    retItems.Autoid = dataTable8.Rows[0]["autoid"].ToString();
                    retItems.RowNo = BasicDAL.ToInt(dataTable8.Rows[0]["ivouchrowno"].ToString());
                    retItems.U8ID = detailId;
                    retItems.U8RowNo = num2;
                    list2.Add(retItems);
                }
                int num14 = U8SqlDBHelper.ExecuteSqlTran(list);
                if (num14 > 0)
                {
                    string jsonResult = JsonConvert.SerializeObject((object)list2);
                    return "{\"Code\":\"200\",\"Msg\":\"U8拒收单[" + voucherCode + "]创建成功！\",\"U8Code\":\"" + voucherCode + "\",\"Items\": " + jsonResult + "}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U8拒收单创建失败！\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        public static string PostPuRet(PuArr pu)
        {
            List<string> list = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(pu.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"到货单号[cCode]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                sql = " select * from pu_arrivalvouch (nolock) where cCode = '" + pu.cCode + "' and iBillType = 0 ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"到货单号[" + pu.cCode + "]不存在！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (string.IsNullOrEmpty(pu.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (string.IsNullOrEmpty(pu.cHandler))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cHandler]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                }
                if (!string.IsNullOrEmpty(pu.cDepCode))
                {
                    sql = " select * from department where cDepCode='" + pu.cDepCode + "' and bDepEnd = 1 ";
                    DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable2.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + pu.cDepCode + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }
                List<PuArrs> items = pu.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].Autoid) && string.IsNullOrEmpty(items[i].RowNo))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"到货单ID[Autoid]或到货单行号[RowNo]未传递！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    if (!string.IsNullOrEmpty(items[i].Autoid))
                    {
                        sql = " select 1 from pu_arrivalvouchs a left join pu_arrivalvouch b on a.ID=b.ID where a.autoid = '" + items[i].Autoid + "' and b.cCode = '" + pu.cCode + "' ";
                        DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable3.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"到货单ID[" + items[i].Autoid + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                        }
                    }
                    if (!string.IsNullOrEmpty(items[i].RowNo))
                    {
                        sql = " select 1 from pu_arrivalvouchs a left join pu_arrivalvouch b on a.ID=b.ID where ivouchrowno = '" + items[i].RowNo + "' and b.cCode = '" + pu.cCode + "' ";
                        DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable4.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"到货单行号[" + items[i].RowNo + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                        }
                    }
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                    }
                    sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                    DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable5.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                    if (!string.IsNullOrEmpty(items[i].cWhCode))
                    {
                        sql = " select 1 from warehouse  where cwhcode = '" + items[i].cWhCode + "' ";
                        DataTable dataTable6 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable6.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + items[i].cWhCode + "]无数据！\",\"U8Code\":\"\",\"Items\":\"\"}";
                        }
                    }
                    try
                    {
                        Convert.ToDecimal(items[i].iQuantity);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"退货数量[" + items[i].iQuantity + "]无效！\",\"U8Code\":\"\",\"Items\":\"\"}";
                    }
                }
                string seedYearMonth = DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0');
                int num = 0;
                sql = " select cNumber from VoucherHistory where CardNumber ='26' and cSeed = '" + seedYearMonth + "' ";
                DataTable dataTable7 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable7.Rows.Count > 0)
                {
                    num = Convert.ToInt32(dataTable7.Rows[0]["cNumber"].ToString());
                }
                else
                {
                    U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('26',null,'单据日期','月','" + seedYearMonth + "',0,0) ");
                }
                num++;
                U8SqlDBHelper.ExecuteSql("update VoucherHistory set cNumber=cNumber+1 where CardNumber ='26' and cSeed = '" + seedYearMonth + "' ");
                string voucherCode = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + num.ToString().PadLeft(4, '0');
                int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "PuArrival");
                string vouchIdStr = "1" + $"{vouchId:D9}";
                string depCodeValue = "cDepCode";
                if (!string.IsNullOrEmpty(pu.cDepCode))
                {
                    depCodeValue = "'" + pu.cDepCode + "'";
                }
                sql = string.Concat(" insert into pu_arrivalvouch (  iVTid,ID,cCode,cptcode,ddate,cvencode,cdepcode,cpersoncode,cexch_name,iexchrate,  itaxrate,cmemo,cBusType,cMaker,bnegative,idiscounttaxtype,ibilltype,cmaketime,  cAuditDate,caudittime,cverifier,iverifystateex,iswfcontrolled,iflowid,iprintcount,csysbarcode,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select '8169','", vouchIdStr, "','", voucherCode, "',cptcode,'", pu.dDate, "',cvencode,", depCodeValue, ",cpersoncode,cexch_name,iexchrate,  itaxrate,'", pu.cMemo, "',cBusType,'", pu.cMaker, "',1,0, 1,getdate(),   '',getdate(),'", pu.cHandler, "',2,0,iflowid,0, '||puth|", voucherCode, "' ,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16   from pu_arrivalvouch where ccode = '", pu.cCode, "' ");
                list.Add(sql);
                List<RetItems> list2 = new List<RetItems>();
                int num2 = 0;
                for (int j = 0; j < items.Count; j++)
                {
                    RetItems retItems = new RetItems();
                    DataTable dataTable8 = new DataTable();
                    if (!string.IsNullOrEmpty(items[j].RowNo))
                    {
                        sql = " select a.* from pu_arrivalvouchs a left join pu_arrivalvouch b on a.ID=b.ID where ivouchrowno = '" + items[j].RowNo + "' and b.cCode = '" + pu.cCode + "' ";
                        dataTable8 = U8SqlDBHelper.GetDataTable(sql);
                    }
                    if (!string.IsNullOrEmpty(items[j].Autoid))
                    {
                        sql = " select a.* from pu_arrivalvouchs a left join pu_arrivalvouch b on a.ID=b.ID where a.autoid = '" + items[j].Autoid + "' and b.cCode = '" + pu.cCode + "' ";
                        dataTable8 = U8SqlDBHelper.GetDataTable(sql);
                    }
                    string cInvCode = items[j].cInvCode;
                    decimal iQuantity = items[j].iQuantity;
                    string iNum = "";
                    string iRate;
                    string AssUnit;
                    decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                    string batchValue = "null";
                    if (BasicDAL.IsBatch(cInvCode, zt) && !string.IsNullOrEmpty(items[j].cBatch))
                    {
                        batchValue = "'" + items[j].cBatch + "'";
                    }
                    string datePValue = "null";
                    string dateVValue = "null";
                    string expDateValue = "null";
                    int num3 = 0;
                    if (BasicDAL.IsPropertyCheck(cInvCode, zt))
                    {
                        num3 = 1;
                    }
                    string massDateValue = "null";
                    string massUnitValue = "null";
                    string whCodeValue = "null";
                    if (!string.IsNullOrEmpty(items[j].cWhCode))
                    {
                        whCodeValue = "'" + items[j].cWhCode + "'";
                    }
                    decimal num4 = BasicDAL.ToDec(dataTable8.Rows[0]["iTaxRate"].ToString());
                    decimal num5 = BasicDAL.ToDec(dataTable8.Rows[0]["iOriCost"].ToString());
                    decimal num6 = BasicDAL.ToDec(dataTable8.Rows[0]["iOriTaxCost"].ToString());
                    decimal num7 = Math.Round(num5 * -iQuantity, 2);
                    decimal num8 = Math.Round(num6 * -iQuantity, 2);
                    decimal num9 = num8 - num7;
                    decimal num10 = BasicDAL.ToDec(dataTable8.Rows[0]["iCost"].ToString());
                    decimal num11 = Math.Round(num10 * -iQuantity, 2);
                    decimal num12 = Math.Round(num11 * (1m + num4 / 100m));
                    decimal num13 = num12 - num11;
                    num2++;
                    int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "PuArrival");
                    string detailId = "1" + $"{vouchId2:D9}";
                    sql = string.Concat(" insert into pu_arrivalvouchs(Autoid,ID,cWhCode,cInvCode,iNum,iQuantity,iOriCost,iOriTaxCost,iOriMoney,  ioritaxprice,iorisum,icost,imoney,itaxprice,iSum,iTaxRate,  iPOsID,cunitid,fValidInQuan,fRealQuantity,fValidQuantity,iCorId,fretquantity,fInValidInQuan,bgsp,  cBatch,fValidNum,btaxcost,sotype,iinvexchrate,cordercode,RejectSource,iordertype,ivouchrowno,carrivalcode,bgift,  dPDate,dVDate,cExpirationdate,imassdate,cmassunit,cbsysbarcode,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )  select '", detailId, "','", vouchIdStr, "',", whCodeValue, ",'", cInvCode, "',-", iNum, ",-", iQuantity, ",", num5, ",", num6, ",", num7, ",  ", num9, ",", num8, ",", num10, ",", num11, ",", num13, ",", num12, ",", num4, ",  iPOsID,cunitid,NULL,NULL,NULL, Autoid ,NULL,NULL,", num3, ", ", batchValue, ", NULL,btaxcost,sotype,0, cordercode ,0,0,'", num2, "','", pu.cCode, "',0, ", datePValue, ",", dateVValue, ",", expDateValue, ",", massDateValue, ",", massUnitValue, ",'||puth|", voucherCode, "|", num2.ToString(), "',  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37   from pu_arrivalvouchs (nolock) where Autoid = '", dataTable8.Rows[0]["autoid"], "' ");
                    list.Add(sql);
                    sql = string.Concat(" update pu_arrivalvouchs set fSumRefuseQuantity=ISNULL(fSumRefuseQuantity,0)+", iQuantity, ",fRefuseQuantity=ISNULL(fRefuseQuantity,0)+", iQuantity, "  where Autoid = '", dataTable8.Rows[0]["autoid"], "' ");
                    list.Add(sql);
                    retItems.Autoid = dataTable8.Rows[0]["autoid"].ToString();
                    retItems.RowNo = BasicDAL.ToInt(dataTable8.Rows[0]["ivouchrowno"].ToString());
                    retItems.U8ID = detailId;
                    retItems.U8RowNo = num2;
                    list2.Add(retItems);
                }
                int num14 = U8SqlDBHelper.ExecuteSqlTran(list);
                if (num14 > 0)
                {
                    string jsonResult = JsonConvert.SerializeObject((object)list2);
                    return "{\"Code\":\"200\",\"Msg\":\"U8退货单[" + voucherCode + "]创建成功！\",\"U8Code\":\"" + voucherCode + "\",\"Items\": " + jsonResult + "}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"U8退货单创建失败！\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string detailId = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + detailId + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }
    }
}
