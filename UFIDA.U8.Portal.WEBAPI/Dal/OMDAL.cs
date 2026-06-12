using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{

public class OMDAL
{
	private static string zt = ConfigurationManager.AppSettings["zt"].ToString();

	private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();

	private static int morow = 0;

	private static List<string> xsqlList = new List<string>();

        public static string OMorder(OMOrder om)
        {
            List<string> list = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(om.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
                }
                if (string.IsNullOrEmpty(om.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
                }
                sql = " select 1 from OM_MOMain where cCode = '" + om.cCode + "' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + om.cCode + "]已存在！\"}";
                }
                if (string.IsNullOrEmpty(om.dDate))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
                }
                DateTime dateTime;
                try
                {
                    dateTime = Convert.ToDateTime(om.dDate);
                }
                catch
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + om.dDate + "]格式错误！\",\"U8Code\":\"\"}";
                }
                string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
                sql = " select bflag_OM from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
                string bflagOM = U8SqlDBHelper.GetString(sql);
                if (bflagOM == "True")
                {
                    return "{\"Code\":\"400\",\"Msg\":\" 日期[" + om.dDate + "]委外已关账！\"}";
                }
                if (string.IsNullOrEmpty(om.cVenCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"供应商编码[cVenCode]未传递！\"}";
                }
                sql = " select * from Vendor where isnull(dEndDate,'2099-01-01')>getdate() and cVenCode = '" + om.cVenCode + "' and bProxyForeign = 1 ";
                DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable2.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"供应商编码[" + om.cVenCode + "]无效！\"}";
                }
                if (string.IsNullOrEmpty(om.cDepCode))
                {
                    sql = " select 1 from Department where bDepEnd = '1' and cDepCode = '" + om.cDepCode + "' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + om.cDepCode + "]无数据或不是末级部门！\"}";
                    }
                }
                if (!string.IsNullOrEmpty(om.cPersonCode))
                {
                    sql = " select 1 from person where cPersonCode = '" + om.cPersonCode + "' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + om.cPersonCode + "]无数据或已停用！\"}";
                    }
                }
                if (string.IsNullOrEmpty(om.cexch_name))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"币种[cexch_name]未传递！\"}";
                }
                if (string.IsNullOrEmpty(om.nflat))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"汇率[nflat]未传递！\"}";
                }
                decimal num = BasicDAL.ToDec(om.nflat);
                List<OMDetail> items = om.Items;
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
                    sql = " select 1 from PU_AppVouchs a left join PU_AppVouch b on a.ID=b.ID  where b.ccode = '" + items[i].cpoid + "' and ivouchrowno = '" + items[i].vouchrowno + "' and isnull(cbcloser,'')='' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"请购单号[" + items[i].cpoid + "]行号[" + items[i].vouchrowno + "]无数据或已停用！\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                    }
                    sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and bProxyForeign=1 and isnull(dEDate,'2099-01-01')>getdate() ";
                    DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable3.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无效！\"}";
                    }
                    string partId = U8SqlDBHelper.GetString("select PartId from bas_part where InvCode ='" + items[i].cInvCode + "'");
                    sql = " select a.* from bom_bom a left join bom_parent b on a.BomId=b.BomId  where b.ParentId = '" + partId + "' and a.status = '3' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]未维护BOM！\"}";
                    }
                    if (items[i].iQuantity <= 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]不可为0！\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].iRowNo))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"行号[iRowNo]未传递！\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].dStartDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"计划下达日期[dStartDate]未传递！\",\"U8Code\":\"\"}";
                    }
                    try
                    {
                        DateTime dateTime2 = Convert.ToDateTime(items[i].dStartDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"计划下达日期[" + items[i].dStartDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].dArriveDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"计划到货日期[dArriveDate]未传递！\",\"U8Code\":\"\"}";
                    }
                    try
                    {
                        DateTime dateTime3 = Convert.ToDateTime(items[i].dArriveDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"计划到货日期[" + items[i].dArriveDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "OM_MO");
                string moId = "1" + $"{vouchId:D9}";
                string depCodeValue = "null";
                string personCodeValue = "null";
                if (!string.IsNullOrEmpty(om.cDepCode))
                {
                    depCodeValue = "'" + om.cDepCode + "'";
                }
                if (!string.IsNullOrEmpty(om.cPersonCode))
                {
                    personCodeValue = "'" + om.cPersonCode + "'";
                }
                string venAddress = dataTable2.Rows[0]["cVenIAddress"].ToString();
                string venPerson = dataTable2.Rows[0]["cVenPerson"].ToString();
                string venBank = dataTable2.Rows[0]["cVenBank"].ToString();
                string venAccount = dataTable2.Rows[0]["cVenAccount"].ToString();
                sql = " insert into OM_MOMain (MOID,cCode,dDate,cvencode,cDepcode,cPersonCode,  cArrivalPlace,cSCCode,cexch_name,nflat,iTaxRate,cmemo,cmaker,cverifier,  ivtid,cbustype,cState,iVerifyStateNew,IsWfControlled,dCreateTime,dverifydate,cptcode,  dverifytime,cvenperson,cVenBank,cVenAccount,csrccode,csysbarcode,iordertype,brework )  values('" + moId + "','" + om.cCode + "','" + dateTime.ToString("yyyy-MM-dd") + "','" + om.cVenCode + "'," + depCodeValue + "," + personCodeValue + ",  '" + venAddress + "',null,'" + om.cexch_name + "'," + num + "," + items[0].iPerTaxRate + ",'" + om.cMemo + "','" + om.cMaker + "', null,  '8157','委外加工',0  ,0  , 1,getdate(), null , '02',  null ,'" + venPerson + "','" + venBank + "','" + venAccount + "','" + items[0].cpoid + "','||ommo|" + om.cCode + "',0,0 ) ";
                list.Add(sql);
                for (int j = 0; j < items.Count; j++)
                {
                    decimal iPerTaxRate = items[j].iPerTaxRate;
                    decimal iUnitPrice = items[j].iUnitPrice;
                    decimal iTaxPrice = items[j].iTaxPrice;
                    decimal iMoney = items[j].iMoney;
                    decimal iTax = items[j].iTax;
                    decimal iSum = items[j].iSum;
                    decimal num2 = iUnitPrice * num;
                    decimal num3 = iUnitPrice * num;
                    decimal num4 = iTax * num;
                    decimal num5 = iSum * num;
                    DateTime sdate = Convert.ToDateTime(items[j].dStartDate);
                    DateTime dateTime4 = Convert.ToDateTime(items[j].dArriveDate);
                    sql = " select a.* from PU_AppVouchs a left join PU_AppVouch b on a.ID=b.ID where b.ccode = '" + items[j].cpoid + "' and ivouchrowno = '" + items[j].vouchrowno + "' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    string appVouchAutoid = dataTable.Rows[0]["autoid"].ToString();
                    string partId = U8SqlDBHelper.GetString("select PartId from bas_part where InvCode ='" + items[j].cInvCode + "'");
                    sql = " select a.*,b.ParentScrap from bom_bom a left join bom_parent b on a.BomId=b.BomId  where b.ParentId = '" + partId + "' and a.status = '3' ";
                    DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                    string bomId = dataTable4.Rows[0]["BomId"].ToString();
                    decimal parentScrap = BasicDAL.ToDec(dataTable4.Rows[0]["ParentScrap"].ToString());
                    int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "OM_MO");
                    string moDetailId = "1" + $"{vouchId2:D9}";
                    sql = " insert into OM_MODetails (MODetailsID,moid,cinvcode,iquantity,  iunitprice,imoney,itax,isum,inatunitprice,inatmoney,inattax,iNatSum,  dStartDate,dArriveDate,ipertaxrate,  cdefine23,bGsp,itaxprice,bTaxCost,cSource,SOType,bomid,fparentscrp,  ivtids,cupsocode,cupsoids,ivouchrowno,cbsysbarcode,BomType,imrpqty)  values('" + moDetailId + "','" + moId + "','" + items[j].cInvCode + "'," + items[j].iQuantity + ",  " + iUnitPrice + "," + iMoney + "," + iTax + "," + iSum + "," + num2 + "," + num3 + "," + num4 + "," + num5 + ",  '" + sdate.ToString("yyyy-MM-dd") + "','" + dateTime4.ToString("yyyy-MM-dd") + "'," + iPerTaxRate + ",  '" + items[j].cdefine23 + "',0, " + iTaxPrice + ",1, 'app',0,'" + bomId + "'," + parentScrap + ",  '8159','" + items[j].cpoid + "','" + appVouchAutoid + "'," + items[j].iRowNo + ",'||ommo|" + om.cCode + "|" + items[j].iRowNo + "',1," + items[j].iQuantity + " ) ";
                    list.Add(sql);
                    sql = " update PU_AppVouchs set iReceivedQTY=isnull(iReceivedQTY,0) + " + items[j].iQuantity + " where autoid='" + appVouchAutoid + "' ";
                    list.Add(sql);
                    sql = " select a.OpComponentId,a.ComponentId,b.InvCode,a.BaseQtyN,a.BaseQtyD,a.CompScrap,op.Whcode,op.WIPType   from bom_opcomponent a (nolock) left join Bas_part b (nolock) on a.ComponentId=b.PartId  left join bom_opcomponentopt op (nolock) on op.OptionsId = a.OptionsId  where BomId = " + bomId + "  ";
                    DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
                    morow = 0;
                    for (int k = 0; k < dataTable5.Rows.Count; k++)
                    {
                        string componentId = dataTable5.Rows[k]["ComponentId"].ToString();
                        decimal baseQtyN = Convert.ToDecimal(dataTable5.Rows[k]["BaseQtyN"]);
                        decimal baseQtyD = Convert.ToDecimal(dataTable5.Rows[k]["BaseQtyD"]);
                        decimal compScrap = BasicDAL.ToDec(dataTable5.Rows[k]["CompScrap"].ToString());
                        decimal scrapFactor = Math.Round((100m + compScrap) / 100m, 6);
                        decimal qtyWithScrap = Math.Round(items[j].iQuantity * baseQtyN / baseQtyD * scrapFactor, 4);
                        int wipType = Convert.ToInt32(dataTable5.Rows[k]["WIPType"]);
                        if (wipType == 4)
                        {
                            xsqlList.Clear();
                            GetXNJsql(componentId, qtyWithScrap, moDetailId, moId, sdate, om.cCode, items[j].iRowNo);
                            for (int l = 0; l < xsqlList.Count; l++)
                            {
                                list.Add(xsqlList[l]);
                            }
                            continue;
                        }
                        BasicDAL.GetVouchId("F", cAcc_Id, "OM_Materials");
                        int vouchId3 = BasicDAL.GetVouchId("C", cAcc_Id, "OM_Materials");
                        string materialId = "1" + $"{vouchId3:D9}";
                        morow++;
                        string whCodeValue = "null";
                        if (!string.IsNullOrEmpty(dataTable5.Rows[k]["Whcode"].ToString()))
                        {
                            whCodeValue = string.Concat("'", dataTable5.Rows[k]["Whcode"], "'");
                        }
                        string invCode = dataTable5.Rows[k]["InvCode"].ToString();
                        string opComponentId = dataTable5.Rows[k]["OpComponentId"].ToString();
                        sql = " insert into OM_MOMaterials(MOMaterialsID,MoDetailsID,MOID,cinvcode,iquantity,drequireddate,  fbaseqtyn,fBaseQtyD,fCompScrp,bFVQty,iWIPtype,cWhCode,  iUnitQuantity,OpComponentId,subflag,csendtype,csubsysbarcode,iProductType )  values (" + materialId + ",'" + moDetailId + "','" + moId + "','" + invCode + "'," + qtyWithScrap + ",'" + sdate.ToString("yyyy-MM-dd") + "',  " + baseQtyN + "," + baseQtyD + "," + compScrap + ",0,'" + wipType + "'," + whCodeValue + ",  1,'" + opComponentId + "',0,0,'||ommo|" + om.cCode + "|" + items[j].iRowNo + "|" + morow + "',0 ) ";
                        list.Add(sql);
                    }
                }
                int num13 = U8SqlDBHelper.ExecuteSqlTran(list);
                if (num13 > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"委外订单[" + om.cCode + "]创建成功！\",\"U8Code\":\"" + om.cCode + "\"}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"委外订单[" + om.cCode + "]创建失败！\",\"U8Code\":\"" + om.cCode + "\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        private static void GetXNJsql(string alpartid, decimal qty, string AUTOID, string ID, DateTime Sdate, string cCode, string row)
        {
            string sql = "";
            try
            {
                sql = " select max(Version) ver from bom_bom  where bomid in (select bomid from bom_parent where ParentId = '" + alpartid + "')  and isnull(CloseUser,'')='' and getdate() < VersionEndDate  ";
                string bomVersion = U8SqlDBHelper.GetString(sql);
                sql = " select BomId from bom_bom where bomid in (select bomid from bom_parent where ParentId = '" + alpartid + "')  and Version = '" + bomVersion + "' ";
                string bomId = U8SqlDBHelper.GetString(sql);
                sql = " select a.OpComponentId,a.ComponentId,b.InvCode,a.BaseQtyN,a.BaseQtyD,a.CompScrap,op.Whcode,op.WIPType   from bom_opcomponent a (nolock) left join Bas_part b (nolock) on a.ComponentId=b.PartId  left join bom_opcomponentopt op (nolock) on op.OptionsId = a.OptionsId  where BomId = " + bomId + "  ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    string componentId = dataTable.Rows[i]["ComponentId"].ToString();
                    decimal baseQtyN = Convert.ToDecimal(dataTable.Rows[i]["BaseQtyN"]);
                    decimal baseQtyD = Convert.ToDecimal(dataTable.Rows[i]["BaseQtyD"]);
                    decimal compScrap = BasicDAL.ToDec(dataTable.Rows[i]["CompScrap"].ToString());
                    decimal scrapFactor = Math.Round((100m + compScrap) / 100m, 6);
                    decimal qtyWithScrap = Math.Round(qty * baseQtyN / baseQtyD * scrapFactor, 4);
                    int wipType = Convert.ToInt32(dataTable.Rows[i]["WIPType"]);
                    if (wipType == 4)
                    {
                        GetXNJsql(componentId, qtyWithScrap, AUTOID, ID, Sdate, cCode, row);
                        continue;
                    }
                    BasicDAL.GetVouchId("F", cAcc_Id, "OM_Materials");
                    int vouchId = BasicDAL.GetVouchId("C", cAcc_Id, "OM_Materials");
                    string materialId = "1" + $"{vouchId:D9}";
                    morow++;
                    string whCodeValue = "null";
                    if (!string.IsNullOrEmpty(dataTable.Rows[i]["Whcode"].ToString()))
                    {
                        whCodeValue = string.Concat("'", dataTable.Rows[i]["Whcode"], "'");
                    }
                    string invCode = dataTable.Rows[i]["InvCode"].ToString();
                    string opComponentId = dataTable.Rows[i]["OpComponentId"].ToString();
                    sql = " insert into OM_MOMaterials(MOMaterialsID,MoDetailsID,MOID,cinvcode,iquantity,drequireddate,  fbaseqtyn,fBaseQtyD,fCompScrp,bFVQty,iWIPtype,cWhCode,  iUnitQuantity,OpComponentId,subflag,csendtype,csubsysbarcode,iProductType )  values (" + materialId + ",'" + AUTOID + "','" + ID + "','" + invCode + "'," + qtyWithScrap + ",'" + Sdate.ToString("yyyy-MM-dd") + "',  " + baseQtyN + "," + baseQtyD + "," + compScrap + ",0,'" + wipType + "'," + whCodeValue + ",  1,'" + opComponentId + "',0,0,'||ommo|" + cCode + "|" + row + "|" + morow + "',0 ) ";
                    xsqlList.Add(sql);
                }
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
            }
        }
    }
}
