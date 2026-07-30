﻿﻿﻿using Newtonsoft.Json;
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
public class RDSDAL
{
    private static string zt = ConfigurationManager.AppSettings["zt"].ToString();

    private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();
    public static string Trans(Trans tr)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(tr.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(tr.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cVerifier]未传递！\"}";
            }
            if (!string.IsNullOrEmpty(tr.cCode))
            {
                sql = " select 1 from TransVouch where cTVCode = '" + tr.cCode + "' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + tr.cCode + "]已存在！\"}";
                }
            }
            if (tr.cSource == "调拨申请")
            {
                if (!string.IsNullOrEmpty(tr.RequestCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"调拨申请单号[RequestCode]未传递！\"}";
                }
                sql = " select 1 from ST_AppTransVouch (nolock) where cTVCode = '" + tr.RequestCode + "' and isnull(cVerifyPerson,'')<>'' and isnull(capprovar,'')<>'' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"调拨申请单号[" + tr.RequestCode + "]无效！\"}";
                }
            }
            if (string.IsNullOrEmpty(tr.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(tr.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + tr.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string periodStr = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + periodStr + "' ";
            string bFlag = U8SqlDBHelper.GetString(sql);
            if (bFlag == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + tr.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(tr.cIWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"调入仓库[cIWhCode]未传递！\",\"U8Code\":\"\",\"08Code\":\"\",\"09Code\":\"\"}";
            }
            if (string.IsNullOrEmpty(tr.cOWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"调出仓库[cOWhCode]未传递！\",\"U8Code\":\"\",\"08Code\":\"\",\"09Code\":\"\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + tr.cIWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            DataTable warehouseTable = U8SqlDBHelper.GetDataTable(sql);
            if (warehouseTable.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + tr.cIWhCode + "]无数据或已停用！\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + tr.cOWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            warehouseTable = U8SqlDBHelper.GetDataTable(sql);
            if (warehouseTable.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + tr.cOWhCode + "]无数据或已停用！\"}";
            }
            if (!string.IsNullOrEmpty(tr.cODepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + tr.cODepCode + "' ";
                DataTable deptTable = U8SqlDBHelper.GetDataTable(sql);
                if (deptTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + tr.cODepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(tr.cIDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + tr.cIDepCode + "' ";
                DataTable deptTable = U8SqlDBHelper.GetDataTable(sql);
                if (deptTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + tr.cIDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(tr.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + tr.cPersonCode + "' ";
                DataTable personTable = U8SqlDBHelper.GetDataTable(sql);
                if (personTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + tr.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<TransItems> items = tr.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (tr.cSource == "调拨申请")
                {
                    if (string.IsNullOrEmpty(items[i].iTRIds))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"调拨申请单明细ID[cInvCode]未传递！\"}";
                    }
                    sql = " select 1 from ST_AppTransVouchs (nolock) where autoID = '" + items[i].iTRIds + "'  ";
                    DataTable appTrTable = U8SqlDBHelper.GetDataTable(sql);
                    if (appTrTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"调拨申请单明细ID[" + items[i].iTRIds + "]无效！\"}";
                    }
                }
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable invTable = U8SqlDBHelper.GetDataTable(sql);
                if (invTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime madeDate = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
                if (items[i].iQuantity <= 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"调拨数量[iQuantity]必须大于0！\",\"U8Code\":\"\"}";
                }
                sql = " select iQuantity from currentstock (nolock) where cinvcode = '" + items[i].cInvCode + "' and cwhcode = '" + tr.cOWhCode + "' ";
                if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                {
                    sql = sql + " and cbatch = '" + items[i].cBatch + "' ";
                }
                decimal stockQty = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                if (stockQty < items[i].iQuantity)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"仓库[" + tr.cOWhCode + "]存货[" + items[i].cInvCode + "]批号[" + items[i].cBatch + "]现存量不足[" + items[i].iQuantity + "]！\",\"U8Code\":\"\"}";
                }
                if (BasicDAL.bWhPos(tr.cIWhCode))
                {
                    if (string.IsNullOrEmpty(items[i].ciPosCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"调入货位编码[ciPosCode]未传递！\",\"U8Code\":\"\"}";
                    }
                    sql = " select 1 from Position where cPosCode='" + items[i].ciPosCode + "' and cWhCode='" + tr.cIWhCode + "' and bPosEnd=1 ";
                    DataTable posTable = U8SqlDBHelper.GetDataTable(sql);
                    if (posTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"调出货位编码[" + items[i].ciPosCode + "]无数据！\"}";
                    }
                }
                if (BasicDAL.bWhPos(tr.cOWhCode))
                {
                    if (string.IsNullOrEmpty(items[i].coPosCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"调入货位编码[ciPosCode]未传递！\",\"U8Code\":\"\"}";
                    }
                    sql = " select 1 from Position where cPosCode='" + items[i].coPosCode + "' and cWhCode='" + tr.cOWhCode + "' and bPosEnd=1 ";
                    DataTable posTable = U8SqlDBHelper.GetDataTable(sql);
                    if (posTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"调出货位编码[" + items[i].coPosCode + "]无数据！\"}";
                    }
                    sql = " select iQuantity from InvPositionSum where cWhCode='" + tr.cOWhCode + "' and cPosCode='" + items[i].coPosCode + "' and cInvCode='" + items[i].cInvCode + "' ";
                    if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                    {
                        sql = sql + " and cBatch='" + items[i].cBatch + "' ";
                    }
                    decimal posStockQty = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                    if (items[i].iQuantity > posStockQty)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + tr.cOWhCode + "]货位[" + items[i].coPosCode + "]批号[" + items[i].cBatch + "]货位现存量不足！\"}";
                    }
                }
            }

            int transVouchId = BasicDAL.GetVouchId("F", cAcc_Id, "tr");
            string transVouchIdStr = "1" + $"{transVouchId:D9}";

            int transVouchNum = 0;
            sql = " select cNumber from VoucherHistory where CardNumber ='0304' ";
            DataTable vhTable = U8SqlDBHelper.GetDataTable(sql);
            if (vhTable.Rows.Count > 0)
            {
                transVouchNum = Convert.ToInt32(vhTable.Rows[0]["cNumber"].ToString());
            }
            else
            {
                U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('0304',null,null,null,null,0,0) ");
            }
            transVouchNum++;
            U8SqlDBHelper.ExecuteSql("update VoucherHistory set cNumber=cNumber+1 where CardNumber ='0304' ");
            string transVouchCode = transVouchNum.ToString().PadLeft(10, '0');
            if (!string.IsNullOrEmpty(tr.cCode))
            {
                transVouchCode = tr.cCode;
            }

            int rdOutId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string rdOutIdStr = "1" + $"{rdOutId:D9}";

            int rdOutNum = 0;
            sql = " select cNumber from VoucherHistory where CardNumber ='0302' ";
            vhTable = U8SqlDBHelper.GetDataTable(sql);
            if (vhTable.Rows.Count > 0)
            {
                rdOutNum = Convert.ToInt32(vhTable.Rows[0]["cNumber"].ToString());
            }
            else
            {
                U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('0302',null,null,null,null,0,0) ");
            }
            rdOutNum++;
            U8SqlDBHelper.ExecuteSql("update VoucherHistory set cNumber=cNumber+1 where CardNumber ='0302' ");
            string rdOutCode = rdOutNum.ToString().PadLeft(10, '0');

            int rdInId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string rdInIdStr = "1" + $"{rdInId:D9}";

            int rdInNum = 0;
            sql = " select cNumber from VoucherHistory where CardNumber ='0301' ";
            vhTable = U8SqlDBHelper.GetDataTable(sql);
            if (vhTable.Rows.Count > 0)
            {
                rdInNum = Convert.ToInt32(vhTable.Rows[0]["cNumber"].ToString());
            }
            else
            {
                U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('0301',null,null,null,null,0,0) ");
            }
            rdInNum++;
            U8SqlDBHelper.ExecuteSql("update VoucherHistory set cNumber=cNumber+1 where CardNumber ='0301' ");
            string rdInCode = rdInNum.ToString().PadLeft(10, '0');

            string rdOutCodeType = "108";
            string rdInCodeType = "214";
            string requestCode = "null";
            if (tr.cSource == "调拨申请")
            {
                requestCode = "'" + tr.RequestCode + "'";
            }

            sql = " insert into TransVouch (  ID,ctvcode,dtvdate,cowhcode,ciwhcode,cirdcode,cordcode,cODepCode,cIDepCode,  ctvmemo,cmaker,inetlock,VT_ID,cVerifyPerson,dVerifyDate,csource,itransflag,  dnmaketime,dnverifytime,iswfcontrolled,iPrintCount,csysbarcode,cTranRequestCode )  values(  '" + transVouchIdStr + "','" + transVouchCode + "','" + dateTime.ToString("yyyy-MM-dd") + "','" + tr.cOWhCode + "','" + tr.cIWhCode + "','" + rdOutCodeType + "','" + rdInCodeType + "',null,null,  '" + tr.cMemo + "','" + tr.cMaker + "',0,'89','" + tr.cVerifier + "','" + dateTime.ToString("yyyy-MM-dd") + "',1,'正向',  getdate(),getdate(),0,0,'||st12|" + transVouchCode + "'," + requestCode + " ) ";
            list.Add(sql);

            sql = " insert into rdrecord09(id,brdflag,cvouchtype,cbustype,csource,  cwhcode,ddate,ccode,crdcode,chandler,cbuscode,cDepCode,  cmaker,bpufirst,biafirst,vt_id,bisstqc,ibg_overflag,cbg_auditor,cbg_audittime,controlresult,  iswfcontrolled,dnmaketime,dnmodifytime,dVeriDate,dnverifytime,iprintcount,cMemo)   values(" + rdOutIdStr + ",N'0',N'09',N'调拨出库',N'调拨',  '" + tr.cOWhCode + "','" + dateTime.ToString("yyyy-MM-dd") + "','" + rdOutCode + "','" + rdInCodeType + "',N'袁小城','" + transVouchCode + "',NULL,  N'何立涵',0,0,85,0,0,N'',N'',-1,  0,getdate(),Null,'" + dateTime.ToString("yyyy-MM-dd") + "',getdate(),0,'" + tr.cMemo + "') ";
            list.Add(sql);

            sql = " insert into rdrecord08(id,brdflag,cvouchtype,cbustype,csource, cwhcode,ddate,ccode,crdcode,cbuscode,cDepCode,  cmaker,bpufirst,biafirst,vt_id,bisstqc,bislsquery,iswfcontrolled,dnmaketime,dnmodifytime,  cHandler,dVeriDate,dnverifytime,iprintcount,cMemo)  values('" + rdInIdStr + "', N'1', N'08', N'调拨入库', N'调拨',  '" + tr.cIWhCode + "', '" + dateTime.ToString("yyyy-MM-dd") + "', '" + rdInCode + "','" + rdOutCodeType + "','" + transVouchCode + "', NULL,  N'何立涵', 0, 0, 67, 0, 0, 0, getdate(), Null, N'袁小城','" + dateTime.ToString("yyyy-MM-dd") + "', getdate(), 0,'" + tr.cMemo + "') ";
            list.Add(sql);

            int rowNum = 0;
            for (int j = 0; j < items.Count; j++)
            {
                rowNum++;
                string cInvCode = items[j].cInvCode;
                decimal iQuantity = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);

                string batchNo = "null";
                string outPosCode = "null";
                string inPosCode = "null";

                if (BasicDAL.bWhPos(tr.cIWhCode))
                {
                    inPosCode = "'" + items[j].ciPosCode + "'";
                }
                if (BasicDAL.bWhPos(tr.cOWhCode))
                {
                    outPosCode = "'" + items[j].coPosCode + "'";
                }

                if (BasicDAL.IsBatch(cInvCode, ""))
                {
                    batchNo = "'" + items[j].cBatch + "'";
                }

                string madeDate = "null";
                string vDateStart = "null";
                string vDateEnd = "null";
                string massDays = "null";
                string massUnit = "null";

                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    madeDate = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    vDateStart = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    vDateEnd = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    massDays = BasicDAL.GetMassdate(cInvCode);
                    massUnit = BasicDAL.GetMassUnit(cInvCode);
                }

                string trIds = "null";
                if (tr.cSource == "调拨申请")
                {
                    trIds = "'" + items[j].iTRIds + "'";
                }

                int transDetailId = BasicDAL.GetVouchId("C", cAcc_Id, "tr");
                string transDetailIdStr = "1" + $"{transDetailId:D9}";

                sql = " insert into TransVouchs (  ID,autoID, cTVCode,cInvCode,RdsID,iTVNum,iTVQuantity,  cTVBatch,cAssUnit,cInVouchCode,bCosting,iTRIds,  dMadeDate,dDisDate,cExpirationdate,iMassDate,cMassUnit,  irowno,cinvouchtype,cbsysbarcode,cbMemo,coutposcode,cinposcode  )  values (  '" + transVouchIdStr + "','" + transDetailIdStr + "','" + transVouchCode + "','" + items[j].cInvCode + "',null," + iNum + ", " + items[j].iQuantity + ",  " + batchNo + "," + AssUnit + ",'" + rdInCode + "',1," + trIds + ",  " + madeDate + "," + vDateStart + "," + vDateEnd + "," + massDays + "," + massUnit + ",  '" + rowNum + "',null,'||st12|" + transVouchCode + "|" + rowNum + "','" + items[j].cbMemo + "'," + outPosCode + "," + inPosCode + " ) ";
                list.Add(sql);

                int rdOutAutoId = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string rdOutAutoIdStr = "1" + $"{rdOutAutoId:D9}";

                sql = " Insert Into rdrecords09(autoid, id, cinvcode, inum, iquantity, cbatch,   bcosting, isotype, irowno,iinvexchrate,cassunit,iTrIds,cbMemo,  cPosition,dMadeDate,dVDate,cExpirationdate,iMassDate,cMassUnit )  values(" + rdOutAutoIdStr + ", " + rdOutIdStr + ", '" + items[j].cInvCode + "', " + iNum + ", " + items[j].iQuantity + "," + batchNo + ",  1, 0, " + rowNum + ", null, null,'" + transDetailIdStr + "','" + items[j].cbMemo + "',  " + outPosCode + "," + madeDate + "," + vDateStart + "," + vDateEnd + "," + massDays + "," + massUnit + "  )";
                list.Add(sql);

                sql = " Insert Into Rdrecords09sub(autoid,id,cbg_itemcode,cbg_itemname,cbg_caliberkey1,cbg_caliberkeyname1,cbg_caliberkey2,cbg_caliberkeyname2,  cbg_caliberkey3,cbg_caliberkeyname3,cbg_calibercode1,cbg_calibername1,cbg_calibercode2,cbg_calibername2,cbg_calibercode3,cbg_calibername3,  ibg_ctrl,cbg_auditopinion,ibgstsum,ibgiasum,cbg_caliberkey4,cbg_caliberkeyname4,cbg_caliberkey5,cbg_caliberkeyname5,cbg_caliberkey6,  cbg_caliberkeyname6,cbg_calibercode4,cbg_calibername4,cbg_calibercode5,cbg_calibername5,cbg_calibercode6,cbg_calibername6)  values(" + rdOutAutoIdStr + ", " + rdOutIdStr + ", Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, 0, Null, 0, Null, Null, Null, Null,  Null, Null, Null, Null, Null, Null, Null, Null, Null)  ";
                list.Add(sql);

                list.Add("insert into IA_ST_UnAccountVouch09(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + rdOutIdStr + "','" + rdOutAutoIdStr + "','09','其他出库')");

                if (BasicDAL.bWhPos(tr.cOWhCode))
                {
                    string outBatchCond = " and isnull(cbatch,'') = '' ";
                    if (BasicDAL.IsBatch(items[j].cInvCode, ""))
                    {
                        outBatchCond = " and cbatch = " + batchNo + " ";
                    }
                    sql = " insert into InvPosition (  RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,iNum,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,iExpiratDateCalcu,cvouchtype,dVouchDate,  dMadeDate,dVDate,cExpirationdate )  select autoid,id,'" + tr.cOWhCode + "','" + items[j].coPosCode + "',cinvcode,cbatch,  " + iQuantity + "," + iNum + ",'','" + dateTime.ToString("yyyy-MM-dd") + "',0,0,  iMassDate,cMassUnit,iExpiratDateCalcu,'09','" + dateTime.ToString("yyyy-MM-dd") + "',  dMadeDate,dVDate,cExpirationdate  from rdrecords09 where  autoid = '" + rdOutAutoIdStr + "' ";
                    list.Add(sql);

                    string outAssQtyExpr = "";
                    if (assQty != 0m)
                    {
                        outAssQtyExpr = ",isnull(iNum,0) - " + assQty + " ";
                    }
                    sql = "  update invpositionsum set iquantity = iquantity - " + iQuantity + "  " + outAssQtyExpr + " where cwhcode = '" + tr.cOWhCode + "' and cposcode = '" + items[j].coPosCode + "' and cinvcode = '" + cInvCode + "' " + outBatchCond;
                    list.Add(sql);
                }

                int rdInAutoId = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string rdInAutoIdStr = "1" + $"{rdInAutoId:D9}";

                sql = " Insert Into rdrecords08(autoid,id,cinvcode,inum,iquantity,cbatch,  bcosting,iexpiratdatecalcu,isotype,irowno,iinvexchrate,cassunit,iTrIds,cbMemo,  cPosition,dMadeDate,dVDate,cExpirationdate,iMassDate,cMassUnit )  values('" + rdInAutoIdStr + "', '" + rdInIdStr + "', '" + items[j].cInvCode + "', " + iNum + ", " + items[j].iQuantity + "," + batchNo + ",    1, 0,  0, " + rowNum + ", null, " + AssUnit + ",'" + transDetailIdStr + "','" + items[j].cbMemo + "',  " + inPosCode + "," + madeDate + "," + vDateStart + "," + vDateEnd + "," + massDays + "," + massUnit + " )";
                list.Add(sql);

                sql = " insert IA_ST_UnAccountVouch08(IDUN, IDSUN, cVouTypeUN, cBustypeUN)  values ('" + rdInIdStr + "','" + rdInAutoIdStr + "','08','其他入库') ";
                list.Add(sql);

                if (BasicDAL.bWhPos(tr.cIWhCode))
                {
                    string inBatchCond = " and isnull(cbatch,'') = '' ";
                    if (BasicDAL.IsBatch(items[j].cInvCode, ""))
                    {
                        inBatchCond = " and cbatch = " + batchNo + " ";
                    }
                    sql = " insert into InvPosition (  RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,iNum,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,iExpiratDateCalcu,cvouchtype,dVouchDate,  dMadeDate,dVDate,cExpirationdate )  select autoid,id,'" + tr.cIWhCode + "','" + items[j].ciPosCode + "',cinvcode,cbatch,   " + iQuantity + "," + iNum + ",'','" + dateTime.ToString("yyyy-MM-dd") + "',1,0,  iMassDate,cMassUnit,iExpiratDateCalcu,'08','" + dateTime.ToString("yyyy-MM-dd") + "',  dMadeDate,dVDate,cExpirationdate  from rdrecords08 where autoid = '" + rdInAutoIdStr + "' ";
                    list.Add(sql);

                    sql = " select 1 from invpositionsum (nolock)  where cwhcode = '" + tr.cIWhCode + "' and cposcode = '" + items[j].ciPosCode + "' and cinvcode = '" + cInvCode + "' " + inBatchCond;
                    DataTable posSumTable = U8SqlDBHelper.GetDataTable(sql);
                    if (posSumTable.Rows.Count == 0)
                    {
                        sql = "  insert into invpositionsum (cWhCode,cPosCode,cInvCode,iQuantity,cBatch,  iTrackid,dMadeDate,dVDate,cExpirationdate,cMassUnit,iMassDate)  values ( '" + tr.cIWhCode + "','" + items[j].ciPosCode + "','" + cInvCode + "', 0 ," + batchNo + ",  0," + madeDate + "," + vDateStart + "," + vDateEnd + "," + massUnit + "," + massDays + " )";
                        U8SqlDBHelper.ExecuteSql(sql);
                    }

                    string inAssQtyExpr = "";
                    if (assQty != 0m)
                    {
                        inAssQtyExpr = ",isnull(iNum,0) + " + assQty + " ";
                    }
                    sql = "  update invpositionsum set iquantity = iquantity + " + iQuantity + "  " + inAssQtyExpr + " where cwhcode = '" + tr.cIWhCode + "' and cposcode = '" + items[j].ciPosCode + "' and cinvcode = '" + cInvCode + "' " + inBatchCond;
                    list.Add(sql);
                }
            }

            sql = "exec pro_uptcurrentStock '" + zt + "','09','" + rdOutIdStr + "'";
            list.Add(sql);
            sql = "exec pro_uptcurrentStock '" + zt + "','08','" + rdInIdStr + "'";
            list.Add(sql);

            int execResult = U8SqlDBHelper.ExecuteSqlTran(list);
            if (execResult > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8调拨单[" + transVouchCode + "]创建成功！\",\"U8Code\":\"" + transVouchCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8调拨单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string errorMsg = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
        }
    }

    public static string RD11(RD11 rd)
    {
        List<string> sqlList = new List<string>();
        string sql = "";
        try
        {
            List<RDs11> items = rd.Items;
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cmaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord11 where ccode = '" + rd.cCode + "' ";
            DataTable dt = U8SqlDBHelper.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime docDate;
            try
            {
                docDate = Convert.ToDateTime(rd.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string period = docDate.Year + docDate.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + period + "' ";
            string bflagST = U8SqlDBHelper.GetString(sql);
            if (bflagST == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[cwhcode]未传递！\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            DataTable dtCheck = U8SqlDBHelper.GetDataTable(sql);
            if (dtCheck.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\"}";
            }
            if (!string.IsNullOrEmpty(items[0].MoCode))
            {
                if (string.IsNullOrEmpty(rd.cDepCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[cDepCode]未传递！\"}";
                }
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable dtCheck2 = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck2.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                }
                if (items[i].iQuantity == 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]不可为0！\"}";
                }
                if (items[0].iQuantity > 0m && items[i].iQuantity < 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]传递异常！\"}";
                }
                if (items[0].iQuantity < 0m && items[i].iQuantity > 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]传递异常！\"}";
                }
                if (!string.IsNullOrEmpty(items[i].MoCode))
                {
                    sql = " select modid from mom_orderdetail a (nolock) left join mom_order b (nolock) on a.moid=b.moid  where isnull(a.CloseUser,'')='' and b.mocode = '" + items[i].MoCode + "' ";
                    dt = U8SqlDBHelper.GetDataTable(sql);
                    if (dt.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"工单号[" + items[i].MoCode + "]无数据或已关闭！\"}";
                    }
                    string moDId = dt.Rows[0]["modid"].ToString();
                    sql = "select Qty,IssQty from mom_moallocate where MoDId='" + moDId + "' and InvCode ='" + items[i].cInvCode + "'";
                    dt = U8SqlDBHelper.GetDataTable(sql);
                    if (dt.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"工单号[" + items[i].MoCode + "]无存货[" + items[i].cInvCode + "]对应子件信息！\"}";
                    }
                    decimal issuedQty = BasicDAL.ToDec(dt.Rows[0]["IssQty"].ToString());
                    if (issuedQty + items[i].iQuantity < 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"工单号[" + items[i].MoCode + "]存货[" + items[i].cInvCode + "]累计领料数量不可小于0！\"}";
                    }
                }
                if ((string.IsNullOrEmpty(items[0].MoCode) && !string.IsNullOrEmpty(items[i].MoCode)) || (!string.IsNullOrEmpty(items[0].MoCode) && string.IsNullOrEmpty(items[i].MoCode)))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据来源不一致！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime madeDate = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"生产日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    if (string.IsNullOrEmpty(items[i].cPosCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"仓库[" + rd.cWhCode + "]启用货位管理，货位[cPosCode]未传递！\"}";
                    }
                    sql = " select 1 from Position where cPosCode='" + items[i].cPosCode + "' and cWhCode='" + rd.cWhCode + "' and bPosEnd=1 ";
                    dt = U8SqlDBHelper.GetDataTable(sql);
                    if (dt.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"货位编码[" + items[i].cPosCode + "]无数据！\"}";
                    }
                }
                if (items[i].iQuantity > 0m)
                {
                    sql = " select iQuantity from currentstock where cwhcode='" + rd.cWhCode + "' and cinvcode='" + items[i].cInvCode + "' ";
                    decimal stockQty = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                    if (items[i].iQuantity > stockQty)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]现存量不足！\"}";
                    }
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string vouchIdStr = "1" + $"{vouchId:D9}";
            string sourceName = "库存";
            string personCodeSql = "null";
            decimal orderQty = default(decimal);
            string moIdSql = "null";
            string invCodeSql = "null";
            string moCodeSql = "null";
            string deptCode = "12";
            string crdCode = "218";
            if (!string.IsNullOrEmpty(items[0].MoCode))
            {
                crdCode = "202";
                sourceName = "生产订单";
                sql = " select MDeptCode,InvCode,Qty,a.moid from mom_orderdetail a (nolock) left join mom_order b (nolock) on a.moid=b.moid  where isnull(a.CloseUser,'')='' and b.mocode = '" + items[0].MoCode + "' ";
                DataTable dtOrder = U8SqlDBHelper.GetDataTable(sql);
                orderQty = BasicDAL.ToDec(dtOrder.Rows[0]["Qty"].ToString());
                moIdSql = "'" + dtOrder.Rows[0]["moid"].ToString() + "'";
                invCodeSql = "'" + dtOrder.Rows[0]["InvCode"].ToString() + "'";
                moCodeSql = "'" + items[0].MoCode + "'";
                if (!string.IsNullOrEmpty(rd.cPersonCode))
                {
                    personCodeSql = "'" + rd.cPersonCode + "'";
                }
                deptCode = dtOrder.Rows[0]["MDeptCode"].ToString();
            }
            sql = " insert into rdrecord11(ID,brdflag,cvouchtype,cbustype,csource,cbuscode,cvencode,cwhcode,ddate,  ccode,crdcode,cdepcode,cpersoncode,chandler,cmemo,btransflag,cmaker,dveridate,bpufirst,biafirst,vt_id,  bisstqc,bOMFirst,bfrompreyear,biscomplement,ireturncount,iverifystate,dnmaketime,dnverifytime,bredvouch,  bmotran,bhyvouch,iprintcount,cpspcode,iMQuantity,cmpocode,iproorderid,iswfcontrolled,csysbarcode )  values( " + vouchIdStr + ",0,11,'领料','" + sourceName + "',null,null,'" + rd.cWhCode + "','" + docDate.ToString("yyyy-MM-dd") + "',  '" + rd.cCode + "','" + crdCode + "','" + deptCode + "'," + personCodeSql + ",'" + rd.cVerifier + "','" + rd.cMemo + "',0,'" + rd.cMaker + "','" + docDate.ToString("yyyy-MM-dd") + "',0,0,'65',  0,0,0,0,0,0,getdate(),getdate(),0,  0,0,0," + invCodeSql + "," + orderQty + "," + moCodeSql + "," + moIdSql + ",0,'||st11|" + rd.cCode + "') ";
            sqlList.Add(sql);
            int rowNo = 0;
            for (int j = 0; j < items.Count; j++)
            {
                rowNo++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string autoId = "1" + $"{vouchId2:D9}";
                string cInvCode = items[j].cInvCode;
                decimal iQuantity = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                string batchSql = "null";
                if (BasicDAL.IsBatch(cInvCode, ""))
                {
                    batchSql = "'" + items[j].cBatch + "'";
                }
                string madeDateSql = "null";
                string vDateSql = "null";
                string expDateSql = "null";
                string massDateSql = "null";
                string massUnitSql = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    madeDateSql = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    vDateSql = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    expDateSql = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    massDateSql = BasicDAL.GetMassdate(cInvCode);
                    massUnitSql = BasicDAL.GetMassUnit(cInvCode);
                }
                string moDIdSql = "null";
                string detailInvCodeSql = "null";
                decimal detailQty = default(decimal);
                int moSortSeq = 0;
                string allocateIdSql = "null";
                string itemClass = "NULL";
                string itemName = "NULL";
                string itemCode = "NULL";
                string itemCName = "NULL";
                if (!string.IsNullOrEmpty(items[j].MoCode))
                {
                    sql = " select modid from mom_orderdetail a (nolock) left join mom_order b (nolock) on a.moid=b.moid  where isnull(a.CloseUser,'')='' and b.mocode = '" + items[j].MoCode + "' ";
                    moDIdSql = "'" + U8SqlDBHelper.GetString(sql) + "'";
                    sql = " select b.MDeptCode,b.InvCode,b.Qty,b.moid,b.SortSeq,Define25,Define24,Define33 from mom_orderdetail b (nolock)   where modid = " + moDIdSql + "  ";
                    DataTable dtDetail = U8SqlDBHelper.GetDataTable(sql);
                    detailQty = BasicDAL.ToDec(dtDetail.Rows[0]["Qty"].ToString());
                    detailInvCodeSql = "'" + dtDetail.Rows[0]["InvCode"].ToString() + "'";
                    moSortSeq = BasicDAL.ToInt(dtDetail.Rows[0]["SortSeq"].ToString());
                    allocateIdSql = "'" + U8SqlDBHelper.GetString("select AllocateId from mom_moallocate where MoDId=" + moDIdSql + " and InvCode ='" + items[j].cInvCode + "'") + "'";
                    if (!string.IsNullOrEmpty(dtDetail.Rows[0]["Define25"].ToString()))
                    {
                        sql = string.Concat(" select * from fitem (nolock) where citem_class='", dtDetail.Rows[0]["Define25"], "' ");
                        dt = U8SqlDBHelper.GetDataTable(sql);
                        if (dt.Rows.Count > 0)
                        {
                            itemClass = string.Concat("'", dtDetail.Rows[0]["Define25"], "'");
                            itemName = string.Concat("'", dt.Rows[0]["citem_name"], "'");
                            itemCode = string.Concat("'", dtDetail.Rows[0]["Define24"], "'");
                            itemCName = string.Concat("'", dtDetail.Rows[0]["Define33"], "'");
                        }
                    }
                }
                else
                {
                    itemClass = "'07'";
                    itemName = "'生产项目'";
                    itemCode = "'2024003'";
                    itemCName = "'通用辅消料'";
                }
                sql = " insert into rdrecords11(autoid,id,cInvCode,iQuantity,iNum,iFlag,iNQuantity,iMPoIds,bLPUseFree,iOriTrackID,bCosting,bVMIUsed,  iMaIDs,cmocode,invcode,imoseq,iopseq,iorderdid,iordertype,iordercode,iorderseq ,isotype,ipesodid,ipesotype,  cpesocode,ipesoseq,irowno,bcanreplace,cBAccounter ,cBatch,cbMemo,isodid,csocode,isoseq,  cItemCode, cName ,cItem_class, cItemCName,imassdate,cmassunit, cdefine23,  dMadeDate,dVDate,cExpirationdate,cbsysbarcode,cPosition,iinvexchrate,cAssUnit   )   values(" + autoId + ", " + vouchIdStr + ", '" + cInvCode + "'," + iQuantity + "," + iNum + ",0," + detailQty + " ," + allocateIdSql + ", 0, 0,1, 0,  null,'" + items[j].MoCode + "'," + detailInvCodeSql + "," + moSortSeq + ",'0000',null,0, null,null, 0," + allocateIdSql + ",'7',  '" + items[j].MoCode + "'," + moSortSeq + "," + rowNo + ", 0, null," + batchSql + ", '" + items[j].cmemo + "', null,null,null,  " + itemCode + " ," + itemCName + " ," + itemClass + " ," + itemName + " ," + massDateSql + "," + massUnitSql + ", '" + items[j].cdefine23 + "', " + madeDateSql + "," + vDateSql + "," + expDateSql + ",'||st11|" + rd.cCode + "|" + rowNo + "',null," + iRate + "," + AssUnit + "  ) ";
                sqlList.Add(sql);
                if (allocateIdSql != "null")
                {
                    sql = " update mom_moallocate set IssQty = isnull(IssQty,0) + " + iQuantity + " where AllocateId = " + allocateIdSql + " ";
                    sqlList.Add(sql);
                }
                sqlList.Add("insert into IA_ST_UnAccountVouch11(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + vouchIdStr + "','" + autoId + "','11','领料')");
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    string batchCondition = " and isnull(cbatch,'') = '' ";
                    if (BasicDAL.IsBatch(cInvCode, zt))
                    {
                        batchCondition = " and cbatch = '" + items[j].cBatch + "' ";
                    }
                    sql = " insert into InvPosition (  RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,iNum,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,iExpiratDateCalcu,cvouchtype,dVouchDate,  dMadeDate,dVDate,cExpirationdate )  select autoid,id,'" + rd.cWhCode + "','" + items[j].cPosCode + "',cinvcode,cbatch,  " + iQuantity + "," + iNum + ",'','" + docDate.ToString("yyyy-MM-dd") + "',0,0,  iMassDate,cMassUnit,iExpiratDateCalcu,'11','" + docDate.ToString("yyyy-MM-dd") + "',  dMadeDate,dVDate,cExpirationdate  from rdrecords32 where ID = '" + vouchIdStr + "' and autoid = '" + autoId + "' ";
                    sqlList.Add(sql);
                    sql = "  update invpositionsum set iquantity = iquantity - " + iQuantity + ",iNum=isnull(iNum,0)-isnull(" + iNum + ",0)   where cwhcode = '" + rd.cWhCode + "' and cposcode = '" + items[j].cPosCode + "' and cinvcode = '" + cInvCode + "' " + batchCondition;
                    sqlList.Add(sql);
                }
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','11','" + vouchIdStr + "' ";
            sqlList.Add(sql);
            int affectedRows = U8SqlDBHelper.ExecuteSqlTran(sqlList);
            if (affectedRows > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8材料出库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8材料出库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string errorMsg = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
        }
    }

    public static string OMRD11(RD11 rd)
    {
        List<string> sqlList = new List<string>();
        string sql = "";
        try
        {
            List<RDs11> items = rd.Items;
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cmaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord11 where ccode = '" + rd.cCode + "' ";
            DataTable dt = U8SqlDBHelper.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime docDate;
            try
            {
                docDate = Convert.ToDateTime(rd.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string period = docDate.Year + docDate.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + period + "' ";
            string bflagST = U8SqlDBHelper.GetString(sql);
            if (bflagST == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[cwhcode]未传递！\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            DataTable dtCheck = U8SqlDBHelper.GetDataTable(sql);
            if (dtCheck.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\"}";
            }
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            else
            {
                sql = "select cDepCode from OM_MOMain where cCode='" + items[0].OmCode + "'";
                string deptFromOm = U8SqlDBHelper.GetString(sql);
                if (string.IsNullOrEmpty(deptFromOm))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[cDepCode]未传递！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].OmCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单号[OmCode]未传递！\"}";
                }
                if (string.IsNullOrEmpty(items[i].Irowno))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单行号[Irowno]未传递！\"}";
                }
                if (BasicDAL.ToInt(items[i].Irowno) == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单行号[" + items[i].Irowno + "]格式错误！\"}";
                }
                sql = "select iReceivedQTY,iQuantity,cbCloser,MODetailsID from OM_MODetails a left join OM_MOMain b on a.MOID=b.MOID  where b.cCode = '" + items[i].OmCode + "' and a.iVouchRowNo = '" + items[i].Irowno + "' ";
                dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单[" + items[i].OmCode + "]行[" + items[i].Irowno + "]无数据！\"}";
                }
                string cbCloser = dt.Rows[0]["cbCloser"].ToString();
                if (!string.IsNullOrEmpty(cbCloser))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单[" + items[i].OmCode + "]行[" + items[i].Irowno + "]已关闭，不可继续出库！\"}";
                }
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable dtCheck2 = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck2.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                }
                sql = string.Concat(" select 1 from OM_MOMaterials where MoDetailsID = '", dt.Rows[0]["MODetailsID"], "' and cInvCode='", items[i].cInvCode, "' ");
                dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单[" + items[i].OmCode + "]行[" + items[i].Irowno + "]无[" + items[i].cInvCode + "]子件数据！\"}";
                }
                if (items[i].iQuantity == 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]不可为0！\"}";
                }
                if (items[0].iQuantity > 0m && items[i].iQuantity < 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]传递异常！\"}";
                }
                if (items[0].iQuantity < 0m && items[i].iQuantity > 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]传递异常！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime madeDate = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"生产日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    if (string.IsNullOrEmpty(items[i].cPosCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"仓库[" + rd.cWhCode + "]启用货位管理，货位[cPosCode]未传递！\"}";
                    }
                    sql = " select 1 from Position where cPosCode='" + items[i].cPosCode + "' and cWhCode='" + rd.cWhCode + "' and bPosEnd=1 ";
                    dt = U8SqlDBHelper.GetDataTable(sql);
                    if (dt.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"货位编码[" + items[i].cPosCode + "]无数据！\"}";
                    }
                }
                if (items[i].iQuantity > 0m)
                {
                    sql = " select iQuantity from currentstock where cwhcode='" + rd.cWhCode + "' and cinvcode='" + items[i].cInvCode + "' ";
                    decimal stockQty = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                    if (items[i].iQuantity > stockQty)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]现存量不足！\"}";
                    }
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string vouchIdStr = "1" + $"{vouchId:D9}";
            string deptCodeSql = "cDepCode";
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                deptCodeSql = "'" + rd.cDepCode + "'";
            }
            string personCodeSql = "null";
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                personCodeSql = "'" + rd.cPersonCode + "'";
            }
            sql = "select a.cInvCode,a.iQuantity,MODetailsID,b.cVenCode from OM_MODetails a left join OM_MOMain b on a.MOID=b.MOID  where b.cCode = '" + items[0].OmCode + "' and a.iVouchRowNo = '" + items[0].Irowno + "' ";
            DataTable dtOrder = U8SqlDBHelper.GetDataTable(sql);
            string invCodeSql = string.Concat("'", dtOrder.Rows[0]["cInvCode"], "'");
            string orderQtySql = string.Concat("'", dtOrder.Rows[0]["iQuantity"], "'");
            string venCode = dtOrder.Rows[0]["cVenCode"].ToString();
            sql = " insert into rdrecord11(ID,brdflag,cvouchtype,cbustype,csource,cbuscode,cvencode,cwhcode,ddate,  ccode,crdcode,cdepcode,cpersoncode,chandler,cmemo,btransflag,cmaker,dveridate,bpufirst,biafirst,vt_id,  bisstqc,bOMFirst,bfrompreyear,biscomplement,ireturncount,iverifystate,dnmaketime,dnverifytime,bredvouch,  bmotran,bhyvouch,iprintcount,cpspcode,iMQuantity,cmpocode,iproorderid,iswfcontrolled,csysbarcode )  select " + vouchIdStr + ",0,11,'委外发料','委外订单', null ,'" + venCode + "','" + rd.cWhCode + "','" + docDate.ToString("yyyy-MM-dd") + "',  '" + rd.cCode + "','203'," + deptCodeSql + "," + personCodeSql + ",'" + rd.cVerifier + "','" + rd.cMemo + "',0,'" + rd.cMaker + "','" + docDate.ToString("yyyy-MM-dd") + "',0,0,'65',  0,0,0,0,0,0,getdate(),getdate(),0,   0,0,0, " + invCodeSql + "," + orderQtySql + ",cCode ,MOID ,0,'||st11|" + rd.cCode + "'  from OM_MOMain where cCode = '" + items[0].OmCode + "' ";
            sqlList.Add(sql);
            int rowNo = 0;
            for (int j = 0; j < items.Count; j++)
            {
                rowNo++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string autoId = "1" + $"{vouchId2:D9}";
                string cInvCode = items[j].cInvCode;
                decimal iQuantity = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                string batchSql = "null";
                if (BasicDAL.IsBatch(cInvCode, ""))
                {
                    batchSql = "'" + items[j].cBatch + "'";
                }
                string madeDateSql = "null";
                string vDateSql = "null";
                string expDateSql = "null";
                string massDateSql = "null";
                string massUnitSql = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    madeDateSql = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    vDateSql = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    expDateSql = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    massDateSql = BasicDAL.GetMassdate(cInvCode);
                    massUnitSql = BasicDAL.GetMassUnit(cInvCode);
                }
                string posCodeSql = "null";
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    posCodeSql = "'" + items[j].cPosCode + "'";
                }
                sql = "select a.cInvCode,a.iQuantity,a.MODetailsID from OM_MODetails a left join OM_MOMain b on a.MOID=b.MOID  where b.cCode = '" + items[j].OmCode + "' and a.iVouchRowNo = '" + items[j].Irowno + "' ";
                dtOrder = U8SqlDBHelper.GetDataTable(sql);
                string moDetailsId = dtOrder.Rows[0]["MODetailsID"].ToString();
                decimal detailQty = BasicDAL.ToDec(dtOrder.Rows[0]["iQuantity"].ToString());
                string moMaterialsId = U8SqlDBHelper.GetString("select MOMaterialsID from OM_MOMaterials where MoDetailsID = '" + moDetailsId + "' and cInvCode='" + cInvCode + "'");
                sql = " insert into rdrecords11(autoid,id,cInvCode,iQuantity,iNum,iFlag,iNQuantity,iMPoIds,bLPUseFree,iOriTrackID,bCosting,bVMIUsed,  iMaIDs,cmocode,invcode,imoseq,iopseq,iorderdid,iordertype,iordercode,iorderseq ,isotype,ipesodid,ipesotype,  cpesocode,ipesoseq,irowno,bcanreplace,cBAccounter ,cBatch,cbMemo,isodid,csocode,isoseq,  cItemCode, cName ,cItem_class, cItemCName,imassdate,cmassunit,iOMoMID,iOMoDID,comcode,  dMadeDate,dVDate,cExpirationdate,cbsysbarcode,cPosition,iinvexchrate,cAssUnit   )   select " + autoId + ", " + vouchIdStr + ", '" + cInvCode + "'," + iQuantity + "," + iNum + ",0," + detailQty + " , null, 0, 0, 1, 0,  null,null, cInvCode,null,null ,null,0,  null,null, 0,MODetailsID,'8',  '" + items[j].OmCode + "',null," + rowNo + ", 0, null," + batchSql + ", '', null,null,null,  cItemCode,cItemName, cItem_class, null," + massDateSql + "," + massUnitSql + ",'" + moMaterialsId + "',MoDetailsID,'" + items[j].OmCode + "', " + madeDateSql + "," + vDateSql + "," + expDateSql + ",'||st11|" + rd.cCode + "|" + rowNo + "'," + posCodeSql + "," + iRate + "," + AssUnit + "  from OM_MODetails where MODetailsID = '" + moDetailsId + "'  ";
                sqlList.Add(sql);
                sql = " update OM_MOMaterials set iSendQTY = isnull(iSendQTY,0) + " + iQuantity + " where MOMaterialsID = '" + moMaterialsId + "' ";
                sqlList.Add(sql);
                sqlList.Add("insert into IA_ST_UnAccountVouch11(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + vouchIdStr + "','" + autoId + "','11','委外发料')");
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    string batchCondition = " and isnull(cbatch,'') = '' ";
                    if (BasicDAL.IsBatch(cInvCode, zt))
                    {
                        batchCondition = " and cbatch = '" + items[j].cBatch + "' ";
                    }
                    sql = " insert into InvPosition (  RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,iNum,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,iExpiratDateCalcu,cvouchtype,dVouchDate,  dMadeDate,dVDate,cExpirationdate )  select autoid,id,'" + rd.cWhCode + "','" + items[j].cPosCode + "',cinvcode,cbatch,  " + iQuantity + "," + iNum + ",'','" + docDate.ToString("yyyy-MM-dd") + "',0,0,  iMassDate,cMassUnit,iExpiratDateCalcu,'11','" + docDate.ToString("yyyy-MM-dd") + "',  dMadeDate,dVDate,cExpirationdate  from rdrecords32 where ID = '" + vouchIdStr + "' and autoid = '" + autoId + "' ";
                    sqlList.Add(sql);
                    sql = "  update invpositionsum set iquantity = iquantity - " + iQuantity + ",iNum=isnull(iNum,0)-isnull(" + iNum + ",0)   where cwhcode = '" + rd.cWhCode + "' and cposcode = '" + items[j].cPosCode + "' and cinvcode = '" + cInvCode + "' " + batchCondition;
                    sqlList.Add(sql);
                }
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','11','" + vouchIdStr + "' ";
            sqlList.Add(sql);
            int affectedRows = U8SqlDBHelper.ExecuteSqlTran(sqlList);
            if (affectedRows > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8委外材料出库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8委外材料出库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string errorMsg = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
        }
    }

    public static string RD01(RD01 rd)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord01 where ccode = '" + rd.cCode + "' ";
            DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(rd.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
            string result = U8SqlDBHelper.GetString(sql);
            if (result == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[cWhCode]未传递！\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "'  ";
            DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable2.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\"}";
            }
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<RDs01> items = rd.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].CPOID))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"采购订单号[CPOID]未传递！\"}";
                }
                if (string.IsNullOrEmpty(items[i].irowno))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"采购订单行号[irowno]未传递！\"}";
                }
                if (BasicDAL.ToInt(items[i].irowno) == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"采购订单行号[" + items[i].irowno + "]格式错误！\"}";
                }
                sql = "select freceivedqty,iQuantity,cbCloser,cInvCode from PO_Podetails a left join po_pomain b on a.poid=b.poid  where b.cpoid = '" + items[i].CPOID + "' and a.ivouchrowno = '" + items[i].irowno + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"采购订单[" + items[i].CPOID + "]行[" + items[i].irowno + "]无数据！\"}";
                }
                string value = dataTable.Rows[0]["cbCloser"].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"采购订单[" + items[i].CPOID + "]行[" + items[i].irowno + "]已关闭，不可继续入库！\"}";
                }
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable3.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                }
                if (items[i].cInvCode != dataTable.Rows[0]["cInvCode"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]与订单不一致！\"}";
                }
                if (items[i].iQuantity == 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]不可为0！\"}";
                }
                if (items[0].iQuantity > 0m && items[i].iQuantity < 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]传递异常！\"}";
                }
                if (items[0].iQuantity < 0m && items[i].iQuantity > 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]传递异常！\"}";
                }
                decimal num = BasicDAL.ToDec(dataTable.Rows[0]["freceivedqty"].ToString());
                decimal num2 = BasicDAL.ToDec(dataTable.Rows[0]["iQuantity"].ToString());
                if (num + items[i].iQuantity > num2)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"采购订单[" + items[i].CPOID + "]行[" + items[i].irowno + "]累计入库数量大于订单数量！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime madeDate = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"生产日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    if (string.IsNullOrEmpty(items[i].cPosCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"仓库[" + items[i].cInvCode + "]启用货位管理，货位[cPosCode]未传递！\"}";
                    }
                    sql = " select 1 from Position where cPosCode='" + items[i].cPosCode + "' and cWhCode='" + rd.cWhCode + "' and bPosEnd=1 ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"货位编码[" + items[i].cPosCode + "]无数据！\"}";
                    }
                }
                if (!(items[i].iQuantity < 0m))
                {
                    continue;
                }
                sql = " select iQuantity from CurrentStock where cWhCode='" + rd.cWhCode + "' and cInvCode = '" + items[i].cInvCode + "' ";
                if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                {
                    sql = sql + " and cBatch='" + items[i].cBatch + "' ";
                }
                decimal num3 = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                if (Math.Abs(items[i].iQuantity) > num3)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]批号[" + items[i].cBatch + "]仓库现存量不足！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    sql = " select iQuantity from InvPositionSum where cWhCode='" + rd.cWhCode + "' and cPosCode='" + items[i].cPosCode + "' and cInvCode='" + items[i].cInvCode + "' ";
                    if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                    {
                        sql = sql + " and cBatch='" + items[i].cBatch + "' ";
                    }
                    decimal num4 = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                    if (Math.Abs(items[i].iQuantity) > num4)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]货位[" + items[i].cPosCode + "]批号[" + items[i].cBatch + "]货位现存量不足！\"}";
                    }
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string vouchIdStr = "1" + $"{vouchId:D9}";
            sql = " select a.cpoid,a.nflat,a.cvencode,a.POID,a.cDepCode,a.cPersonCode from po_pomain a   left join PO_Podetails b on a.POID=b.POID where a.cpoid='" + items[0].CPOID + "' and b.ivouchrowno = '" + items[0].irowno + "' ";
            DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
            string detailId = dataTable4.Rows[0]["cpoid"].ToString();
            decimal num5 = BasicDAL.ToDec(dataTable4.Rows[0]["nflat"].ToString());
            string itemId = dataTable4.Rows[0]["cvencode"].ToString();
            string value1 = dataTable4.Rows[0]["POID"].ToString();
            string value2 = dataTable4.Rows[0]["cDepCode"].ToString();
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                value2 = rd.cDepCode;
            }
            string value3 = dataTable4.Rows[0]["cPersonCode"].ToString();
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                value3 = rd.cPersonCode;
            }
            sql = " insert into RdRecord01 (id,brdflag,cvouchtype,cbustype,csource,cwhcode,ddate,ccode,crdcode,cdepcode,  cptcode,cvencode,cPersonCode ,cordercode,cmaker,bpufirst,vt_id,bisstqc,itaxrate,iexchrate,  cexch_name,bomfirst,idiscounttaxtype,iswfcontrolled,dnmaketime,bredvouch,bcredit,iprintcount,cvenpuomprotocol,cHandler ,csysbarcode,  dVeriDate ,dnverifytime,ipurorderid,ipurarriveid,cARVCode,cMemo,dARVDate ,cChkCode,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select " + vouchIdStr + ",'1','01',cbustype,'采购订单','" + rd.cWhCode + "','" + dateTime.ToString("yyyy-MM-dd") + "','" + rd.cCode + "','101','" + value2 + "',  cptcode, cvencode,'" + value3 + "','" + detailId + "','" + rd.cMaker + "','0','27','0',iTaxRate , nflat,  cexch_name ,'0','0','0',getdate(),'0','0','0',cvenpuomprotocol ,'" + rd.cVerifier + "','||st01|" + rd.cCode + "',  '" + dateTime.ToString("yyyy-MM-dd") + "',getdate(),'" + value1 + "', NULL , NULL,'" + rd.cMemo + "', NULL, NULL,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16   from PO_Pomain where cpoid = '" + detailId + "' ";
            list.Add(sql);
            int num6 = 0;
            for (int j = 0; j < items.Count; j++)
            {
                num6++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string batchValue = "1" + $"{vouchId2:D9}";
                string cInvCode = items[j].cInvCode;
                decimal iQuantity = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                string cDefine = items[j].cDefine23;
                string posCodeValue = "null";
                if (BasicDAL.IsBatch(cInvCode, ""))
                {
                    posCodeValue = "'" + items[j].cBatch + "'";
                }
                string madeDateValue = "null";
                string vDateValue = "null";
                string expDateValue = "null";
                string massDateValue = "null";
                string massUnitValue = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    madeDateValue = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    vDateValue = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    expDateValue = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    massDateValue = BasicDAL.GetMassdate(cInvCode);
                    massUnitValue = BasicDAL.GetMassUnit(cInvCode);
                }
                string cPOID = items[j].CPOID;
                sql = " select a.* from PO_Podetails a (nolock) left join po_pomain b on a.poid=b.poid  where b.cpoid = '" + items[j].CPOID + "' and a.ivouchrowno = '" + items[j].irowno + "' ";
                DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
                string batchClause = dataTable5.Rows[0]["ID"].ToString();
                decimal num7 = default(decimal);
                decimal num8 = default(decimal);
                decimal num9 = default(decimal);
                decimal num10 = default(decimal);
                decimal num11 = default(decimal);
                decimal num12 = default(decimal);
                decimal num13 = default(decimal);
                decimal num14 = default(decimal);
                decimal num15 = default(decimal);
                decimal num16 = default(decimal);
                decimal num17 = default(decimal);
                num7 = BasicDAL.ToDec(dataTable5.Rows[0]["iPerTaxRate"].ToString());
                num8 = BasicDAL.ToDec(dataTable5.Rows[0]["iUnitPrice"].ToString());
                num9 = BasicDAL.ToDec(dataTable5.Rows[0]["iTaxPrice"].ToString());
                num10 = Math.Round(num8 * iQuantity, 2);
                num11 = Math.Round(num9 * iQuantity, 2);
                num12 = num11 - num10;
                num13 = num5 * num8;
                num14 = num5 * num9;
                num15 = Math.Round(num13 * iQuantity, 2);
                num16 = Math.Round(num14 * iQuantity, 2);
                num17 = num16 - num15;
                sql = " insert into rdrecords01(autoid,id,cinvcode,inum,iquantity,iunitcost,iprice,iaprice,  isquantity , isnum, imoney, ifnum, ifquantity, iposid, facost, inquantity, innum, cassunit,  ioritaxcost,ioricost,iorimoney,ioritaxprice,iorisum,itaxrate,itaxprice,  isum,btaxcost,cpoid,bcosting,bvmiused,iinvexchrate,iexpiratdatecalcu,iordertype,isotype,  irowno ,cBatch ,iArrsId ,cbarvcode,cPosition,chVencode,  cItemCode, cName ,cItem_class, cItemCName ,imassdate,cmassunit,  dMadeDate,dVDate,cExpirationdate,cCheckCode,iCheckIdBaks,cCheckPersonCode,dCheckDate,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )  select " + batchValue + "," + vouchIdStr + ",cinvcode," + iNum + "," + iQuantity + "," + num13 + "," + num10 + "," + num10 + ",  0,0,0,null,null,'" + batchClause + "'," + num13 + "," + iQuantity + "," + iNum + "," + AssUnit + ",  " + num9 + "," + num8 + "," + num10 + "," + num12 + "," + num11 + "," + num7 + "," + num17 + ",  " + num16 + ",1,'" + cPOID + "', 1,null," + iRate + ",0,0,0,  '" + num6 + "', " + posCodeValue + ",NULL , NULL, NULL ,'" + itemId + "',  NULL , NULL, NULL,null, " + massDateValue + "," + massUnitValue + " ,  " + madeDateValue + "," + vDateValue + "," + expDateValue + ", null,null,null,null ,  cDefine22,'" + cDefine + "',cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37   from PO_Podetails (nolock) where ID = '" + batchClause + "' ";
                list.Add(sql);
                list.Add("insert into IA_ST_UnAccountVouch01(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + vouchIdStr + "','" + batchValue + "','01','普通采购')");
                sql = "update PO_Podetails set iReceivedQTY =isnull(iReceivedQTY,0)+" + iQuantity + ", iReceivedNum=isnull(iReceivedNum,0)+" + iNum + ", iReceivedMoney=isnull(iReceivedMoney,0)+ " + num10 + "  where ID='" + batchClause + "'";
                list.Add(sql);
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    string iaCreateBillFlag = " and isnull(cbatch,'') = '' ";
                    if (BasicDAL.IsBatch(cInvCode, zt))
                    {
                        iaCreateBillFlag = " and cbatch = '" + items[j].cBatch + "' ";
                    }
                    sql = " insert into InvPosition (  RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,iNum,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,iExpiratDateCalcu,cvouchtype,dVouchDate,  dMadeDate,dVDate,cExpirationdate )  select autoid,id,'" + rd.cWhCode + "','" + items[j].cPosCode + "',cinvcode,cbatch,  " + iQuantity + "," + iNum + ",'','" + dateTime.ToString("yyyy-MM-dd") + "',0,0,  iMassDate,cMassUnit,iExpiratDateCalcu,'11','" + dateTime.ToString("yyyy-MM-dd") + "',  dMadeDate,dVDate,cExpirationdate  from rdrecords01 where ID = '" + vouchIdStr + "' and autoid = '" + batchValue + "' ";
                    list.Add(sql);
                    sql = "  update invpositionsum set iquantity = iquantity + " + iQuantity + ",iNum=isnull(iNum,0)+isnull(" + iNum + ",0)   where cwhcode = '" + rd.cWhCode + "' and cposcode = '" + items[j].cPosCode + "' and cinvcode = '" + cInvCode + "' " + iaCreateBillFlag;
                    list.Add(sql);
                }
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','01','" + vouchIdStr + "' ";
            list.Add(sql);
            int num18 = U8SqlDBHelper.ExecuteSqlTran(list);
            if (num18 > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8采购入库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8采购入库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string batchClause2 = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + batchClause2 + "\",\"U8Code\":\"\"}";
        }
    }

    public static string OMRD01(RD01 rd)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord01 where ccode = '" + rd.cCode + "' ";
            DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(rd.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
            string result = U8SqlDBHelper.GetString(sql);
            if (result == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[cWhCode]未传递！\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable2.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\"}";
            }
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<RDs01> items = rd.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].MoCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单号[MoCode]未传递！\"}";
                }
                if (string.IsNullOrEmpty(items[i].Irowno))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单行号[Irowno]未传递！\"}";
                }
                if (BasicDAL.ToInt(items[i].Irowno) == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单行号[" + items[i].Irowno + "]格式错误！\"}";
                }
                sql = "select iReceivedQTY,iQuantity,cbCloser,cInvCode,MODetailsID from OM_MODetails a left join OM_MOMain b on a.MOID=b.MOID  where b.cCode = '" + items[i].MoCode + "' and a.iVouchRowNo = '" + items[i].Irowno + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单[" + items[i].MoCode + "]行[" + items[i].Irowno + "]无数据！\"}";
                }
                string value = dataTable.Rows[0]["cbCloser"].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单[" + items[i].MoCode + "]行[" + items[i].Irowno + "]已关闭，不可继续入库！\"}";
                }
                string vouchIdStr = dataTable.Rows[0]["MODetailsID"].ToString();
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable3.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                }
                if (items[i].cInvCode != dataTable.Rows[0]["cInvCode"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]与订单不一致！\"}";
                }
                if (items[i].iQuantity == 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]不可为0！\"}";
                }
                if (items[0].iQuantity > 0m && items[i].iQuantity < 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]传递异常！\"}";
                }
                if (items[0].iQuantity < 0m && items[i].iQuantity > 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]传递异常！\"}";
                }
                decimal num = BasicDAL.ToDec(dataTable.Rows[0]["iReceivedQTY"].ToString());
                decimal num2 = BasicDAL.ToDec(dataTable.Rows[0]["iQuantity"].ToString());
                if (num + items[i].iQuantity > num2)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单[" + items[i].MoCode + "]行[" + items[i].Irowno + "]累计入库数量大于订单数量！\"}";
                }
                sql = " select min(case when iQuantity=0 then 1 else isnull(isendqty,0)/iQuantity end) from OM_MOMaterials where modetailsid='" + vouchIdStr + "' ";
                decimal num3 = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                if (num + items[i].iQuantity > Math.Ceiling(num2 * num3))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"委外订单[" + items[i].MoCode + "]行[" + items[i].Irowno + "]累计入库数量大于领用套数！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime madeDate = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"生产日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    if (string.IsNullOrEmpty(items[i].cPosCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"仓库[" + items[i].cInvCode + "]启用货位管理，货位[cPosCode]未传递！\"}";
                    }
                    sql = " select 1 from Position where cPosCode='" + items[i].cPosCode + "' and cWhCode='" + rd.cWhCode + "' and bPosEnd=1 ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"货位编码[" + items[i].cPosCode + "]无数据！\"}";
                    }
                }
                if (!(items[i].iQuantity < 0m))
                {
                    continue;
                }
                sql = " select iQuantity from CurrentStock where cWhCode='" + rd.cWhCode + "' and cInvCode = '" + items[i].cInvCode + "' ";
                if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                {
                    sql = sql + " and cBatch='" + items[i].cBatch + "' ";
                }
                decimal num4 = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                if (Math.Abs(items[i].iQuantity) > num4)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]批号[" + items[i].cBatch + "]仓库现存量不足！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    sql = " select iQuantity from InvPositionSum where cWhCode='" + rd.cWhCode + "' and cPosCode='" + items[i].cPosCode + "' and cInvCode='" + items[i].cInvCode + "' ";
                    if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                    {
                        sql = sql + " and cBatch='" + items[i].cBatch + "' ";
                    }
                    decimal num5 = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                    if (Math.Abs(items[i].iQuantity) > num5)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]货位[" + items[i].cPosCode + "]批号[" + items[i].cBatch + "]货位现存量不足！\"}";
                    }
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string detailId = "1" + $"{vouchId:D9}";
            string itemId = "cdepcode";
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                itemId = "'" + rd.cDepCode + "'";
            }
            string value1 = "cPersonCode";
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                value1 = "'" + rd.cPersonCode + "'";
            }
            sql = " insert into RdRecord01 (id,brdflag,cvouchtype,cbustype,csource,cwhcode,ddate,ccode,crdcode,cdepcode,  cptcode,cvencode,cPersonCode ,cordercode,cmaker,bpufirst,vt_id,bisstqc,itaxrate,iexchrate,  cexch_name,bomfirst,idiscounttaxtype,iswfcontrolled,dnmaketime,bredvouch,bcredit,iprintcount,cvenpuomprotocol,cHandler ,  dVeriDate ,dnverifytime,ipurorderid,ipurarriveid,cARVCode,cChkCode,cMemo,csysbarcode,iflowid,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select " + detailId + ",'1','01',cbustype,'委外订单','" + rd.cWhCode + "','" + rd.dDate + "','" + rd.cCode + "','103'," + itemId + ",  cptcode,cvencode," + value1 + ",ccode,'" + rd.cMaker + "','0','27','0',itaxrate, nflat,  cexch_name,'0','0','0',getdate(),'0','0','0',NULL,'" + rd.cVerifier + "',  '" + rd.dDate + "',getdate(),null, MOID , null ,NULL,'" + rd.cMemo + "','||st01|" + rd.cCode + "', NULL,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16   from OM_MOMain (nolock) where ccode = '" + items[0].MoCode + "' ";
            list.Add(sql);
            int num6 = 0;
            for (int j = 0; j < items.Count; j++)
            {
                num6++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string value2 = "1" + $"{vouchId2:D9}";
                sql = " select a.*,b.nflat,b.cvencode from OM_MODetails a left join OM_MOMain b on a.MOID=b.MOID  where b.cCode = '" + items[j].MoCode + "' and a.iVouchRowNo = '" + items[j].Irowno + "' ";
                DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                string value3 = dataTable4.Rows[0]["cvencode"].ToString();
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
                string posCodeValue = "null";
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    posCodeValue = "'" + items[j].cPosCode + "'";
                }
                string madeDateValue = "null";
                string vDateValue = "null";
                string expDateValue = "null";
                string massDateValue = "null";
                string massUnitValue = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    madeDateValue = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    vDateValue = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    expDateValue = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    massDateValue = BasicDAL.GetMassdate(cInvCode);
                    massUnitValue = BasicDAL.GetMassUnit(cInvCode);
                }
                decimal num7 = BasicDAL.ToDec(dataTable4.Rows[0]["nflat"].ToString());
                decimal num8 = BasicDAL.ToDec(dataTable4.Rows[0]["iUnitPrice"].ToString());
                decimal num9 = num8 * num7;
                decimal num10 = Math.Round(num9 * iQuantity, 5);
                sql = string.Concat(" insert into rdrecords01(autoid,id,cinvcode,inum,iquantity,iunitcost,iprice,iaprice,  isquantity , isnum, imoney, ifnum, ifquantity, iposid, facost, inquantity, innum, cassunit,  ioritaxcost,ioricost,iorimoney,ioritaxprice,iorisum,itaxrate,itaxprice,iProcessCost,iProcessFee,  isum,btaxcost,cpoid,bcosting,bvmiused,iinvexchrate,iexpiratdatecalcu,iordertype,isotype,  irowno ,cBatch ,iArrsId ,cbarvcode,cPosition,chVencode,iOMoDID,  cItemCode, cName ,cItem_class, cItemCName ,imassdate,cmassunit,  dMadeDate,dVDate,cExpirationdate,cCheckCode,iCheckIdBaks,cCheckPersonCode,dCheckDate,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )  select ", value2, ",", detailId, ",cinvcode,", iNum, ",", iQuantity, ", null,null,null,  0,0,0,null,null, null ,null ,", iQuantity, ",", iNum, ",", AssUnit, ",  null,null,null,null,null,null,null,", num9, ", ", num10, ",  null ,1,'", items[j].MoCode, "', 1,null,", iRate, ",0,0,0,  '", num6, "', ", batchValue, ", null , null,", posCodeValue, ",'", value3, "',MODetailsID,  null, null, null,null, ", massDateValue, ",", massUnitValue, ",  ", madeDateValue, ",", vDateValue, ",", expDateValue, ", null,null,null,null ,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37   from OM_MODetails where MODetailsID = '", dataTable4.Rows[0]["MODetailsID"], "' ");
                list.Add(sql);
                list.Add("insert into IA_ST_UnAccountVouch01(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + detailId + "','" + value2 + "','01','委外加工')");
                sql = string.Concat(" update OM_MODetails set iReceivedQTY =isnull(iReceivedQTY,0)+", iQuantity, " where MODetailsID = '", dataTable4.Rows[0]["MODetailsID"], "' ");
                list.Add(sql);
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    string batchClause = " and isnull(cbatch,'') = '' ";
                    if (BasicDAL.IsBatch(cInvCode, zt))
                    {
                        batchClause = " and cbatch = '" + items[j].cBatch + "' ";
                    }
                    sql = " insert into InvPosition (  RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,iNum,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,iExpiratDateCalcu,cvouchtype,dVouchDate,  dMadeDate,dVDate,cExpirationdate )  select autoid,id,'" + rd.cWhCode + "','" + items[j].cPosCode + "',cinvcode,cbatch,  " + iQuantity + "," + iNum + ",'','" + dateTime.ToString("yyyy-MM-dd") + "',0,0,  iMassDate,cMassUnit,iExpiratDateCalcu,'11','" + dateTime.ToString("yyyy-MM-dd") + "',  dMadeDate,dVDate,cExpirationdate  from rdrecords01 where ID = '" + detailId + "' and autoid = '" + value2 + "' ";
                    list.Add(sql);
                    sql = "  update invpositionsum set iquantity = iquantity + " + iQuantity + ",iNum=isnull(iNum,0)+isnull(" + iNum + ",0)   where cwhcode = '" + rd.cWhCode + "' and cposcode = '" + items[j].cPosCode + "' and cinvcode = '" + cInvCode + "' " + batchClause;
                    list.Add(sql);
                }
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','01','" + detailId + "' ";
            list.Add(sql);
            int num11 = U8SqlDBHelper.ExecuteSqlTran(list);
            if (num11 > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8委外入库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8委外入库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string iaCreateBillFlag = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + iaCreateBillFlag + "\",\"U8Code\":\"\"}";
        }
    }

    public static string RD32(RD32 rd)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord32 where ccode = '" + rd.cCode + "' ";
            DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(rd.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
            string result = U8SqlDBHelper.GetString(sql);
            if (result == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[cWhCode]未传递！\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable2.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\"}";
            }
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<RD32S> items = rd.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].iDLsID))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货单明细[iDLsID]未传递！\"}";
                }
                sql = " select fOutQuantity,iQuantity,cInvCode from dispatchlists a (nolock)  left join dispatchlist b (nolock) on a.DLID=b.DLID where a.iDLsID = '" + items[i].iDLsID + "' and b.bReturnFlag = 0 ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货单明细[" + items[i].iDLsID + "]无数据！\"}";
                }
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable3.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                }
                if (items[i].cInvCode != dataTable.Rows[0]["cInvCode"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]不属于发货单明细[" + items[i].iDLsID + "]\"}";
                }
                if (items[i].iQuantity <= 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库数量[iQuantity]必须大于0！\"}";
                }
                decimal num = BasicDAL.ToDec(dataTable.Rows[0]["fOutQuantity"].ToString());
                decimal num2 = BasicDAL.ToDec(dataTable.Rows[0]["iQuantity"].ToString());
                if (num + items[i].iQuantity > num2)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货单明细[" + items[i].iDLsID + "]累计出库数量大于发货单数量！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime dateTime2 = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    if (string.IsNullOrEmpty(items[i].cPosCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"仓库[" + items[i].cInvCode + "]启用货位管理，货位[cPosCode]未传递！\"}";
                    }
                    sql = " select 1 from Position where cPosCode='" + items[i].cPosCode + "' and cWhCode='" + rd.cWhCode + "' and bPosEnd=1 ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"货位编码[" + items[i].cPosCode + "]无数据！\"}";
                    }
                }
                sql = " select iQuantity from CurrentStock where cWhCode='" + rd.cWhCode + "' and cInvCode = '" + items[i].cInvCode + "' ";
                if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                {
                    sql = sql + " and cBatch='" + items[i].cBatch + "' ";
                }
                decimal num3 = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                if (items[i].iQuantity > num3)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]批号[" + items[i].cBatch + "]仓库现存量不足！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    sql = " select iQuantity from InvPositionSum where cWhCode='" + rd.cWhCode + "' and cPosCode='" + items[i].cPosCode + "' and cInvCode='" + items[i].cInvCode + "' ";
                    if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                    {
                        sql = sql + " and cBatch='" + items[i].cBatch + "' ";
                    }
                    decimal num4 = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                    if (items[i].iQuantity > num4)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]货位[" + items[i].cPosCode + "]批号[" + items[i].cBatch + "]货位现存量不足！\"}";
                    }
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string vouchIdStr = "1" + $"{vouchId:D9}";
            string detailId = U8SqlDBHelper.GetString("select DLID from DispatchLists where iDLsID = '" + items[0].iDLsID + "'");
            sql = "  insert rdrecord32  \r\n (id, brdflag, cvouchtype, cbustype, csource ,  \r\n cbuscode, cwhcode, ddate, ccode, crdcode ,  \r\n cdepcode, cpersoncode, cstcode, ccuscode, cdlcode ,cinvoicecompany,caddcode,  \r\n cmaker, vt_id, iswfcontrolled, dnmaketime,cShipAddress ,csysbarcode,  \r\n iprintcount, dVeriDate, cHandler, dnverifytime, cMemo , iflowid, \r\n cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,  \r\n cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  \r\n   select '" + vouchIdStr + "' ,0 ,'32',cbustype ,'发货单' , \r\n cDLCode ,'" + rd.cWhCode + "' ,'" + dateTime.ToString("yyyy-MM-dd") + "' ,'" + rd.cCode + "' ,'201' , \r\n cdepcode ,cpersoncode ,cstcode ,ccuscode ,DLID , cinvoicecompany,caddcode, \r\n '" + rd.cMaker + "' ,'87' ,0 ,getdate() ,cShipAddress ,'||st32|" + rd.cCode + "', \r\n 0 ,'" + dateTime.ToString("yyyy-MM-dd") + "','" + rd.cVerifier + "',getdate() , '" + rd.cMemo + "' , iflowid, \r\n cDefine1,cDefine2,'" + rd.cdefine3 + "',cDefine4,cDefine5,cDefine6,cDefine7, \r\n cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 \r\n from DispatchList  \r\n where DLID = '" + detailId + "' ";
            list.Add(sql);
            int num5 = 0;
            for (int j = 0; j < items.Count; j++)
            {
                num5++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string itemId = "1" + $"{vouchId2:D9}";
                string cInvCode = items[j].cInvCode;
                decimal iQuantity = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                string value1 = "null";
                if (BasicDAL.IsBatch(cInvCode, ""))
                {
                    value1 = "'" + items[j].cBatch + "'";
                }
                string value2 = "null";
                string value3 = "null";
                string batchValue = "null";
                string posCodeValue = "null";
                string madeDateValue = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    value2 = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    value3 = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    batchValue = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    posCodeValue = BasicDAL.GetMassdate(cInvCode);
                    madeDateValue = BasicDAL.GetMassUnit(cInvCode);
                }
                sql = " select b.cDLCode from DispatchLists a (nolock)  left join DispatchList b (nolock) on a.DLID=b.DLID where a.iDLsID = '" + items[j].iDLsID + "'";
                string vDateValue = U8SqlDBHelper.GetString(sql);
                sql = " select b.cbustype from DispatchLists a (nolock)  left join DispatchList b (nolock) on a.DLID=b.DLID where a.iDLsID = '" + items[j].iDLsID + "'";
                string expDateValue = U8SqlDBHelper.GetString(sql);
                sql = "select * from DispatchLists (nolock) where iDLsID='" + items[j].iDLsID + "'";
                DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                string massDateValue = dataTable4.Rows[0]["iSOsID"].ToString();
                string massUnitValue = dataTable4.Rows[0]["cWhCode"].ToString();
                string batchClause = dataTable4.Rows[0]["cBatch"].ToString();
                string iaCreateBillFlag = dataTable4.Rows[0]["bIAcreatebill"].ToString();
                sql = "  insert rdrecords32  \r\n (autoid, id, cinvcode, iNum, iquantity, iFlag, iDLsID ,  \r\n iNQuantity, bLPUseFree, iRSRowNO, iOriTrackID, bCosting ,  \r\n bVMIUsed, cbdlcode, ipesodid, ipesotype , cpesocode, ipesoseq, isotype, irowno , \r\n iorderseq,iordertype,iorderdid,csocode,iordercode,isodid,isoseq ,  \r\n cbsysbarcode, bIAcreatebill, bsaleoutcreatebill, bneedbill, iposflag, \r\n cbatch,cPosition,iinvexchrate,cAssUnit,dMadeDate,dVDate,cExpirationdate, \r\n imassdate,cmassunit,cItemCode,cName,cItem_class,cItemCName,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29, \r\n cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 ) \r\n   select '" + itemId + "','" + vouchIdStr + "','" + cInvCode + "', " + iNum + ", " + iQuantity + ", 0, iDLsID ,  " + iQuantity + ", 0, 0, 0, 1 , \r\n 0,'" + vDateValue + "', iSOsID, 1 , cSoCode , iorderrowno ,0, '" + num5 + "', \r\n iorderrowno ,1, iSOsID ,null, cSoCode ,null,null,  \r\n '||st32|" + rd.cCode + "|" + num5 + "',0, 0, 1, null, \r\n " + value1 + ",null ," + iRate + "," + AssUnit + "," + value2 + "," + value3 + "," + batchValue + ", \r\n " + posCodeValue + "," + madeDateValue + ",cItemCode,cItemName,cItem_class,cItem_CName, \r\n '" + items[j].cDefine22 + "','" + items[j].cDefine23 + "',cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29, \r\n cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 \r\n from DispatchLists  where iDLsID='" + items[j].iDLsID + "' ";
                list.Add(sql);
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    string batchClause2 = " and isnull(cbatch,'') = '' ";
                    if (BasicDAL.IsBatch(cInvCode, zt))
                    {
                        batchClause2 = " and cbatch = '" + items[j].cBatch + "' ";
                    }
                    sql = " insert into InvPosition (  RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,iNum,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,iExpiratDateCalcu,cvouchtype,dVouchDate,  dMadeDate,dVDate,cExpirationdate )  select autoid,id,'" + rd.cWhCode + "','" + items[j].cPosCode + "',cinvcode,cbatch,  " + iQuantity + "," + iNum + ",'','" + dateTime.ToString("yyyy-MM-dd") + "',0,0,  iMassDate,cMassUnit,iExpiratDateCalcu,'32','" + dateTime.ToString("yyyy-MM-dd") + "',  dMadeDate,dVDate,cExpirationdate  from rdrecords32 where ID = '" + vouchIdStr + "' and autoid = '" + itemId + "' ";
                    list.Add(sql);
                    sql = "  update invpositionsum set iquantity = iquantity - " + iQuantity + ",iNum=isnull(iNum,0)-isnull(" + iNum + ",0)   where cwhcode = '" + rd.cWhCode + "' and cposcode = '" + items[j].cPosCode + "' and cinvcode = '" + cInvCode + "' " + batchClause2;
                    list.Add(sql);
                }
                sql = " update DispatchList set cSaleOut = 'ST' where cDLCode = '" + vDateValue + "' ";
                list.Add(sql);
                sql = " update DispatchLists set fOutQuantity=isnull(fOutQuantity,0)+ " + iQuantity + ",  fOutNum=isnull(fOutNum,0)+isnull(" + iNum + ",0) where iDLsID='" + items[j].iDLsID + "' ";
                list.Add(sql);
                sql = " update so_sodetails set foutquantity=isnull(foutquantity,0)+" + iQuantity + ",  foutnum=isnull(foutnum,0)+isnull(" + iNum + ",0)  where isosid = '" + massDateValue + "' ";
                list.Add(sql);
                if (!string.IsNullOrEmpty(massUnitValue))
                {
                    sql = " update CurrentStock set fOutQuantity=fOutQuantity- " + iQuantity + ",fOutNum=fOutNum- isnull(" + iNum + ",0)  where cinvcode = '" + cInvCode + "' and cwhcode='" + massUnitValue + "' ";
                    sql = sql + (string.IsNullOrEmpty(batchClause) ? " and isnull(cBatch,'')= '' " : " and cBatch='" + batchClause + "' ");
                    list.Add(sql);
                }
                if (iaCreateBillFlag == "True")
                {
                    list.Add("insert into IA_ST_UnAccountVouch32(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + vouchIdStr + "','" + itemId + "','32','" + expDateValue + "')");
                }
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','32','" + vouchIdStr + "' ";
            list.Add(sql);
            int num6 = U8SqlDBHelper.ExecuteSqlTran(list);
            if (num6 > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8销售出库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8销售出库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string seedYearMonth0 = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + seedYearMonth0 + "\",\"U8Code\":\"\"}";
        }
    }

    public static string RD32Re(RD32 rd)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord32 where ccode = '" + rd.cCode + "' ";
            DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(rd.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
            string result = U8SqlDBHelper.GetString(sql);
            if (result == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[cWhCode]未传递！\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable2.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\"}";
            }
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<RD32S> items = rd.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].iDLsID))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"退货单明细[iDLsID]未传递！\"}";
                }
                sql = " select fOutQuantity,iQuantity,cInvCode from dispatchlists a (nolock)  left join dispatchlist b (nolock) on a.DLID=b.DLID where a.iDLsID = '" + items[i].iDLsID + "' and b.bReturnFlag = 1 ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"退货单明细[" + items[i].iDLsID + "]无数据！\"}";
                }
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable3.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                }
                if (items[i].cInvCode != dataTable.Rows[0]["cInvCode"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]不属于退货单明细[" + items[i].iDLsID + "]\"}";
                }
                if (items[i].iQuantity >= 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"退货数量[iQuantity]必须小于0！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime dateTime2 = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string vouchIdStr = "1" + $"{vouchId:D9}";
            string detailId = U8SqlDBHelper.GetString("select DLID from DispatchLists where iDLsID = '" + items[0].iDLsID + "'");
            sql = "  insert rdrecord32  \r\n (id, brdflag, cvouchtype, cbustype, csource ,  \r\n cbuscode, cwhcode, ddate, ccode, crdcode ,  \r\n cdepcode, cpersoncode, cstcode, ccuscode, cdlcode ,cinvoicecompany,caddcode,  \r\n cmaker, vt_id, iswfcontrolled, dnmaketime,cShipAddress ,csysbarcode,  \r\n iprintcount, dVeriDate, cHandler, dnverifytime, cMemo , iflowid, \r\n cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,  \r\n cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  \r\n   select '" + vouchIdStr + "' ,0 ,'32',cbustype ,'发货单' , \r\n cDLCode ,'" + rd.cWhCode + "' ,'" + dateTime.ToString("yyyy-MM-dd") + "' ,'" + rd.cCode + "' ,'0401' , \r\n cdepcode ,cpersoncode ,cstcode ,ccuscode ,DLID , cinvoicecompany,caddcode, \r\n '" + rd.cMaker + "' ,'87' ,0 ,getdate() ,cShipAddress ,'||st32|" + rd.cCode + "', \r\n 0 ,'" + dateTime.ToString("yyyy-MM-dd") + "','" + rd.cVerifier + "',getdate() , '" + rd.cMemo + "' , iflowid, \r\n cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7, \r\n cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 \r\n from DispatchList  \r\n where DLID = '" + detailId + "' ";
            list.Add(sql);
            int num = 0;
            for (int j = 0; j < items.Count; j++)
            {
                num++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string itemId = "1" + $"{vouchId2:D9}";
                string cInvCode = items[j].cInvCode;
                decimal iQuantity = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                string value1 = "null";
                if (BasicDAL.IsBatch(cInvCode, ""))
                {
                    value1 = "'" + items[j].cBatch + "'";
                }
                string value2 = "null";
                string value3 = "null";
                string batchValue = "null";
                string posCodeValue = "null";
                string madeDateValue = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    value2 = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    value3 = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    batchValue = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    posCodeValue = BasicDAL.GetMassdate(cInvCode);
                    madeDateValue = BasicDAL.GetMassUnit(cInvCode);
                }
                sql = " select b.cDLCode from DispatchLists a (nolock)  left join DispatchList b (nolock) on a.DLID=b.DLID where a.iDLsID = '" + items[j].iDLsID + "'";
                string vDateValue = U8SqlDBHelper.GetString(sql);
                sql = " select b.cbustype from DispatchLists a (nolock)  left join DispatchList b (nolock) on a.DLID=b.DLID where a.iDLsID = '" + items[j].iDLsID + "'";
                string expDateValue = U8SqlDBHelper.GetString(sql);
                sql = "select * from DispatchLists (nolock) where iDLsID='" + items[j].iDLsID + "'";
                DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                string massDateValue = dataTable4.Rows[0]["iSOsID"].ToString();
                string massUnitValue = dataTable4.Rows[0]["cWhCode"].ToString();
                string batchClause = dataTable4.Rows[0]["cBatch"].ToString();
                string iaCreateBillFlag = dataTable4.Rows[0]["bIAcreatebill"].ToString();
                sql = "  insert rdrecords32  \r\n (autoid, id, cinvcode, iNum, iquantity, iFlag, iDLsID ,  \r\n iNQuantity, bLPUseFree, iRSRowNO, iOriTrackID, bCosting ,  \r\n bVMIUsed, cbdlcode, ipesodid, ipesotype , cpesocode, ipesoseq, isotype, irowno , \r\n iorderseq,iordertype,iorderdid,csocode,iordercode,isodid,isoseq ,  \r\n cbsysbarcode, bIAcreatebill, bsaleoutcreatebill, bneedbill, iposflag, \r\n cbatch,cPosition,iinvexchrate,cAssUnit,dMadeDate,dVDate,cExpirationdate, \r\n imassdate,cmassunit,cItemCode,cName,cItem_class,cItemCName,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29, \r\n cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 ) \r\n   select '" + itemId + "','" + vouchIdStr + "','" + cInvCode + "', " + iNum + ", " + iQuantity + ", 0, iDLsID ,  " + iQuantity + ", 0, 0, 0, 1 , \r\n 0,'" + vDateValue + "', iSOsID, 1 , cSoCode , iorderrowno ,0, '" + num + "', \r\n iorderrowno ,1, iSOsID ,null, cSoCode ,null,null,  \r\n '||st32|" + rd.cCode + "|" + num + "',0, 0, 1, null, \r\n " + value1 + ",null ," + iRate + "," + AssUnit + "," + value2 + "," + value3 + "," + batchValue + ", \r\n " + posCodeValue + "," + madeDateValue + ",cItemCode,cItemName,cItem_class,cItem_CName, \r\n cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29, \r\n cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 \r\n from DispatchLists  where iDLsID='" + items[j].iDLsID + "' ";
                list.Add(sql);
                sql = " update DispatchList set cSaleOut = 'ST' where cDLCode = '" + vDateValue + "' ";
                list.Add(sql);
                sql = " update DispatchLists set fOutQuantity=isnull(fOutQuantity,0)+ " + iQuantity + ",  fOutNum=isnull(fOutNum,0)+isnull(" + iNum + ",0) where iDLsID='" + items[j].iDLsID + "' ";
                list.Add(sql);
                sql = " update so_sodetails set foutquantity=isnull(foutquantity,0)+" + iQuantity + ",  foutnum=isnull(foutnum,0)+isnull(" + iNum + ",0)  where isosid = '" + massDateValue + "' ";
                list.Add(sql);
                if (!string.IsNullOrEmpty(massUnitValue))
                {
                    sql = " update CurrentStock set fOutQuantity=fOutQuantity- " + iQuantity + ",fOutNum=fOutNum- isnull(" + iNum + ",0)  where cinvcode = '" + cInvCode + "' and cwhcode='" + massUnitValue + "' ";
                    sql = sql + (string.IsNullOrEmpty(batchClause) ? " and isnull(cBatch,'')= '' " : " and cBatch='" + batchClause + "' ");
                    list.Add(sql);
                }
                if (iaCreateBillFlag == "True")
                {
                    list.Add("insert into IA_ST_UnAccountVouch32(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + vouchIdStr + "','" + itemId + "','32','" + expDateValue + "')");
                }
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','32','" + vouchIdStr + "' ";
            list.Add(sql);
            int num2 = U8SqlDBHelper.ExecuteSqlTran(list);
            if (num2 > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8销售出库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8销售出库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string batchClause2 = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + batchClause2 + "\",\"U8Code\":\"\"}";
        }
    }

    public static string RD08(RD08 rd)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord08 where ccode = '" + rd.cCode + "' ";
            DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(rd.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
            string result = U8SqlDBHelper.GetString(sql);
            if (result == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库[cWhCode]未传递！\",\"U8Code\":\"\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable2.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\",\"U8Code\":\"\"}";
            }
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            if (rd.Items == null)
            {
                return "{\"Code\":\"400\",\"Msg\":\" 明细[Items]未传递！\",\"U8Code\":\"\"}";
            }
            List<RDs08> items = rd.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\",\"U8Code\":\"\"}";
                }
                sql = " select 1 from inventory (nolock) where cInvCode = '" + items[i].cInvCode + "' ";
                DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable3.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\" 存货编码[" + items[i].cInvCode + "]不存在！\",\"U8Code\":\"\"}";
                }
                decimal iQuantity = items[i].iQuantity;
                if (iQuantity <= 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\" 入库数量必须大于0！\",\"U8Code\":\"\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime dateTime2 = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string vouchIdStr = "1" + $"{vouchId:D9}";
            string detailId = "null";
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                detailId = "'" + rd.cDepCode + "'";
            }
            sql = " insert into rdrecord08(id,brdflag,cvouchtype,cbustype,csource, cwhcode,ddate,ccode,crdcode,cbuscode,cDepCode,  cmaker,bpufirst,biafirst,vt_id,bisstqc,bislsquery,iswfcontrolled,dnmaketime,dnmodifytime, cHandler,dVeriDate,dnverifytime,iprintcount,cMemo,csysbarcode)  values('" + vouchIdStr + "', N'1', N'08', N'其他入库', N'库存',  '" + rd.cWhCode + "', '" + dateTime.ToString("yyyy-MM-dd") + "', '" + rd.cCode + "','109',null, " + detailId + ",  '" + rd.cMaker + "', 0, 0, 67, 0, 0, 0, getdate(), Null, '" + rd.cVerifier + "','" + dateTime.ToString("yyyy-MM-dd") + "', getdate(), 0,'" + rd.cMemo + "','||st08|" + rd.cCode + "') ";
            list.Add(sql);
            int num = 0;
            for (int j = 0; j < items.Count; j++)
            {
                num++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string itemId = "1" + $"{vouchId2:D9}";
                string cInvCode = items[j].cInvCode;
                decimal iQuantity2 = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity2, out iNum, out iRate, out AssUnit);
                string value1 = "null";
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    value1 = "'" + items[j].cPosCode + "'";
                }
                string value2 = "null";
                if (BasicDAL.IsBatch(cInvCode, ""))
                {
                    value2 = "'" + items[j].cBatch + "'";
                }
                string value3 = "null";
                string batchValue = "null";
                string posCodeValue = "null";
                string madeDateValue = "null";
                string vDateValue = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    value3 = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    batchValue = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    posCodeValue = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    madeDateValue = BasicDAL.GetMassdate(cInvCode);
                    vDateValue = BasicDAL.GetMassUnit(cInvCode);
                }
                sql = " Insert Into rdrecords08(autoid,id,cinvcode,inum,iquantity,cbatch,  bcosting,iexpiratdatecalcu,isotype,irowno,iinvexchrate,cassunit,iTrIds,cbMemo,  cPosition,dMadeDate,dVDate,cExpirationdate,iMassDate,cMassUnit )  values('" + itemId + "', '" + vouchIdStr + "', '" + cInvCode + "', " + iNum + ", " + iQuantity2 + "," + value2 + ",   1, 0,  0, " + num + ", null, null,null,'',   " + value1 + "," + value3 + "," + batchValue + "," + posCodeValue + "," + madeDateValue + "," + vDateValue + " )";
                list.Add(sql);
                sql = " insert IA_ST_UnAccountVouch08(IDUN, IDSUN, cVouTypeUN, cBustypeUN)  values ('" + vouchIdStr + "','" + itemId + "','08','其他入库') ";
                list.Add(sql);
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','08','" + vouchIdStr + "'";
            list.Add(sql);
            int num2 = U8SqlDBHelper.ExecuteSqlTran(list);
            if (num2 > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8其他入库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8其他入库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string expDateValue = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + expDateValue + "\",\"U8Code\":\"\"}";
        }
    }

    public static string RD09(RD09 rd)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord09 where ccode = '" + rd.cCode + "' ";
            DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(rd.dDate);
            }              
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
            string result = U8SqlDBHelper.GetString(sql);
            if (result == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库[cWhCode]未传递！\",\"U8Code\":\"\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
            DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable2.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\",\"U8Code\":\"\"}";
            }
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            if (rd.Items == null)
            {
                return "{\"Code\":\"400\",\"Msg\":\" 明细[Items]未传递！\",\"U8Code\":\"\"}";
            }
            List<RDs09> items = rd.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\",\"U8Code\":\"\"}";
                }
                sql = " select 1 from inventory (nolock) where cInvCode = '" + items[i].cInvCode + "' ";
                DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable3.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\" 存货编码[" + items[i].cInvCode + "]不存在！\",\"U8Code\":\"\"}";
                }
                decimal iQuantity = items[i].iQuantity;
                if (iQuantity <= 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\" 入库数量必须大于0！\",\"U8Code\":\"\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime dateTime2 = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
                sql = " select iQuantity from currentstock (nolock) where cinvcode = '" + items[i].cInvCode + "' and cwhcode = '" + rd.cWhCode + "' ";
                if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                {
                    sql = sql + " and cbatch = '" + items[i].cBatch + "' ";
                }
                decimal num = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                if (num < items[i].iQuantity)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"仓库[" + rd.cWhCode + "]存货[" + items[i].cInvCode + "]批号[" + items[i].cBatch + "]现存量不足[" + items[i].iQuantity + "]！\",\"U8Code\":\"\"}";
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string vouchIdStr = "1" + $"{vouchId:D9}";
            string detailId = "null";
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                detailId = "'" + rd.cDepCode + "'";
            }
            sql = " insert into rdrecord09(id,brdflag,cvouchtype,cbustype,csource,  cwhcode,ddate,ccode,crdcode,chandler,cbuscode,cDepCode,  cmaker,bpufirst,biafirst,vt_id,bisstqc,ibg_overflag,cbg_auditor,cbg_audittime,controlresult,  iswfcontrolled,dnmaketime,dnmodifytime,dVeriDate,dnverifytime,iprintcount,cMemo)   values (" + vouchIdStr + ",N'0',N'09',N'其他出库',N'库存',  '" + rd.cWhCode + "','" + dateTime.ToString("yyyy-MM-dd") + "','" + rd.cCode + "','215','" + rd.cVerifier + "',null, " + detailId + ",  N'" + rd.cMaker + "',0,0,85,0,0,N'',N'',-1,  0,getdate(),Null,'" + dateTime.ToString("yyyy-MM-dd") + "', getdate(),0,'" + rd.cMemo + "') ";
            list.Add(sql);
            int num2 = 0;
            for (int j = 0; j < items.Count; j++)
            {
                num2++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string itemId = "1" + $"{vouchId2:D9}";
                string cInvCode = items[j].cInvCode;
                decimal iQuantity2 = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity2, out iNum, out iRate, out AssUnit);
                string value1 = "null";
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    value1 = "'" + items[j].cPosCode + "'";
                }
                string value2 = "null";
                if (BasicDAL.IsBatch(cInvCode, ""))
                {
                    value2 = "'" + items[j].cBatch + "'";
                }
                string value3 = "null";
                string batchValue = "null";
                string posCodeValue = "null";
                string madeDateValue = "null";
                string vDateValue = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    value3 = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    batchValue = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    posCodeValue = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    madeDateValue = BasicDAL.GetMassdate(cInvCode);
                    vDateValue = BasicDAL.GetMassUnit(cInvCode);
                }
                sql = " Insert Into rdrecords09(autoid, id, cinvcode, inum, iquantity, cbatch,   bcosting, isotype, irowno,iinvexchrate,cassunit,iTrIds,cbMemo,   cPosition,dMadeDate,dVDate,cExpirationdate,iMassDate,cMassUnit)  values(" + itemId + ", " + vouchIdStr + ", '" + cInvCode + "', " + iNum + ", " + iQuantity2 + "," + value2 + ",  1, 0, " + num2 + ", null, null,null,'',  " + value1 + "," + value3 + "," + batchValue + "," + posCodeValue + "," + madeDateValue + "," + vDateValue + " )";
                list.Add(sql);
                sql = " Insert Into Rdrecords09sub(autoid,id,cbg_itemcode,cbg_itemname,cbg_caliberkey1,cbg_caliberkeyname1,cbg_caliberkey2,cbg_caliberkeyname2,  cbg_caliberkey3,cbg_caliberkeyname3,cbg_calibercode1,cbg_calibername1,cbg_calibercode2,cbg_calibername2,cbg_calibercode3,cbg_calibername3,  ibg_ctrl,cbg_auditopinion,ibgstsum,ibgiasum,cbg_caliberkey4,cbg_caliberkeyname4,cbg_caliberkey5,cbg_caliberkeyname5,cbg_caliberkey6,  cbg_caliberkeyname6,cbg_calibercode4,cbg_calibername4,cbg_calibercode5,cbg_calibername5,cbg_calibercode6,cbg_calibername6)  values(" + itemId + ", " + vouchIdStr + ", Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, 0, Null, 0, Null, Null, Null, Null,  Null, Null, Null, Null, Null, Null, Null, Null, Null)  ";
                list.Add(sql);
                list.Add("insert into IA_ST_UnAccountVouch09(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + vouchIdStr + "','" + itemId + "','09','其他出库')");
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','09','" + vouchIdStr + "'";
            list.Add(sql);
            int num3 = U8SqlDBHelper.ExecuteSqlTran(list);
            if (num3 > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8其他出库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8其他出库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string expDateValue = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + expDateValue + "\",\"U8Code\":\"\"}";
        }
    }

    public static string RD10(RD10 rd)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(rd.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(rd.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from rdrecord10 where ccode = '" + rd.cCode + "' ";
            DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + rd.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(rd.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(rd.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + rd.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
            string result = U8SqlDBHelper.GetString(sql);
            if (result == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + rd.dDate + "]库存已关账！\"}";
            }
            if (string.IsNullOrEmpty(rd.cWhCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[cWhCode]未传递！\"}";
            }
            sql = " select * from Warehouse (nolock) where cwhcode='" + rd.cWhCode + "'  ";
            DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable2.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + rd.cWhCode + "]无数据或已停用！\"}";
            }
            if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + rd.cDepCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + rd.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(rd.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + rd.cPersonCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + rd.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<RD10S> items = rd.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].MoCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[MoCode]未传递！\"}";
                }
                sql = " select moid from mom_order where MoCode = '" + items[i].MoCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + items[i].MoCode + "]无数据！\"}";
                }
                string vouchIdStr = dataTable.Rows[0]["moid"].ToString();
                sql = "select CloseUser,InvCode from mom_orderdetail where moid = '" + vouchIdStr + "'";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                string value = dataTable.Rows[0]["CloseUser"].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + items[i].MoCode + "]已关闭，不可继续入库！\"}";
                }
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                sql = " select * from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable3.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]无数据或已停用！\"}";
                }
                if (items[i].cInvCode != dataTable.Rows[0]["InvCode"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]不属于工单[" + items[i].MoCode + "]\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime dateTime2 = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"生产日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(items[i].cBatch))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                }
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    if (string.IsNullOrEmpty(items[i].cPosCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"仓库[" + rd.cWhCode + "]启用货位管理，货位[cPosCode]未传递！\"}";
                    }
                    sql = " select 1 from Position where cPosCode='" + items[i].cPosCode + "' and cWhCode='" + rd.cWhCode + "' and bPosEnd=1 ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"货位编码[" + items[i].cPosCode + "]无数据！\"}";
                    }
                }
                if (items[i].iQuantity <= 0m)
                {
                    sql = " select iQuantity from currentstock where cwhcode='" + rd.cWhCode + "' and cinvcode='" + items[i].cInvCode + "' ";
                    if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                    {
                        sql = sql + " and cbatch = '" + items[i].cBatch + "' ";
                    }
                    decimal num = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                    if (Math.Abs(items[i].iQuantity) > num)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]仓库[" + rd.cWhCode + "]现存量不足！\"}";
                    }
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "rd");
            string detailId = "1" + $"{vouchId:D9}";
            sql = " select a.*,b.MoLotCode,b.MDeptCode from mom_order a left join mom_orderdetail b on a.moid=b.moid where a.mocode='" + items[0].MoCode + "' ";
            DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
            string itemId = "null";
            if (!string.IsNullOrEmpty(dataTable4.Rows[0]["MDeptCode"].ToString()))
            {
                itemId = string.Concat("'", dataTable4.Rows[0]["MDeptCode"], "'");
            }
            else if (!string.IsNullOrEmpty(rd.cDepCode))
            {
                itemId = "'" + rd.cDepCode + "'";
            }
            string value1 = string.Concat("'", dataTable4.Rows[0]["MoCode"], "'");
            string value2 = string.Concat("'", dataTable4.Rows[0]["MoID"], "'");
            string value3 = string.Concat("'", dataTable4.Rows[0]["MoLotCode"], "'");
            sql = " insert into RDRECORD10(id,brdflag,cvouchtype,cbustype,csource,cwhcode,ddate,ccode,crdcode,chandler,  cmemo,btransflag,cmaker,dveridate,bpufirst,biafirst,vt_id,bisstqc,cmpocode,iproorderid,cprobatch,  bfrompreyear,biscomplement,iDiscountTaxType,ireturncount,iverifystate,iswfcontrolled,dnmaketime,dnverifytime,bredvouch,iPrintCount,cDepCode,csysbarcode,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select  '" + detailId + "',1,'10','成品入库','生产订单','" + rd.cWhCode + "','" + dateTime.ToString("yyyy-MM-dd") + "','" + rd.cCode + "','102','" + rd.cVerifier + "',  '" + rd.cMemo + "',0,'" + rd.cMaker + "','" + dateTime.ToString("yyyy-MM-dd") + "',0,0,'63',0," + value1 + "," + value2 + "," + value3 + ", 0,0,0,0,0,0,getdate(),getdate(),0,0," + itemId + ",'||st10|" + rd.cCode + "' ,  Define1,Define2,Define3,Define4,Define5,Define6,Define7,Define8,  Define9,Define10,Define11,Define12,Define13,Define14,Define15,Define16   from mom_order where MoCode = '" + items[0].MoCode + "' ";
            list.Add(sql);
            int num2 = 0;
            for (int j = 0; j < items.Count; j++)
            {
                num2++;
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "rd");
                string batchValue = "1" + $"{vouchId2:D9}";
                string posCodeValue = U8SqlDBHelper.GetString("select moid from mom_order where mocode = '" + items[j].MoCode + "'");
                sql = " select * from mom_orderdetail where moid = '" + posCodeValue + "' ";
                DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
                string cInvCode = items[j].cInvCode;
                decimal iQuantity = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                string madeDateValue = "null";
                if (BasicDAL.IsBatch(cInvCode, zt) && !string.IsNullOrEmpty(items[j].cBatch))
                {
                    madeDateValue = "'" + items[j].cBatch + "'";
                }
                string vDateValue = "null";
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    vDateValue = "'" + items[j].cPosCode + "'";
                }
                string expDateValue = "null";
                string massDateValue = "null";
                string massUnitValue = "null";
                string batchClause = "null";
                string iaCreateBillFlag = "null";
                if (BasicDAL.IsInvQuality(cInvCode))
                {
                    expDateValue = "'" + items[j].dMadeDate + "'";
                    DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                    massDateValue = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                    massUnitValue = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                    batchClause = BasicDAL.GetMassdate(cInvCode);
                    iaCreateBillFlag = BasicDAL.GetMassUnit(cInvCode);
                }
                decimal num3 = BasicDAL.ToDec(dataTable5.Rows[0]["Qty"].ToString());
                string batchClause2 = dataTable5.Rows[0]["modid"].ToString();
                sql = "insert into rdrecords10(autoid,id,cinvcode,iquantity,iNum,cbatch,iflag,  iNQuantity,impoids,brelated,blpusefree,irsrowno,ioritrackid,bcosting,iinvsncount,  cmocode,imoseq,iorderdid,iordertype,isotype,irowno,cbMemo,cPosition,  iinvexchrate,cAssUnit,dMadeDate,dVDate,cExpirationdate,  imassdate,cmassunit,cItemCode, cName ,cItem_class, cItemCName,cMoLotCode ,  cCheckPersonCode,dCheckDate,cCheckCode,iCheckIdBaks,cbsysbarcode ,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 )  select  " + batchValue + ",'" + detailId + "','" + cInvCode + "'," + iQuantity + "," + iNum + "," + madeDateValue + ",0,  " + num3 + ",MoDId ,0,0,0,0, 1, 0,  '" + items[j].MoCode + "', SortSeq ,0,0,0," + num2 + ", '' ," + vDateValue + ", " + iRate + "," + AssUnit + ", " + expDateValue + "," + massDateValue + "," + massUnitValue + ",  " + batchClause + "," + iaCreateBillFlag + ", CostItemCode, CostItemName, null, null, MoLotCode,  null , null, null, null,'||st10|" + rd.cCode + "|" + num2 + "',  '" + items[j].cDefine22 + "','" + items[j].cDefine23 + "',Define24,Define25,Define26,Define27,Define28,Define29,  Define30,Define31,Define32,Define33,Define34,Define35,Define36,Define37   from mom_orderdetail where MoDId = '" + batchClause2 + "'  ";
                list.Add(sql);
                sql = " update mom_orderdetail set QualifiedInQty=isnull(QualifiedInQty,0) + " + iQuantity + "  where MoDId = '" + batchClause2 + "' ";
                list.Add(sql);
                list.Add("insert into IA_ST_UnAccountVouch10(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + detailId + "','" + batchValue + "','10','成品入库')");
                if (BasicDAL.bWhPos(rd.cWhCode))
                {
                    string seedYearMonth0 = " and isnull(cbatch,'') = '' ";
                    if (BasicDAL.IsBatch(items[j].cInvCode, ""))
                    {
                        seedYearMonth0 = " and isnull(cbatch,'') = " + madeDateValue + " ";
                    }
                    sql = " insert into InvPosition (  RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,iNum,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,iExpiratDateCalcu,cvouchtype,dVouchDate,  dMadeDate,dVDate,cExpirationdate )  select autoid,id,'" + rd.cWhCode + "'," + vDateValue + ",cinvcode,cbatch,   " + iQuantity + "," + iNum + ",'','" + dateTime.ToString("yyyy-MM-dd") + "',1,0,  iMassDate,cMassUnit,iExpiratDateCalcu,'10','" + dateTime.ToString("yyyy-MM-dd") + "',  dMadeDate,dVDate,cExpirationdate  from rdrecords10 where ID = '" + detailId + "' and autoid = '" + batchValue + "' ";
                    list.Add(sql);
                    sql = " select 1 from invpositionsum (nolock)  where cwhcode = '" + rd.cWhCode + "' and cposcode = " + vDateValue + " and cinvcode = '" + cInvCode + "' " + seedYearMonth0;
                    DataTable dataTable6 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable6.Rows.Count == 0)
                    {
                        sql = "  insert into invpositionsum (cWhCode,cPosCode,cInvCode,iQuantity,cBatch,  iTrackid,dMadeDate,dVDate,cExpirationdate,cMassUnit,iMassDate)  values ( '" + rd.cWhCode + "'," + vDateValue + ", '" + cInvCode + "', 0 ," + madeDateValue + ",  0," + expDateValue + "," + massDateValue + "," + massUnitValue + "," + iaCreateBillFlag + "," + batchClause + " ) ";
                        U8SqlDBHelper.ExecuteSql(sql);
                    }
                    sql = "  update invpositionsum set iquantity = iquantity + " + iQuantity + " ,iNum=isnull(iNum,0)+isnull(" + iNum + ",0)   where cwhcode = '" + rd.cWhCode + "' and cposcode = " + vDateValue + " and cinvcode = '" + cInvCode + "' " + seedYearMonth0;
                    list.Add(sql);
                }
            }
            sql = "exec pro_uptcurrentStock '" + zt + "','10','" + detailId + "'";
            list.Add(sql);
            int num4 = U8SqlDBHelper.ExecuteSqlTran(list);
            if (num4 > 0)
            {
                for (int k = 0; k < items.Count; k++)
                {
                    string seedYearMonth1 = U8SqlDBHelper.GetString("select moid from mom_order where mocode = '" + items[k].MoCode + "'");
                    sql = " update mom_orderdetail set CloseUser='" + rd.cVerifier + "',CloseTime=getdate(),CloseDate='" + dateTime.ToString("yyyy-MM-dd") + "',Status=4  where moid = '" + seedYearMonth1 + "' and isnull(QualifiedInQty,0) >= Qty ";
                    U8SqlDBHelper.ExecuteSql(sql);
                }
                return "{\"Code\":\"200\",\"Msg\":\"U8生产入库单[" + rd.cCode + "]创建成功！\",\"U8Code\":\"" + rd.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8生产入库单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string seedYearMonth2 = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + seedYearMonth2 + "\",\"U8Code\":\"\"}";
        }
    }

    public static string Dispatch(Dispatch d)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(d.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(d.cVerifier))
            {
                return "{\"Code\":\"400\",\"Msg\":\"审核人[cVerifier]未传递！\"}";
            }
            if (string.IsNullOrEmpty(d.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sql = " select 1 from dispatchlist where cdlcode = '" + d.cCode + "' ";
            DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + d.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(d.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(d.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + d.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
            string result = U8SqlDBHelper.GetString(sql);
            if (result == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + d.dDate + "]库存已关账！\"}";
            }
            if (!string.IsNullOrEmpty(d.cDepCode))
            {
                sql = " select * from Department where bDepEnd = '1' and cDepCode = '" + d.cDepCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + d.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(d.cPersonCode))
            {
                sql = " select * from person where cPersonCode = '" + d.cPersonCode + "' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + d.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<Dispatchs> items = d.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].cSoCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"订单号[cSoCode]未传递！\"}";
                }
                if (string.IsNullOrEmpty(items[i].Irowno))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"订单行号[Irowno]未传递！\"}";
                }
                if (string.IsNullOrEmpty(items[i].RowNo))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货单行号[RowNo]未传递！\"}";
                }
                if (BasicDAL.ToInt(items[i].RowNo) == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货单行号[" + items[i].RowNo + "]不正确！\"}";
                }
                sql = " select iSOsID,cSoCode,iRowNo,cInvCode,iFHQuantity,iQuantity from SO_SODetails where cSoCode = '" + items[i].cSoCode + "' and iRowNo = '" + items[i].Irowno + "' and isnull(cSCloser,'')='' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"订单[" + items[i].cSoCode + "]行[" + items[i].Irowno + "]无数据！\"}";
                }
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                if (items[i].cInvCode != dataTable.Rows[0]["cInvCode"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]不属于工单[" + items[i].cSoCode + "]\"}";
                }
                decimal shippedQty = BasicDAL.ToDec(dataTable.Rows[0]["iFHQuantity"].ToString());
                decimal orderQty = BasicDAL.ToDec(dataTable.Rows[0]["iQuantity"].ToString());
                if (shippedQty + items[i].iQuantity > orderQty)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"订单[" + items[i].cSoCode + "]行[" + items[i].Irowno + "]累计发货数量大于订单数量！\"}";
                }
                if (string.IsNullOrEmpty(items[i].cWhCode))
                {
                    continue;
                }
                sql = " select * from Warehouse (nolock) where cwhcode='" + items[i].cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
                DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable2.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + items[i].cWhCode + "]无数据或已停用！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime dateTime2 = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"生产日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                string vouchIdStr = "";
                if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                {
                    if (string.IsNullOrEmpty(items[i].cBatch))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                    }
                    vouchIdStr = "and cbatch = '" + items[i].cBatch + "'";
                }
                sql = "select iQuantity+isnull(fInQuantity,0)-isnull(fOutQuantity,0) from CurrentStock where cWhCode = '" + items[i].cWhCode + "' and cInvCode ='" + items[i].cInvCode + "' " + vouchIdStr;
                decimal availableQty = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                if (items[i].iQuantity > availableQty)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]可用量不足！\"}";
                }
            }
            int vouchId = BasicDAL.GetVouchId("F", cAcc_Id, "DISPATCH");
            string detailId = "1" + $"{vouchId:D9}";
            string itemId = "cDepCode";
            if (!string.IsNullOrEmpty(d.cDepCode))
            {
                itemId = "'" + d.cDepCode + "'";
            }
            string value1 = "cPersonCode";
            if (!string.IsNullOrEmpty(d.cPersonCode))
            {
                value1 = "'" + d.cPersonCode + "'";
            }
            string value2 = "1";
            if (d.bsaleoutcreatebill == "0")
            {
                value2 = "0";
            }
            sql = " insert into dispatchlist  (DLID, cDLCode, cVouchType, cSTCode, dDate, cRdCode, cDepCode, cPersonCode, SBVID, cCusCode, cexch_name, iExchRate, iTaxRate,  bFirst, bReturnFlag, bSettleAll, cMemo, cVerifier, cMaker, iSale, cCusName, iVTid, cBusType,  bIAFirst, bCredit, iverifystate, iswfcontrolled, bARFirst, bsaleoutcreatebill,cShipAddress,caddcode ,  dverifydate, dcreatesystime, dverifysystime, iflowid, bsigncreate, bcashsale, bmustbook, bneedbill, baccswitchflag, cSaleOut ,  csocode,cinvoicecompany,csysbarcode,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,   cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  \r\n select   '" + detailId + "','" + d.cCode + "','05',cSTCode,'" + dateTime.ToString("yyyy-MM-dd") + "',null," + itemId + " ," + value1 + ",0,cCusCode,cexch_name,iExchRate, iTaxRate,  0,0,0, '" + d.cMemo + "','" + d.cVerifier + "','" + d.cMaker + "', 0,cCusName,'71',cBusType ,  0,0,0,0,0, " + value2 + ", cCusOAddress, caddcode,  '" + dateTime.ToString("yyyy-MM-dd") + "' ,getdate(),  getdate() ,iflowid, 0,0,0,1,0,NULL ,   csocode,cinvoicecompany, '||SA01|" + d.cCode + "',  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,  cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,'" + d.cDefine13 + "',cDefine14,cDefine15,cDefine16  from SO_SOMain where csocode = '" + items[0].cSoCode + "' ";
            list.Add(sql);
            List<string> fEntryList = new List<string>();
            for (int j = 0; j < items.Count; j++)
            {
                int dispatchRowNo = BasicDAL.ToInt(items[j].RowNo);
                sql = " select * from so_sodetails where cSoCode = '" + items[j].cSoCode + "' and iRowNo = '" + items[j].Irowno + "' ";
                DataTable detailTable = U8SqlDBHelper.GetDataTable(sql);
                string cInvCode = items[j].cInvCode;
                decimal iQuantity = items[j].iQuantity;
                string iNum = "";
                string iRate;
                string AssUnit;
                decimal assQty = BasicDAL.GetAssQty(zt, cInvCode, iQuantity, out iNum, out iRate, out AssUnit);
                string value3 = "null";
                string batchValue = "null";
                string posCodeValue = "null";
                string madeDateValue = "null";
                string vDateValue = "null";
                string expDateValue = "null";
                string massDateValue = "null";
                if (!string.IsNullOrEmpty(items[j].cWhCode))
                {
                    value3 = "'" + items[j].cWhCode + "'";
                    string massUnitValue = "";
                    if (BasicDAL.IsBatch(items[j].cInvCode, ""))
                    {
                        batchValue = "'" + items[j].cBatch + "'";
                        massUnitValue = "and cbatch = '" + items[j].cBatch + "'";
                    }
                    if (BasicDAL.IsInvQuality(cInvCode))
                    {
                        posCodeValue = "'" + items[j].dMadeDate + "'";
                        DateTime vDate = BasicDAL.GetVDate(cInvCode, items[j].dMadeDate);
                        madeDateValue = "'" + vDate.ToString("yyyy-MM-dd") + "'";
                        vDateValue = "'" + vDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                        expDateValue = BasicDAL.GetMassdate(cInvCode);
                        massDateValue = BasicDAL.GetMassUnit(cInvCode);
                    }
                    sql = "update CurrentStock set fOutQuantity=isnull(fOutQuantity,0)+" + iQuantity + ",fOutNum=isnull(fOutNum,0)+" + iNum + "  where cWhCode = '" + items[j].cWhCode + "' and cInvCode ='" + items[j].cInvCode + "' " + massUnitValue;
                    list.Add(sql);
                }
                string batchClause = "1";
                if (items[j].bIAcreatebill == "0")
                {
                    batchClause = "0";
                }
                int iTB = 0;
                if (items[j].iTB == 1)
                {
                    iTB = 1;
                }

                decimal exchRate = BasicDAL.ToDec(U8SqlDBHelper.GetString("select iExchRate from so_somain where csocode='" + items[j].cSoCode + "'"));
                decimal unitPrice = BasicDAL.ToDec(detailTable.Rows[0]["iUnitPrice"].ToString());
                decimal taxUnitPrice = BasicDAL.ToDec(detailTable.Rows[0]["iTaxUnitPrice"].ToString());
                decimal taxRate = BasicDAL.ToDec(detailTable.Rows[0]["iTaxRate"].ToString());
                decimal taxAmount = Math.Round(taxUnitPrice * iQuantity, 2);
                decimal untaxedAmount = Math.Round(taxAmount / (100m + taxRate) * 100m, 2);
                decimal taxValue = taxAmount - untaxedAmount;
                decimal natUntaxedAmount = Math.Round(untaxedAmount * exchRate, 2);
                decimal natTaxValue = Math.Round(taxValue * exchRate, 2);
                decimal natTaxAmount = Math.Round(taxAmount * exchRate, 2);
                int vouchId2 = BasicDAL.GetVouchId("C", cAcc_Id, "DISPATCH");
                string detailItemId = "1" + $"{vouchId2:D9}";
                sql = "insert into dispatchlists  (DLID, irowno, iDLsID,bsaleprice,  cInvCode, iQuantity, iNum, iUnitPrice, iTaxUnitPrice,  iMoney, iTax, iSum, iNatUnitPrice, iNatMoney, iNatTax, iNatSum, iSettleNum,  cBatch, bSettleAll, iTB, cInvName, iTaxRate, fOutQuantity, fOutNum, bIsSTQc,  cUnitID, bGsp, bQANeedCheck, bQAUrgency, bQAChecking, bQAChecked, KL, KL2,   cordercode,iorderrowno,iSOsID,cSoCode, cItemCode,cItemName,cItem_class,cItem_CName,  dMDate,dvDate,cExpirationdate, iMassDate, cMassUnit, bcosting, bIAcreatebill, bneedsign, cWhCode ,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 ) \r\n select   '" + detailId + "'," + dispatchRowNo + ",'" + detailItemId + "',1,  '" + cInvCode + "', " + iQuantity + "," + iNum + ", " + unitPrice + "," + taxUnitPrice + ",  " + untaxedAmount + ", " + taxValue + ", " + taxAmount + ",iNatUnitPrice , " + natUntaxedAmount + ", " + natTaxValue + "," + natTaxAmount + ", 0 ,  " + batchValue + ",0, " + iTB + ",  cInvName , iTaxRate , 0 ," + iNum + ",0,  cUnitID ,0,0,0,0,0, KL, KL2,  cSoCode , iRowNo, iSOsID, cSoCode, cItemCode,cItemName,cItem_class,cItem_CName,  " + posCodeValue + "," + madeDateValue + "," + vDateValue + "," + expDateValue + "," + massDateValue + ", 1," + batchClause + ",0, " + value3 + ",  '" + items[j].cDefine22 + "','" + items[j].cDefine23 + "',cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 \r\n from so_sodetails  where cSoCode = '" + items[j].cSoCode + "' and iRowNo = '" + items[j].Irowno + "' ";
                list.Add(sql);
                fEntryList.Add("{\"FEntryID\":\"" + detailItemId + "\",\"Fseq\":" + dispatchRowNo + "}");
                sql = " update so_sodetails set iFHQuantity=isnull(iFHQuantity,0)+ " + iQuantity + ",   iFHNum=isnull(iFHNum,0)+isnull(" + iNum + ",0) , iFHMoney=isnull(iFHMoney,0)+ " + untaxedAmount + "  where cSoCode = '" + items[j].cSoCode + "' and iRowNo = '" + items[j].Irowno + "' ";
                list.Add(sql);
                string busType = U8SqlDBHelper.GetString(" select cBusType from SO_SOMain where cSOCode = '" + items[j].cSoCode + "'");
                sql = " insert into IA_SA_UnAccountVouch (IDUN,IDSUN,cVouTypeUN,cBustypeUN)  values ('" + detailId + "','" + detailItemId + "','05','" + busType + "' ) ";
                list.Add(sql);
            }
            int executeResult = U8SqlDBHelper.ExecuteSqlTran(list);
            if (executeResult > 0)
            {
                string fEntry = "[" + string.Join(",", fEntryList) + "]";
                return "{\"Code\":\"200\",\"Msg\":\"U8发货单[" + d.cCode + "]创建成功！\",\"U8Code\":\"" + d.cCode + "\",\"data\":{\"Number\":\"" + d.cCode + "\",\"Id\":\"" + detailId + "\",\"FEntry\":" + fEntry + "} }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8发货单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string seedYearMonth0 = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + seedYearMonth0 + "\",\"U8Code\":\"\"}";
        }
    }

    public static string DispatchUpdate(Dispatch d)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            if (string.IsNullOrEmpty(d.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            if (string.IsNullOrEmpty(d.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            sql = " select * from dispatchlist (nolock) where cdlcode = '" + d.cCode + "' and bReturnFlag = 0 ";
            DataTable dtHeader = U8SqlDBHelper.GetDataTable(sql);
            if (dtHeader.Rows.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"发货单[" + d.cCode + "]不存在！\"}";
            }
            string DLID = dtHeader.Rows[0]["DLID"].ToString();
            decimal origExchRate = BasicDAL.ToDec(dtHeader.Rows[0]["iExchRate"].ToString());
            string cSaleOut = dtHeader.Rows[0]["cSaleOut"].ToString();
            if (!string.IsNullOrEmpty(d.dDate))
            {
                DateTime dateTime;
                try
                {
                    dateTime = Convert.ToDateTime(d.dDate);
                }
                catch
                {
                    return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + d.dDate + "]格式错误！\",\"U8Code\":\"\"}";
                }
                string seedYearMonth = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
                sql = " select bflag_ST from GL_mend where iYPeriod = '" + seedYearMonth + "' ";
                string result = U8SqlDBHelper.GetString(sql);
                if (result == "True")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"日期[" + d.dDate + "]库存已关账！\"}";
                }
            }
            if (!string.IsNullOrEmpty(d.cDepCode))
            {
                sql = " select 1 from Department where bDepEnd = '1' and cDepCode = '" + d.cDepCode + "' ";
                DataTable dtDep = U8SqlDBHelper.GetDataTable(sql);
                if (dtDep.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + d.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(d.cPersonCode))
            {
                sql = " select 1 from person where cPersonCode = '" + d.cPersonCode + "' ";
                DataTable dtPer = U8SqlDBHelper.GetDataTable(sql);
                if (dtPer.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + d.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<Dispatchs> items = d.Items;
            if (items == null || items.Count == 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据明细[Items]未传递！\"}";
            }
            List<string> fEntryListUpdate = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：存货编码[cInvCode]未传递！\"}";
                }
                if (string.IsNullOrEmpty(items[i].iDLsID))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：发货单明细ID[iDLsID]未传递！\"}";
                }
                if (items[i].iQuantity <= 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：发货数量[iQuantity]必须大于0！\"}";
                }
                sql = " select a.iDLsID,a.iQuantity,a.fOutQuantity,a.iNum,a.cInvCode,a.iSOsID,a.irowno,a.cSoCode,a.cWhCode,a.cBatch,a.bIAcreatebill,a.iMoney,a.iTax,a.iSum,a.iNatMoney,a.iNatTax,a.iNatSum " +
                        " from DispatchLists a (nolock) left join DispatchList b (nolock) on a.DLID=b.DLID " +
                        " where a.iDLsID = '" + items[i].iDLsID + "' and b.DLID = '" + DLID + "' and b.bReturnFlag = 0 ";
                DataTable dtDetail = U8SqlDBHelper.GetDataTable(sql);
                if (dtDetail.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：发货单明细ID[" + items[i].iDLsID + "]不存在或不属于发货单[" + d.cCode + "]！\"}";
                }
                string origInvCode = dtDetail.Rows[0]["cInvCode"].ToString();
                if (items[i].cInvCode != origInvCode)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：物料编码不可变更(原:" + origInvCode + "，新:" + items[i].cInvCode + ")！\"}";
                }
                decimal origQty = BasicDAL.ToDec(dtDetail.Rows[0]["iQuantity"].ToString());
                decimal fOutQty = BasicDAL.ToDec(dtDetail.Rows[0]["fOutQuantity"].ToString());
                if (items[i].iQuantity < fOutQty)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：发货数量[" + items[i].iQuantity + "]不得小于已出库数量[" + fOutQty + "]！\"}";
                }
                string iSOsID = dtDetail.Rows[0]["iSOsID"].ToString();
                string cSoCode = dtDetail.Rows[0]["cSoCode"].ToString();
                string origIrowno = dtDetail.Rows[0]["irowno"].ToString();
                fEntryListUpdate.Add("{\"FEntryID\":\"" + items[i].iDLsID + "\",\"Fseq\":" + origIrowno + "}");
                decimal qtyDiff = items[i].iQuantity - origQty;
                if (qtyDiff != 0m)
                {
                    sql = " select iFHQuantity,iQuantity,iUnitPrice,iTaxUnitPrice,iTaxRate from SO_SODetails where iSOsID = '" + iSOsID + "' ";
                    DataTable dtSO = U8SqlDBHelper.GetDataTable(sql);
                    if (dtSO.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：关联销售订单明细不存在！\"}";
                    }
                    decimal sIQuantity = BasicDAL.ToDec(dtSO.Rows[0]["iQuantity"].ToString());
                    decimal sIFHQuantity = BasicDAL.ToDec(dtSO.Rows[0]["iFHQuantity"].ToString());
                    if (sIFHQuantity + qtyDiff > sIQuantity)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：调整后累计发货数量[" + (sIFHQuantity + qtyDiff) + "]大于订单数量[" + sIQuantity + "]！\"}";
                    }
                    if (sIFHQuantity + qtyDiff < 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：调整后累计发货数量不能小于0！\"}";
                    }
                    decimal sUnitPrice = BasicDAL.ToDec(dtSO.Rows[0]["iUnitPrice"].ToString());
                    decimal sTaxUnitPrice = BasicDAL.ToDec(dtSO.Rows[0]["iTaxUnitPrice"].ToString());
                    decimal sTaxRate = BasicDAL.ToDec(dtSO.Rows[0]["iTaxRate"].ToString());
                    decimal newSum = Math.Round(sTaxUnitPrice * items[i].iQuantity, 2);
                    decimal newMoney = Math.Round(newSum / (100m + sTaxRate) * 100m, 2);
                    decimal newTax = newSum - newMoney;
                    decimal newNatMoney = Math.Round(newMoney * origExchRate, 2);
                    decimal newNatTax = Math.Round(newTax * origExchRate, 2);
                    decimal newNatSum = Math.Round(newSum * origExchRate, 2);
                    string newINum = "";
                    string dummy1, dummy2;
                    BasicDAL.GetAssQty(zt, items[i].cInvCode, items[i].iQuantity, out newINum, out dummy1, out dummy2);
                    string batchClause = "";
                    string origWhCode = dtDetail.Rows[0]["cWhCode"].ToString();
                    string origBatch = dtDetail.Rows[0]["cBatch"].ToString();
                    string newWhCode = string.IsNullOrEmpty(items[i].cWhCode) ? origWhCode : items[i].cWhCode;
                    string newBatch = string.IsNullOrEmpty(items[i].cBatch) ? origBatch : items[i].cBatch;
                    if (!string.IsNullOrEmpty(newWhCode))
                    {
                        sql = " select 1 from Warehouse (nolock) where cwhcode='" + newWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
                        DataTable dtWh = U8SqlDBHelper.GetDataTable(sql);
                        if (dtWh.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：仓库编码[" + newWhCode + "]无数据或已停用！\"}";
                        }
                    }
                    if (BasicDAL.IsInvQuality(items[i].cInvCode))
                    {
                        if (string.IsNullOrEmpty(items[i].dMadeDate))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                        }
                        try
                        {
                            DateTime dt2 = Convert.ToDateTime(items[i].dMadeDate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：生产日期[" + items[i].dMadeDate + "]格式错误！\"}";
                        }
                    }
                    if (BasicDAL.IsBatch(items[i].cInvCode, "") && string.IsNullOrEmpty(newBatch))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                    }
                    if (qtyDiff > 0m && !string.IsNullOrEmpty(newWhCode))
                    {
                        batchClause = "";
                        if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                        {
                            batchClause = " and cBatch='" + newBatch + "' ";
                        }
                        sql = "select isnull(iQuantity,0)-isnull(fOutQuantity,0) from CurrentStock where cWhCode = '" + newWhCode + "' and cInvCode ='" + items[i].cInvCode + "' " + batchClause;
                        decimal stockQty = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
                        if (qtyDiff > stockQty)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"第" + (i + 1) + "行：存货[" + items[i].cInvCode + "]可用量不足(可用:" + stockQty + ",需增加:" + qtyDiff + ")！\"}";
                        }
                    }
                    if (!string.IsNullOrEmpty(origWhCode))
                    {
                        string origBatchFilter = "";
                        if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                        {
                            origBatchFilter = " and isnull(cBatch,'') = '" + origBatch + "' ";
                        }
                        if (qtyDiff > 0m)
                        {
                            sql = " update CurrentStock set fOutQuantity=isnull(fOutQuantity,0)+" + qtyDiff + ",fOutNum=isnull(fOutNum,0)+" + qtyDiff + "  where cWhCode = '" + origWhCode + "' and cInvCode ='" + items[i].cInvCode + "' " + origBatchFilter;
                        }
                        else
                        {
                            sql = " update CurrentStock set fOutQuantity=isnull(fOutQuantity,0)-" + (-qtyDiff) + ",fOutNum=isnull(fOutNum,0)-" + (-qtyDiff) + "  where cWhCode = '" + origWhCode + "' and cInvCode ='" + items[i].cInvCode + "' " + origBatchFilter;
                        }
                        list.Add(sql);
                    }
                    decimal origMoney = BasicDAL.ToDec(dtDetail.Rows[0]["iMoney"].ToString());
                    decimal moneyDiff = newMoney - origMoney;
                    sql = " update DispatchLists set iQuantity=" + items[i].iQuantity + ", iNum=" + newINum + ", iMoney=" + newMoney + ", iTax=" + newTax + ", iSum=" + newSum + ", iNatMoney=" + newNatMoney + ", iNatTax=" + newNatTax + ", iNatSum=" + newNatSum;
                    if (!string.IsNullOrEmpty(items[i].cBatch))
                    {
                        sql = sql + ", cBatch='" + items[i].cBatch + "'";
                    }
                    if (!string.IsNullOrEmpty(items[i].cWhCode))
                    {
                        sql = sql + ", cWhCode='" + items[i].cWhCode + "'";
                    }
                    if (items[i].bIAcreatebill == "0" || items[i].bIAcreatebill == "1")
                    {
                        sql = sql + ", bIAcreatebill=" + (items[i].bIAcreatebill == "0" ? "0" : "1");
                    }
                    if (!string.IsNullOrEmpty(items[i].cDefine22))
                    {
                        sql = sql + ", cDefine22='" + items[i].cDefine22 + "'";
                    }
                    if (!string.IsNullOrEmpty(items[i].cDefine23))
                    {
                        sql = sql + ", cDefine23='" + items[i].cDefine23 + "'";
                    }
                    sql = sql + " where iDLsID='" + items[i].iDLsID + "'";
                    list.Add(sql);
                    decimal newNatMoneySO = Math.Round(newMoney, 2);
                    sql = " update so_sodetails set iFHQuantity=isnull(iFHQuantity,0)+" + qtyDiff + ", iFHNum=isnull(iFHNum,0)+" + qtyDiff + ", iFHMoney=isnull(iFHMoney,0)+" + moneyDiff + " where iSOsID = '" + iSOsID + "' ";
                    list.Add(sql);
                }
                else
                {
                    sql = " update DispatchLists set ";
                    bool hasField = false;
                    if (!string.IsNullOrEmpty(items[i].cBatch))
                    {
                        sql = sql + " cBatch='" + items[i].cBatch + "',";
                        hasField = true;
                    }
                    if (!string.IsNullOrEmpty(items[i].cWhCode))
                    {
                        sql = sql + " cWhCode='" + items[i].cWhCode + "',";
                        hasField = true;
                    }
                    if (items[i].bIAcreatebill == "0" || items[i].bIAcreatebill == "1")
                    {
                        sql = sql + " bIAcreatebill=" + (items[i].bIAcreatebill == "0" ? "0" : "1") + ",";
                        hasField = true;
                    }
                    if (!string.IsNullOrEmpty(items[i].cDefine22))
                    {
                        sql = sql + " cDefine22='" + items[i].cDefine22 + "',";
                        hasField = true;
                    }
                    if (!string.IsNullOrEmpty(items[i].cDefine23))
                    {
                        sql = sql + " cDefine23='" + items[i].cDefine23 + "',";
                        hasField = true;
                    }
                    if (hasField)
                    {
                        sql = sql.TrimEnd(',') + " where iDLsID='" + items[i].iDLsID + "'";
                        list.Add(sql);
                    }
                }
            }
            sql = " update DispatchList set ";
            bool hasHeaderField = false;
            if (!string.IsNullOrEmpty(d.dDate))
            {
                sql = sql + " dDate='" + Convert.ToDateTime(d.dDate).ToString("yyyy-MM-dd") + "',";
                hasHeaderField = true;
            }
            if (!string.IsNullOrEmpty(d.cDepCode))
            {
                sql = sql + " cDepCode='" + d.cDepCode + "',";
                hasHeaderField = true;
            }
            if (!string.IsNullOrEmpty(d.cPersonCode))
            {
                sql = sql + " cPersonCode='" + d.cPersonCode + "',";
                hasHeaderField = true;
            }
            if (!string.IsNullOrEmpty(d.cMemo))
            {
                sql = sql + " cMemo='" + d.cMemo + "',";
                hasHeaderField = true;
            }
            if (d.bsaleoutcreatebill == "0" || d.bsaleoutcreatebill == "1")
            {
                sql = sql + " bsaleoutcreatebill=" + (d.bsaleoutcreatebill == "0" ? "0" : "1") + ",";
                hasHeaderField = true;
            }
            if (!string.IsNullOrEmpty(d.cDefine13))
            {
                sql = sql + " cDefine13='" + d.cDefine13 + "',";
                hasHeaderField = true;
            }
            if (hasHeaderField)
            {
                sql = sql.TrimEnd(',') + " where cDLCode='" + d.cCode + "'";
                list.Add(sql);
            }
            if (list.Count == 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"没有需要更新的数据！\",\"U8Code\":\"" + d.cCode + "\" }";
            }
            int num = U8SqlDBHelper.ExecuteSqlTran(list);
            if (num > 0)
            {
                string fEntry = "[" + string.Join(",", fEntryListUpdate) + "]";
                return "{\"Code\":\"200\",\"Msg\":\"U8发货单[" + d.cCode + "]修改成功！\",\"U8Code\":\"" + d.cCode + "\",\"data\":{\"Number\":\"" + d.cCode + "\",\"Id\":\"" + DLID + "\",\"FEntry\":" + fEntry + "} }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8发货单修改失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string seedYearMonth0 = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + seedYearMonth0 + "\",\"U8Code\":\"\"}";
        }
    }

    public static string Current(Current c)
    {
        List<string> list = new List<string>();
        string sql = "";
        try
        {
            List<Currents> items = c.Items;
            int num = 0;
            string seedYearMonth = "";
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].cWhCode))
                {
                    seedYearMonth += "仓库编码[cWhCode]未传递！";
                    continue;
                }
                sql = " select 1 from Warehouse (nolock) where cwhcode='" + items[i].cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count == 0)
                {
                    seedYearMonth = seedYearMonth + "仓库编码[" + items[i].cWhCode + "]无数据或已停用！";
                    continue;
                }
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    seedYearMonth += "存货编码[cInvCode]未传递！";
                    continue;
                }
                sql = " select 1 from inventory (nolock) where cinvcode = '" + items[i].cInvCode + "' and isnull(dEDate,'2099-01-01')>getdate() ";
                DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable2.Rows.Count == 0)
                {
                    seedYearMonth = seedYearMonth + "存货编码[" + items[i].cInvCode + "]无数据或已停用！";
                    continue;
                }
                sql = " select 1 from XD_MEScurrent (nolock) where cinvcode = '" + items[i].cInvCode + "' and cwhcode = '" + items[i].cWhCode + "' and isnull(cbatch,'')='" + items[i].cBatch + "' ";
                DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable3.Rows.Count == 0)
                {
                    sql = " insert into XD_MEScurrent(cWhCode,cInvCode,iQuantity,cBatch,dMadeDate,ctime)  values ('" + items[i].cWhCode + "','" + items[i].cInvCode + "'," + items[i].iQuantity + ",'" + items[i].cBatch + "','" + items[i].dMadeDate + "',getdate()) ";
                    U8SqlDBHelper.ExecuteSql(sql);
                    num++;
                }
                else
                {
                    sql = " update XD_MEScurrent set iQuantity=" + items[i].iQuantity + ",dMadeDate='" + items[i].dMadeDate + "',ctime=getdate()  where cinvcode = '" + items[i].cInvCode + "' and cwhcode = '" + items[i].cWhCode + "' and isnull(cbatch,'')='" + items[i].cBatch + "' ";
                    U8SqlDBHelper.ExecuteSql(sql);
                    num++;
                }
            }
            if (num > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"WMS库存数据更新成功[" + num + "]条！[" + seedYearMonth + "]\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"WMS库存数据更新失败！[" + seedYearMonth + "]\"}";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sql);
            string result = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + result + "\",\"U8Code\":\"\"}";
        }
    }

    public static string SaleBill(SaleBill sb)
    {
        List<string> sqlList = new List<string>();
        string sqlQuery = "";
        try
        {
            if (string.IsNullOrEmpty(sb.cSBVCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"发票号[cSBVCode]未传递！\"}";
            }
            sqlQuery = "select 1 from salebillvouch where csbvcode = '" + sb.cSBVCode + "' ";
            DataTable resultTable = U8SqlDBHelper.GetDataTable(sqlQuery);
            if (resultTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"发票号[" + sb.cSBVCode + "]已存在！\"}";
            }
            if (sb.cVouchType != "ZP" && sb.cVouchType != "PP")
            {
                return "{\"Code\":\"400\",\"Msg\":\"发票类型[" + sb.cVouchType + "]不正确！\"}";
            }
            if (string.IsNullOrEmpty(sb.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(sb.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"开票日期[dDate]未传递！\"}";
            }
            DateTime invoiceDate;
            try
            {
                invoiceDate = Convert.ToDateTime(sb.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"开票日期[" + sb.dDate + "]格式错误！\"}";
            }
            if (!string.IsNullOrEmpty(sb.cDepCode))
            {
                sqlQuery = " select bDepEnd from department where cdepcode = '" + sb.cDepCode + "' ";
                resultTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                if (resultTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门[" + sb.cDepCode + "]不存在！\"}";
                }
                if (resultTable.Rows[0]["bDepEnd"].ToString() == "False")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门[" + sb.cDepCode + "]不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(sb.cPersonCode))
            {
                sqlQuery = " select 1 from Person where cPersonCode = '" + sb.cPersonCode + "' ";
                resultTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                if (resultTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"业务员[" + sb.cPersonCode + "]不存在！\"}";
                }
            }
            if (sb.Items == null)
            {
                return "{\"Code\":\"400\",\"Msg\":\"发票明细[Items]未传递！\"}";
            }
            List<SaleBills> invoiceItems = sb.Items;
            DataTable dispatchTable = new DataTable();
            DataTable outboundTable = new DataTable();
            string currencyName = "";
            string customerCode = "";
            for (int i = 0; i < invoiceItems.Count; i++)
            {
                if (invoiceItems[i].iQuantity == 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"开票数量[iQuantity]不可为0！\"}";
                }
                if (invoiceItems[0].iQuantity > 0m && invoiceItems[i].iQuantity < 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"开票数量[iQuantity]错误！\"}";
                }
                if (invoiceItems[0].iQuantity < 0m && invoiceItems[i].iQuantity > 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"开票数量[iQuantity]错误！\"}";
                }
                if (string.IsNullOrEmpty(invoiceItems[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                if (string.IsNullOrWhiteSpace(invoiceItems[i].crmDetailId))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"CRM单据明细ID[crmDetailId]未传递！\"}";
                }
                sqlQuery = " select 1 from inventory (nolock) where cInvCode = '" + invoiceItems[i].cInvCode + "' ";
                DataTable checkTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                if (checkTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\" 存货编码[" + invoiceItems[i].cInvCode + "]不存在！\"}";
                }
                if (BasicDAL.ToDec(invoiceItems[i].iTaxRate) < 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"税率[" + BasicDAL.ToDec(invoiceItems[i].iTaxRate) + "]错误！\"}";
                }
                if (string.IsNullOrEmpty(invoiceItems[i].PatchCode) && string.IsNullOrEmpty(invoiceItems[i].Rdids))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货来源[PatchCode]与出库来源[Rdids]未传递！\"}";
                }
                if (!string.IsNullOrEmpty(invoiceItems[i].PatchCode))
                {
                    sqlQuery = " select b.cexch_name,b.ccuscode,a.cinvcode,a.iSettleQuantity,a.iQuantity from DispatchLists a   left join DispatchList b on a.dlid=b.dlid where b.cdlcode='" + invoiceItems[i].PatchCode + "' and a.irowno = '" + invoiceItems[i].PatchRow + "' ";
                    dispatchTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                    if (dispatchTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"发货单号[" + invoiceItems[i].PatchCode + "]行[" + invoiceItems[i].PatchRow + "]无数据！\"}";
                    }
                    if (Math.Abs(invoiceItems[i].iQuantity) + Math.Abs(BasicDAL.ToDec(dispatchTable.Rows[0]["iSettleQuantity"].ToString())) > Math.Abs(BasicDAL.ToDec(dispatchTable.Rows[0]["iQuantity"].ToString())))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"发货单号[" + invoiceItems[i].PatchCode + "]行[" + invoiceItems[i].PatchRow + "]累计开票数量大于发货数量！\"}";
                    }
                    if (invoiceItems[i].cInvCode != dispatchTable.Rows[0]["cinvcode"].ToString())
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + invoiceItems[i].cInvCode + "]不属于发货单[" + invoiceItems[i].PatchCode + "]行[" + invoiceItems[i].PatchRow + "]！\"}";
                    }
                    if (i == 0)
                    {
                        currencyName = dispatchTable.Rows[0]["cexch_name"].ToString();
                        customerCode = dispatchTable.Rows[0]["ccuscode"].ToString();
                        continue;
                    }
                    if (currencyName != dispatchTable.Rows[0]["cexch_name"].ToString())
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"来源单据币种必须一致！\"}";
                    }
                    if (customerCode != dispatchTable.Rows[0]["ccuscode"].ToString())
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"来源单据客户编码必须一致！\"}";
                    }
                    continue;
                }
                sqlQuery = "select c.cexch_name,a.cinvcode,b.ccuscode,a.fsettleqty,a.iQuantity from rdrecords32 a  left join rdrecord32 b on a.ID = b.ID  left join SO_SOMain c on a.iordercode=c.csocode   where a.autoid = '" + invoiceItems[i].Rdids + "' ";
                outboundTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                if (outboundTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库单ID[" + invoiceItems[i].Rdids + "]无数据！\"}";
                }
                if (Math.Abs(invoiceItems[i].iQuantity) + Math.Abs(BasicDAL.ToDec(outboundTable.Rows[0]["fsettleqty"].ToString())) > Math.Abs(BasicDAL.ToDec(outboundTable.Rows[0]["iQuantity"].ToString())))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"出库单ID[" + invoiceItems[i].Rdids + "]累计开票数量大于出库数量！\"}";
                }
                if (invoiceItems[i].cInvCode != outboundTable.Rows[0]["cinvcode"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + invoiceItems[i].cInvCode + "]出库单ID[" + invoiceItems[i].Rdids + "]！\"}";
                }
                if (i == 0)
                {
                    currencyName = outboundTable.Rows[0]["cexch_name"].ToString();
                    customerCode = outboundTable.Rows[0]["ccuscode"].ToString();
                    continue;
                }
                if (currencyName != outboundTable.Rows[0]["cexch_name"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"来源单据币种必须一致！\"}";
                }
                if (customerCode != outboundTable.Rows[0]["ccuscode"].ToString())
                {
                    return "{\"Code\":\"400\",\"Msg\":\"来源单据客户编码必须一致！\"}";
                }
            }
            int mainVouchId = BasicDAL.GetVouchId("F", cAcc_Id, "BILLVOUCH");
            string mainVouchIdStr = "1" + $"{mainVouchId:D9}";
            string invTypeCode = "";
            string iVTidValue = "";
            string sysBarPrefix = "";
            if (sb.cVouchType == "ZP")
            {
                invTypeCode = "26";
                iVTidValue = "53";
                sysBarPrefix = "||SA71|";
            }
            else
            {
                invTypeCode = "27";
                iVTidValue = "17";
                sysBarPrefix = "||SA72|";
            }
            string depCodeParam = "cDepCode";
            string personCodeParam = "cPersonCode";
            if (!string.IsNullOrEmpty(sb.cDepCode))
            {
                depCodeParam = "'" + sb.cDepCode + "'";
            }
            if (!string.IsNullOrEmpty(sb.cPersonCode))
            {
                personCodeParam = "'" + sb.cPersonCode + "'";
            }
            string customerBank = "null";
            string customerAccount = "null";
            string customerAddress = "null";
            sqlQuery = "select ccusbank,ccusaccount,cCusOAddress from customer where ccuscode = '" + customerCode + "' ";
            resultTable = U8SqlDBHelper.GetDataTable(sqlQuery);
            if (!string.IsNullOrEmpty(resultTable.Rows[0]["ccusbank"].ToString()))
            {
                customerBank = string.Concat("'", resultTable.Rows[0]["ccusbank"], "'");
            }
            if (!string.IsNullOrEmpty(resultTable.Rows[0]["ccusaccount"].ToString()))
            {
                customerAccount = string.Concat("'", resultTable.Rows[0]["ccusaccount"], "'");
            }
            if (!string.IsNullOrEmpty(resultTable.Rows[0]["cCusOAddress"].ToString()))
            {
                customerAddress = string.Concat("'", resultTable.Rows[0]["cCusOAddress"], "'");
            }
            int returnFlag = 0;
            if (invoiceItems[0].iQuantity < 0m)
            {
                returnFlag = 1;
            }
            string verifierSqlValue = "NULL";
            string verifyDateSqlValue = "NULL";
            string verifySysTimeSqlValue = "NULL";
            if (!string.IsNullOrEmpty(sb.cVerifier))
            {
                verifierSqlValue = "'" + sb.cVerifier + "'";
                verifyDateSqlValue = "CONVERT(varchar(10),getdate(),120)";       // dverifydate: 仅日期
                verifySysTimeSqlValue = "getdate()";                             // dverifysystime: 含时分秒
            }
            string saleOutValue = "null";
            string sourceTableName = "";
            string orderCode;
            decimal exchangeRate;
            decimal mainTaxRate;
            string customerName;
            string dispatchCode;
            if (!string.IsNullOrEmpty(invoiceItems[0].Rdids))
            {
                // 优先：销售出库单路径（Rdids）
                sqlQuery = " select b.ccode,a.iordercode,c.cexch_name,c.iExchRate,c.iTaxRate,c.cCusName,dl.cDLCode  from rdrecords32 a  left join rdrecord32 b on a.ID = b.ID left join SO_SOMain c on a.iordercode=c.csocode  left join DispatchLists dls on a.iDLsID = dls.iDLsID  left join DispatchList dl on dls.DLID = dl.DLID  where a.autoid = '" + invoiceItems[0].Rdids + "' ";
                outboundTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                saleOutValue = string.Concat("'", outboundTable.Rows[0]["ccode"], "'");
                orderCode = outboundTable.Rows[0]["iordercode"].ToString();
                exchangeRate = BasicDAL.ToDec(outboundTable.Rows[0]["iExchRate"].ToString());
                mainTaxRate = BasicDAL.ToDec(outboundTable.Rows[0]["iTaxRate"].ToString());
                customerName = outboundTable.Rows[0]["cCusName"].ToString();
                dispatchCode = outboundTable.Rows[0]["cDLCode"].ToString();
                sourceTableName = string.Concat(" (select rd32.cSTCode,rd32.cCusCode,rd32.cBusType, (select top 1 so.cDefine1 from SO_SOMain so where so.csocode = '", orderCode, "') as cDefine1,rd32.cDefine2,rd32.cDefine3,", /* cDefine4在SaleBillVouch为datetime */ "NULL as cDefine4,", "rd32.cDefine5,", /* cDefine6在SaleBillVouch为datetime */ "NULL as cDefine6,", "rd32.cDefine7,rd32.cDefine8,rd32.cDefine9,rd32.cDefine10,rd32.cDefine11,rd32.cDefine12,rd32.cDefine13,rd32.cDefine14,rd32.cDefine15,rd32.cDefine16,", "(select top 1 so.cMemo from SO_SOMain so where so.csocode = '", orderCode, "') as cContractName,", "(select top 1 so2.cSCCode from SO_SOMain so2 where so2.csocode = '", orderCode, "') as cSCCode,", "rd32.cDepCode,rd32.cPersonCode from rdrecord32 rd32 where rd32.ccode = '", outboundTable.Rows[0]["ccode"], "') t ");
            }
            else if (!string.IsNullOrEmpty(invoiceItems[0].PatchCode))
            {
                // 备选：发货单路径（PatchCode）
                sqlQuery = " select a.cordercode,b.cexch_name,b.iExchRate,b.iTaxRate,b.cCusName,b.cDLCode from DispatchLists a   left join DispatchList b on a.dlid=b.dlid where b.cdlcode='" + invoiceItems[0].PatchCode + "' and a.irowno = '" + invoiceItems[0].PatchRow + "' ";
                dispatchTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                orderCode = dispatchTable.Rows[0]["cordercode"].ToString();
                exchangeRate = BasicDAL.ToDec(dispatchTable.Rows[0]["iExchRate"].ToString());
                mainTaxRate = BasicDAL.ToDec(dispatchTable.Rows[0]["iTaxRate"].ToString());
                customerName = dispatchTable.Rows[0]["cCusName"].ToString();
                dispatchCode = dispatchTable.Rows[0]["cDLCode"].ToString();
                sourceTableName = string.Concat(" (select cSTCode,cCusCode,cBusType,", "(select top 1 so.cDefine1 from SO_SOMain so where so.csocode = '", orderCode, "') as cDefine1,", "cDefine2,cDefine3,", /* cDefine4在SaleBillVouch为datetime */ "NULL as cDefine4,", "cDefine5,", /* cDefine6在SaleBillVouch为datetime */ "NULL as cDefine6,", "cDefine7,cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16,", "(select top 1 so.cMemo from SO_SOMain so where so.csocode = '", orderCode, "') as cContractName,", "cSCCode,", "cDepCode,cPersonCode from DispatchList where cDLCode = '", dispatchTable.Rows[0]["cDLCode"], "') t ");
            }
            else
            {
                return "{\"Code\":\"400\",\"Msg\":\"Rdids和PatchCode均为空，无法确定来源单据！\"}";
            }
            // ═══════════════════════════════════════════════════════════
            // 汇率取数：从 Exch 表取对应币别、对应会计期间的记账汇率
            // 不再使用来源单据（SO_SOMain/DispatchList）的 iExchRate
            // ITYPE='2' = 记账汇率（非调整汇率）
            // 查不到汇率直接报错，不做静默兜底
            // ═══════════════════════════════════════════════════════════
            if (string.IsNullOrEmpty(currencyName))
            {
                return "{\"Code\":\"400\",\"Msg\":\"获取汇率失败：来源单据币别名称为空，无法查询Exch表记账汇率！\",\"Items\":\"\"}";
            }
            if (currencyName == "人民币")
            {
                // 人民币汇率固定为1，无需查Exch表
                exchangeRate = 1;
            }
            else
            {
                string exchYearStr = invoiceDate.Year.ToString();
                string exchPeriodStr = invoiceDate.Month.ToString();
                string exchSql = "select nflat from Exch where CEXCH_NAME = '" + currencyName + "' and IYEAR = '" + exchYearStr + "' and Iperiod = '" + exchPeriodStr + "' and ITYPE = '2' ";
                string exchRateStr = U8SqlDBHelper.GetString(exchSql);
                if (string.IsNullOrEmpty(exchRateStr))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"获取汇率失败：币别[" + currencyName + "]在Exch表中未找到[" + exchYearStr + "]年[" + exchPeriodStr + "]期的记账汇率(ITYPE=2)，请先在U8基础档案-外币及汇率中维护！\",\"Items\":\"\"}";
                }
                exchangeRate = BasicDAL.ToDec(exchRateStr);
            }
            sqlQuery = " insert into SaleBillVouch (SBVID,cSBVCode,cVouchType,cSTCode,dDate,cSaleOut,cRdCode,\r\n                    cDepCode,cPersonCode,cSOCode,cCusCode,cPayCode,cexch_name,cMemo,iExchRate,\r\n                    iTaxRate,bReturnFlag,cBCode,cBillVer,cMaker,cInvalider,cVerifier,cChecker,dverifydate,dverifysystime,\r\n                    cBusType,bFirst,citem_class,citemcode,cHeadCode,bPayMent, iDisp,cCusName,cDLCode,iVTid,bIAFirst,cCreChpName,cInfoTypeCode,\r\n                    cSource,cSCCode,cShipAddress,ccusbank,ccusaccount, ioutgolden,cgatheringplan,dCreditStart,dGatheringDate,icreditdays,\r\n                    bCredit,caddcode,iverifystate,ireturncount,iswfcontrolled,icreditstate, dcreatesystime,iflowid,bcashsale,retail_id,                   \r\n                    cSysBarCode,iTaxBillState,cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,\r\n                    cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  select   '" + mainVouchIdStr + "','" + sb.cSBVCode + "','" + invTypeCode + "',cSTCode,'" + invoiceDate.Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'," + saleOutValue + ", NULL,  " + depCodeParam + "," + personCodeParam + ",'" + orderCode + "',cCusCode,NULL,'" + currencyName + "',cContractName, " + exchangeRate + ",  " + mainTaxRate + ", " + returnFlag + ",'001',NULL,'" + sb.cMaker + "',NULL," + verifierSqlValue + ", NULL, " + verifyDateSqlValue + " , " + verifySysTimeSqlValue + ",   cBusType, 0, NULL,NULL,NULL,NULL, 1, '" + customerName + "', '" + dispatchCode + "','" + iVTidValue + "',0,NULL,NULL,  '销售',cSCCode," + customerAddress + "," + customerBank + "," + customerAccount + ",NULL,NULL,NULL,NULL,NULL,  0,NULL,0,NULL,0, NULL, GETDATE(),0,0, NULL,    '" + sysBarPrefix + sb.cSBVCode + "', 0, cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,  cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16  from  " + sourceTableName;
            sqlList.Add(sqlQuery);
            // 写入表头扩展自定义项6（chdefine6 = 用户传入的备注）
            sqlQuery = " insert into SaleBillVouch_extradefine (SBVID, chdefine6) values ('" + mainVouchIdStr + "','" + sb.cMemo + "') ";
            sqlList.Add(sqlQuery);
            for (int j = 0; j < invoiceItems.Count; j++)
            {
                int detailVouchId = BasicDAL.GetVouchId("C", cAcc_Id, "BILLVOUCH");
                string detailVouchIdStr = "1" + $"{detailVouchId:D9}";
                string invName = U8SqlDBHelper.GetString("select cinvname from inventory where cinvcode = '" + invoiceItems[j].cInvCode + "'");
                string itemClassValue = "null";
                string itemCodeValue = "null";
                string itemCNameValue = "null";
                string itemNameValue = "null";
                string detailSourceTable = "";
                string whCode;
                string soDetailId;
                string soCode;
                string soRowNo;
                string sourceCodeValue;
                string saleOutIdValue;
                string rdCodeValue;
                string detailIdValue;
                decimal unitPrice;
                decimal taxUnitPrice;
                decimal natUnitPrice;
                decimal detailTaxRate;
                decimal untaxedAmount;
                decimal taxAmount;
                decimal taxValue;
                decimal natUntaxedAmount;
                decimal natTaxAmount;
                decimal natTaxValue;
                if (!string.IsNullOrEmpty(invoiceItems[j].Rdids))
                {
                    // 优先：销售出库单路径（Rdids）
                    sqlQuery = " select b.cWhCode,c.iUnitPrice,c.iTaxUnitPrice,c.iNatUnitPrice,c.iTaxRate,a.iorderdid,  c.cItem_class,c.cItemCode,c.cItem_CName,c.cItemName,c.csocode,c.iRowNo,a.cbdlcode,a.autoid,b.ccode,a.iDLsID  from rdrecords32 a  left join rdrecord32 b on a.ID = b.ID left join SO_SODetails c on a.iorderdid=c.iSOsID  where a.autoid = '" + invoiceItems[j].Rdids + "' ";
                    outboundTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                    whCode = outboundTable.Rows[0]["cWhCode"].ToString();
                    soDetailId = outboundTable.Rows[0]["iorderdid"].ToString();
                    soCode = outboundTable.Rows[0]["csocode"].ToString();
                    soRowNo = outboundTable.Rows[0]["iRowNo"].ToString();
                    sourceCodeValue = outboundTable.Rows[0]["cbdlcode"].ToString();
                    saleOutIdValue = string.Concat("'", outboundTable.Rows[0]["autoid"], "'");
                    rdCodeValue = string.Concat("'", outboundTable.Rows[0]["ccode"], "'");
                    detailIdValue = outboundTable.Rows[0]["iDLsID"].ToString();
                    unitPrice = BasicDAL.ToDec(outboundTable.Rows[0]["iUnitPrice"].ToString());
                    taxUnitPrice = BasicDAL.ToDec(outboundTable.Rows[0]["iTaxUnitPrice"].ToString());
                    natUnitPrice = BasicDAL.ToDec(outboundTable.Rows[0]["iNatUnitPrice"].ToString());
                    detailTaxRate = BasicDAL.ToDec(invoiceItems[j].iTaxRate);
                    // ⚠️ 原公式: taxValue = taxAmount - untaxedAmount 在iUnitPrice与iTaxUnitPrice精度不一致时会产生0.01误差
                    // 修正：税额通过税率倒算，保证 iMoney(不含税金额) + iTax(税额) = iSum(含税金额) 恒成立
                    taxAmount = Math.Round(taxUnitPrice * invoiceItems[j].iQuantity, 2);     // [iSum] 含税金额 = 含税单价 × 数量
                    taxValue = Math.Round(taxAmount * detailTaxRate / (100m + detailTaxRate), 2); // [iTax] 税额 = 含税金额 × 税率/(100+税率)
                    untaxedAmount = taxAmount - taxValue;                                    // [iMoney] 不含税金额 = 含税金额 - 税额
                    natTaxAmount = Math.Round(taxAmount * exchangeRate, 2);                   // [iNatSum] 本币含税金额
                    natUntaxedAmount = Math.Round(untaxedAmount * exchangeRate, 2);           // [iNatMoney] 本币不含税金额
                    natTaxValue = natTaxAmount - natUntaxedAmount;                            // [iNatTax] 本币税额
                    // 税率改用传入值后，无税单价和本币单价需按新税率重算
                    // unitPrice = 含税单价 × 100 / (100 + 税率)，避免中间金额 Round 丢精度
                    unitPrice = Math.Round(taxUnitPrice * 100m / (100m + detailTaxRate), 6);                     // [iUnitPrice] 无税单价
                    natUnitPrice = Math.Round(unitPrice * exchangeRate, 6);                                       // [iNatUnitPrice] 本币单价 = 无税单价 × 汇率
                    if (!string.IsNullOrEmpty(outboundTable.Rows[0]["cItem_class"].ToString()))
                    {
                        itemClassValue = string.Concat("'", outboundTable.Rows[0]["cItem_class"], "'");
                        itemCodeValue = string.Concat("'", outboundTable.Rows[0]["cItemCode"], "'");
                        itemCNameValue = string.Concat("'", outboundTable.Rows[0]["cItem_CName"], "'");
                        itemNameValue = string.Concat("'", outboundTable.Rows[0]["cItemName"], "'");
                    }
                    detailSourceTable = " rdrecords32 where autoid = '" + invoiceItems[j].Rdids + "' ";
                }
                else if (!string.IsNullOrEmpty(invoiceItems[j].PatchCode))
                {
                    // 备选：发货单路径（PatchCode）
                    sqlQuery = " select a.cWhCode,c.iUnitPrice,c.iTaxUnitPrice,c.iNatUnitPrice,c.iTaxRate,a.iSOsID,a.iDLsID,c.cItem_class,c.cItemCode,c.cItem_CName,c.cItemName,c.csocode,c.iRowNo,b.cdlcode,a.cWhCode as whCode from DispatchLists a   left join DispatchList b on a.dlid=b.dlid left join SO_SODetails c on c.iSOsID=a.iSOsID  where b.cdlcode='" + invoiceItems[j].PatchCode + "' and a.irowno = '" + invoiceItems[j].PatchRow + "' ";
                    dispatchTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                    whCode = dispatchTable.Rows[0]["whCode"].ToString();
                    soDetailId = dispatchTable.Rows[0]["iSOsID"].ToString();
                    soCode = dispatchTable.Rows[0]["csocode"].ToString();
                    soRowNo = dispatchTable.Rows[0]["iRowNo"].ToString();
                    sourceCodeValue = dispatchTable.Rows[0]["cdlcode"].ToString();
                    saleOutIdValue = "NULL";
                    rdCodeValue = "NULL";
                    detailIdValue = dispatchTable.Rows[0]["iDLsID"].ToString();
                    unitPrice = BasicDAL.ToDec(dispatchTable.Rows[0]["iUnitPrice"].ToString());
                    taxUnitPrice = BasicDAL.ToDec(dispatchTable.Rows[0]["iTaxUnitPrice"].ToString());
                    natUnitPrice = BasicDAL.ToDec(dispatchTable.Rows[0]["iNatUnitPrice"].ToString());
                    detailTaxRate = BasicDAL.ToDec(invoiceItems[j].iTaxRate);
                    // ⚠️ 原公式: taxValue = taxAmount - untaxedAmount 在iUnitPrice与iTaxUnitPrice精度不一致时会产生0.01误差
                    // 修正：税额通过税率倒算，保证 iMoney(不含税金额) + iTax(税额) = iSum(含税金额) 恒成立
                    taxAmount = Math.Round(taxUnitPrice * invoiceItems[j].iQuantity, 2);     // [iSum] 含税金额 = 含税单价 × 数量
                    taxValue = Math.Round(taxAmount * detailTaxRate / (100m + detailTaxRate), 2); // [iTax] 税额 = 含税金额 × 税率/(100+税率)
                    untaxedAmount = taxAmount - taxValue;                                    // [iMoney] 不含税金额 = 含税金额 - 税额
                    natTaxAmount = Math.Round(taxAmount * exchangeRate, 2);                   // [iNatSum] 本币含税金额
                    natUntaxedAmount = Math.Round(untaxedAmount * exchangeRate, 2);           // [iNatMoney] 本币不含税金额
                    natTaxValue = natTaxAmount - natUntaxedAmount;                            // [iNatTax] 本币税额
                    // 税率改用传入值后，无税单价和本币单价需按新税率重算
                    // unitPrice = 含税单价 × 100 / (100 + 税率)，避免中间金额 Round 丢精度
                    unitPrice = Math.Round(taxUnitPrice * 100m / (100m + detailTaxRate), 6);                     // [iUnitPrice] 无税单价
                    natUnitPrice = Math.Round(unitPrice * exchangeRate, 6);                                       // [iNatUnitPrice] 本币单价 = 无税单价 × 汇率
                    if (!string.IsNullOrEmpty(dispatchTable.Rows[0]["cItem_class"].ToString()))
                    {
                        itemClassValue = string.Concat("'", dispatchTable.Rows[0]["cItem_class"], "'");
                        itemCodeValue = string.Concat("'", dispatchTable.Rows[0]["cItemCode"], "'");
                        itemCNameValue = string.Concat("'", dispatchTable.Rows[0]["cItem_CName"], "'");
                        itemNameValue = string.Concat("'", dispatchTable.Rows[0]["cItemName"], "'");
                    }
                    detailSourceTable = string.Concat(" DispatchLists where iDLsID = '", dispatchTable.Rows[0]["iDLsID"], "' ");
                }
                else
                {
                    return "{\"Code\":\"400\",\"Msg\":\"第" + (j + 1) + "行明细Rdids和PatchCode均为空，无法确定来源单据！\"}";
                }
                sqlQuery = " insert into SaleBillVouchs (SBVID,AutoID,cWhCode,cInvCode,iQuantity,iNum,iQuotedPrice,iUnitPrice,\r\n                        iTaxUnitPrice,iMoney,iTax,iSum,idiscount,iNatUnitPrice,\r\n                        iNatMoney,iNatTax,iNatSum,iNatDisCount,iSBVID,iMoneySum,iExchSum,iBatch,cBatch,bSettleAll,iTB,\r\n                        TBQuantity,iSOsID,iDLsID,KL,KL2,cInvName,iTaxRate,fOutQuantity,foutnum,fsaleprice,\r\n                        citemcode,citem_class,citemname,citem_cname,csocode,bgsp,cmassunit,bqaneedcheck,bqaurgency,bcosting,\r\n                        cordercode,iorderrowno,fcusminprice,irowno,iexpiratdatecalcu,cbdlcode,\r\n                        isaleoutid,bsaleprice,bgift,cbsaleout,cbsysbarcode)  select  '" + mainVouchIdStr + "','" + detailVouchIdStr + "',nullif('" + whCode + "',''),cInvCode," + invoiceItems[j].iQuantity + ",0,0," + unitPrice + ",  " + taxUnitPrice + "," + untaxedAmount + "," + taxValue + "," + taxAmount + ",0," + natUnitPrice + ",  " + natUntaxedAmount + "," + natTaxValue + "," + natTaxAmount + ",0, 0 ,0, 0, 0, cBatch, 0, 0,  0, '" + soDetailId + "', iDLsID, 100,100, '" + invName + "'," + detailTaxRate + ", 0, 0, 0,  " + itemCodeValue + "," + itemClassValue + "," + itemNameValue + "," + itemCNameValue + ",'" + soCode + "',0,0,0,0,bcosting,  '" + soCode + "','" + soRowNo + "',NULL," + invoiceItems[j].irowno + ",NULL,'" + sourceCodeValue + "',  " + saleOutIdValue + ",1, 0, " + rdCodeValue + ",'" + sysBarPrefix + sb.cSBVCode + "|" + invoiceItems[j].irowno + "'  from " + detailSourceTable;
                sqlList.Add(sqlQuery);
                // 写入明细扩展自定义项6（chdefine6 = CRM单据明细ID）
                if (!string.IsNullOrWhiteSpace(invoiceItems[j].crmDetailId))
                {
                    sqlQuery = " insert into SaleBillVouchs_extradefine (AutoID, cbdefine6) values ('" + detailVouchIdStr + "','" + invoiceItems[j].crmDetailId + "') ";
                    sqlList.Add(sqlQuery);
                }
                sqlQuery = " update SO_SODetails set iKPQuantity=isnull(iKPQuantity,0)+" + invoiceItems[j].iQuantity + ",iKPMoney=isnull(iKPMoney,0)+" + taxAmount + "  where iSOsID = '" + soDetailId + "' ";
                sqlList.Add(sqlQuery);
                sqlQuery = " update DispatchLists set iSettleQuantity=isnull(iSettleQuantity,0)+" + invoiceItems[j].iQuantity + " where iDLsID='" + detailIdValue + "' ";
                sqlList.Add(sqlQuery);
                if (!string.IsNullOrEmpty(invoiceItems[j].Rdids))
                {
                    sqlQuery = " update rdrecords32 set fsettleqty=isnull(fsettleqty,0)+" + invoiceItems[j].iQuantity + " where autoid = '" + invoiceItems[j].Rdids + "' ";
                    sqlList.Add(sqlQuery);
                }
            }
            resultTable?.Dispose();
            dispatchTable?.Dispose();
            outboundTable?.Dispose();
            // 调试：将所有SQL语句输出到日志，方便复制到数据库执行排查
            //for (int _dbg_i = 0; _dbg_i < sqlList.Count; _dbg_i++)
            //{
            //    LogException.WriteLog("===== SQL[" + _dbg_i + "] =====");
            //    LogException.WriteLog(sqlList[_dbg_i]);
            //}
            int executeResult = U8SqlDBHelper.ExecuteSqlTran(sqlList);
            if (executeResult > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8销售发票[" + sb.cSBVCode + "]创建完成！\",\"U8Code\":\"" + sb.cSBVCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8销售发票变更失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sqlQuery);
            string errorMsg = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
        }
    }

    public static string RedDispatch(RedDispatch d)
    {
        List<string> sqlList = new List<string>();
        string sqlQuery = "";
        try
        {
            if (string.IsNullOrEmpty(d.cMaker))
            {
                return "{\"Code\":\"400\",\"Msg\":\"制单人[cMaker]未传递！\"}";
            }
            if (string.IsNullOrEmpty(d.cCode))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
            }
            sqlQuery = " select 1 from dispatchlist where cdlcode = '" + d.cCode + "' ";
            DataTable resultTable = U8SqlDBHelper.GetDataTable(sqlQuery);
            if (resultTable.Rows.Count > 0)
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + d.cCode + "]已存在！\"}";
            }
            if (string.IsNullOrEmpty(d.dDate))
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[dDate]未传递！\",\"U8Code\":\"\"}";
            }
            DateTime invoiceDate;
            try
            {
                invoiceDate = Convert.ToDateTime(d.dDate);
            }
            catch
            {
                return "{\"Code\":\"400\",\"Msg\":\"单据日期[" + d.dDate + "]格式错误！\",\"U8Code\":\"\"}";
            }
            string yearMonth = invoiceDate.Year + invoiceDate.Month.ToString().PadLeft(2, '0');
            sqlQuery = " select bflag_ST from GL_mend where iYPeriod = '" + yearMonth + "' ";
            string closeFlag = U8SqlDBHelper.GetString(sqlQuery);
            if (closeFlag == "True")
            {
                return "{\"Code\":\"400\",\"Msg\":\" 日期[" + d.dDate + "]库存已关账！\"}";
            }
            if (!string.IsNullOrEmpty(d.cDepCode))
            {
                sqlQuery = " select * from Department where bDepEnd = '1' and cDepCode = '" + d.cDepCode + "' ";
                resultTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                if (resultTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + d.cDepCode + "]无数据或不是末级部门！\"}";
                }
            }
            if (!string.IsNullOrEmpty(d.cPersonCode))
            {
                sqlQuery = " select * from person where cPersonCode = '" + d.cPersonCode + "' ";
                resultTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                if (resultTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"人员编码[" + d.cPersonCode + "]无数据或已停用！\"}";
                }
            }
            List<RedDispatchs> items = d.Items;
            string sourceCurrency = "";
            string sourceCusCode = "";
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\"}";
                }
                if (items[i].iQuantity >= 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"退货数量[iQuantity]必须小于0！\"}";
                }
                if (string.IsNullOrEmpty(items[i].PatchCode) && string.IsNullOrEmpty(items[i].Rdids))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"发货来源[PatchCode]与出库来源[Rdids]未传递！\"}";
                }
                if (string.IsNullOrEmpty(items[i].PatchCode) && string.IsNullOrEmpty(items[i].Rdids))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"来源[PatchCode]与出库来源[Rdids]未传递！\"}";
                }
                if (!string.IsNullOrEmpty(items[i].PatchCode))
                {
                    sqlQuery = " select b.cexch_name,b.ccuscode,a.cinvcode,a.iRetQuantity,a.iQuantity from DispatchLists a   left join DispatchList b on a.dlid=b.dlid  where b.cdlcode='" + items[i].PatchCode + "' and a.irowno = '" + items[i].PatchRow + "' and b.bReturnFlag=0 ";
                    DataTable sourceTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                    if (sourceTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"发货单号[" + items[i].PatchCode + "]行[" + items[i].PatchRow + "]无数据！\"}";
                    }
                    if (items[i].cInvCode != sourceTable.Rows[0]["cinvcode"].ToString())
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]不属于发货单[" + items[i].PatchCode + "]行[" + items[i].PatchRow + "]！\"}";
                    }
                    decimal returnedQty = BasicDAL.ToDec(sourceTable.Rows[0]["iRetQuantity"].ToString());
                    decimal dispatchQty = BasicDAL.ToDec(sourceTable.Rows[0]["iQuantity"].ToString());
                    if (returnedQty + items[i].iQuantity > dispatchQty)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"发货单号[" + items[i].PatchCode + "]行[" + items[i].PatchRow + "]累计退货数量大于发货数量！\"}";
                    }
                    if (i == 0)
                    {
                        sourceCurrency = sourceTable.Rows[0]["cexch_name"].ToString();   // 币种名称
                        sourceCusCode = sourceTable.Rows[0]["ccuscode"].ToString();       // 客户编码
                    }
                    else
                    {
                        if (sourceCurrency != sourceTable.Rows[0]["cexch_name"].ToString())
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"来源单据币种必须一致！\"}";
                        }
                        if (sourceCusCode != sourceTable.Rows[0]["ccuscode"].ToString())
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"来源单据客户编码必须一致！\"}";
                        }
                    }
                }
                else
                {
                    sqlQuery = "select c.cexch_name,a.cinvcode,b.ccuscode,d.iRetQuantity,d.iQuantity from rdrecords32 a  left join rdrecord32 b on a.ID = b.ID  left join SO_SOMain c on a.iordercode=c.csocode  left join DispatchLists d on a.iDLsID=d.iDLsID   where a.autoid = '" + items[i].Rdids + "' and a.iQuantity > 0 ";
                    DataTable sourceTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                    if (sourceTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"出库单ID[" + items[i].Rdids + "]无数据！\"}";
                    }
                    decimal rdReturnedQty = BasicDAL.ToDec(sourceTable.Rows[0]["iRetQuantity"].ToString());
                    decimal rdQty = BasicDAL.ToDec(sourceTable.Rows[0]["iQuantity"].ToString());
                    if (rdReturnedQty + items[i].iQuantity > rdQty)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"出库单ID[" + items[i].Rdids + "]发货单累计退货数量大于发货数量！\"}";
                    }
                    if (items[i].cInvCode != sourceTable.Rows[0]["cinvcode"].ToString())
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货编码[" + items[i].cInvCode + "]出库单ID[" + items[i].Rdids + "]！\"}";
                    }
                    if (i == 0)
                    {
                        sourceCurrency = sourceTable.Rows[0]["cexch_name"].ToString();   // 币种名称
                        sourceCusCode = sourceTable.Rows[0]["ccuscode"].ToString();       // 客户编码
                    }
                    else
                    {
                        if (sourceCurrency != sourceTable.Rows[0]["cexch_name"].ToString())
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"来源单据币种必须一致！\"}";
                        }
                        if (sourceCusCode != sourceTable.Rows[0]["ccuscode"].ToString())
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"来源单据客户编码必须一致！\"}";
                        }
                    }
                }
                if (string.IsNullOrEmpty(items[i].cWhCode))
                {
                    continue;
                }
                sqlQuery = " select * from Warehouse (nolock) where cwhcode='" + items[i].cWhCode + "' and (dWhEndDate>getdate() or isnull(dWhEndDate,'')='') ";
                DataTable warehouseTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                if (warehouseTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"仓库编码[" + items[i].cWhCode + "]无数据或已停用！\"}";
                }
                if (BasicDAL.IsInvQuality(items[i].cInvCode))
                {
                    if (string.IsNullOrEmpty(items[i].dMadeDate))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用保质期管理，[dMadeDate]未传递！\"}";
                    }
                    try
                    {
                        DateTime madeDate = Convert.ToDateTime(items[i].dMadeDate);
                    }
                    catch
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"生产日期[" + items[i].dMadeDate + "]格式错误！\",\"U8Code\":\"\"}";
                    }
                }
                string batchFilter = "";
                if (BasicDAL.IsBatch(items[i].cInvCode, ""))
                {
                    if (string.IsNullOrEmpty(items[i].cBatch))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"存货[" + items[i].cInvCode + "]启用批号管理，[cBatch]未传递！\"}";
                    }
                    batchFilter = "and cbatch = '" + items[i].cBatch + "'";
                }
            }
            int mainSeqNo = BasicDAL.GetVouchId("F", cAcc_Id, "DISPATCH");
            string mainVouchId = "1" + $"{mainSeqNo:D9}";
            string depCodeParam = "cDepCode";
            if (!string.IsNullOrEmpty(d.cDepCode))
            {
                depCodeParam = "'" + d.cDepCode + "'";
            }
            string personCodeParam = "cPersonCode";
            if (!string.IsNullOrEmpty(d.cPersonCode))
            {
                personCodeParam = "'" + d.cPersonCode + "'";
            }
            string saleOutFlag = "1";
            if (d.bsaleoutcreatebill == "0")
            {
                saleOutFlag = "0";
            }
            string verifierValue = "null";
            string verifyDateValue = "null";
            string verifyTimeValue = "null";
            if (!string.IsNullOrEmpty(d.cVerifier))
            {
                verifierValue = "'" + d.cVerifier + "'";
                verifyDateValue = "CONVERT(varchar(10),getdate(),121)";
                verifyTimeValue = "getdate()";
            }
            string dispatchDLID = "";
            sqlQuery = (string.IsNullOrEmpty(items[0].PatchCode) ? ("select b.dlid from rdrecords32 a left join DispatchLists b on a.iDLsID=b.iDLsID  where a.autoid = '" + items[0].Rdids + "' ") : (" select a.dlid from DispatchLists a  left join DispatchList b on a.dlid=b.dlid  where b.cdlcode='" + items[0].PatchCode + "' and a.irowno = '" + items[0].PatchRow + "' and b.bReturnFlag=0 "));
            dispatchDLID = U8SqlDBHelper.GetString(sqlQuery);
            sqlQuery = " insert into dispatchlist  (DLID, cDLCode, cVouchType, cSTCode, dDate, cRdCode, cDepCode, cPersonCode, SBVID, cCusCode, cexch_name, iExchRate, iTaxRate,  bFirst, bReturnFlag, bSettleAll, cMemo, cVerifier, cMaker, iSale, cCusName, iVTid, cBusType,  bIAFirst, bCredit, iverifystate, iswfcontrolled, bARFirst, bsaleoutcreatebill,cShipAddress,caddcode ,  dverifydate, dcreatesystime, dverifysystime, iflowid, bsigncreate, bcashsale, bmustbook, bneedbill, baccswitchflag, cSaleOut ,  csocode,cinvoicecompany,csysbarcode,  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,   cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  \r\n select   '" + mainVouchId + "','" + d.cCode + "','05',cSTCode,'" + invoiceDate.ToString("yyyy-MM-dd") + "',null," + depCodeParam + " ," + personCodeParam + ",0,cCusCode,cexch_name,iExchRate, iTaxRate,  0,1 ,0, '" + d.cMemo + "'," + verifierValue + " ,'" + d.cMaker + "', 0,cCusName,'71',cBusType ,  0,0,0,0,0, " + saleOutFlag + ", cShipAddress, caddcode,  " + verifyDateValue + " ,getdate() ," + verifyTimeValue + " ,iflowid, 0,0,0,1,0,NULL ,   csocode,cinvoicecompany, '||SA01|" + d.cCode + "',  cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,  cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16  from dispatchlist where DLID = '" + dispatchDLID + "' ";
            sqlList.Add(sqlQuery);
            for (int j = 0; j < items.Count; j++)
            {
                string detailDLsID = "";
                sqlQuery = (string.IsNullOrEmpty(items[j].PatchCode) ? ("select iDLsID from rdrecords32 where autoid = '" + items[j].Rdids + "' ") : ("select iDLsID from DispatchLists a  left join DispatchList b on a.dlid=b.dlid  where b.cdlcode='" + items[0].PatchCode + "' and a.irowno = '" + items[0].PatchRow + "' and b.bReturnFlag=0 "));
                detailDLsID = U8SqlDBHelper.GetString(sqlQuery);
                string invCode = items[j].cInvCode;
                sqlQuery = " select * from dispatchlists where iDLsID = '" + detailDLsID + "'  ";
                DataTable dispatchDetailTable = U8SqlDBHelper.GetDataTable(sqlQuery);
                string dispatchCode = U8SqlDBHelper.GetString("select cDLCode from dispatchlist a left join dispatchlists b on a.DLID=b.DLID where b.iDLsID = '" + detailDLsID + "'");
                string soDetailId = U8SqlDBHelper.GetString("select iSOsID from dispatchlists where iDLsID = '" + detailDLsID + "'");
                decimal iQuantity = items[j].iQuantity;
                string assQuantityNum = "";
                string assRate;
                string assUnit;
                decimal assQuantityResult = BasicDAL.GetAssQty(zt, invCode, iQuantity, out assQuantityNum, out assRate, out assUnit);
                string whCodeValue = "null";
                string batchValue = "null";
                string madeDateValue = "null";
                string vDateValue = "null";
                string expDateValue = "null";
                string massDateValue = "null";
                string massUnitValue = "null";
                if (!string.IsNullOrEmpty(items[j].cWhCode))
                {
                    whCodeValue = "'" + items[j].cWhCode + "'";
                    string batchFilter2 = "";
                    if (BasicDAL.IsBatch(items[j].cInvCode, ""))
                    {
                        batchValue = "'" + items[j].cBatch + "'";
                        batchFilter2 = "and cbatch = '" + items[j].cBatch + "'";
                    }
                    if (BasicDAL.IsInvQuality(invCode))
                    {
                        sqlQuery = "select dMdate from CurrentStock where cWhCode = '" + items[j].cWhCode + "' and cInvCode ='" + items[j].cInvCode + "' " + batchFilter2;
                        string dmDateValue = U8SqlDBHelper.GetString(sqlQuery);
                        madeDateValue = "'" + dmDateValue + "'";
                        DateTime expiryDate = BasicDAL.GetVDate(invCode, dmDateValue);
                        vDateValue = "'" + expiryDate.ToString("yyyy-MM-dd") + "'";
                        expDateValue = "'" + expiryDate.AddDays(-1.0).ToString("yyyy-MM-dd") + "'";
                        massDateValue = BasicDAL.GetMassdate(invCode);
                        massUnitValue = BasicDAL.GetMassUnit(invCode);
                    }
                    sqlQuery = "update CurrentStock set fOutQuantity=isnull(fOutQuantity,0)+" + iQuantity + ",fOutNum=isnull(fOutNum,0)+" + assQuantityNum + "  where cWhCode = '" + items[j].cWhCode + "' and cInvCode ='" + items[j].cInvCode + "' " + batchFilter2;
                    sqlList.Add(sqlQuery);
                }
                string iaCreateFlag = "1";
                if (items[j].bIAcreatebill == "0")
                {
                    iaCreateFlag = "0";
                }
                int iTBValue = 0;
                if (items[j].iTB == 1)
                {
                    iTBValue = 1;
                }
                decimal exchRate = BasicDAL.ToDec(U8SqlDBHelper.GetString("select iExchRate from dispatchlist where cdlcode='" + dispatchCode + "'"));
                decimal unitPrice = BasicDAL.ToDec(dispatchDetailTable.Rows[0]["iUnitPrice"].ToString());
                decimal taxUnitPrice = BasicDAL.ToDec(dispatchDetailTable.Rows[0]["iTaxUnitPrice"].ToString());
                decimal taxRate = BasicDAL.ToDec(dispatchDetailTable.Rows[0]["iTaxRate"].ToString());
                decimal taxAmount = Math.Round(taxUnitPrice * iQuantity, 2);
                decimal untaxedAmount = Math.Round(taxAmount / (100m + taxRate) * 100m, 2);
                decimal taxValue = taxAmount - untaxedAmount;
                decimal natUntaxedAmount = Math.Round(untaxedAmount * exchRate, 2);
                decimal natTaxValue = Math.Round(taxValue * exchRate, 2);
                decimal natTaxAmount = Math.Round(taxAmount * exchRate, 2);
                int detailSeqNo = BasicDAL.GetVouchId("C", cAcc_Id, "DISPATCH");
                string detailVouchId = "1" + $"{detailSeqNo:D9}";
                sqlQuery = " insert into dispatchlists  (DLID, irowno, iDLsID,bsaleprice,  cInvCode, iQuantity, iNum, iUnitPrice, iTaxUnitPrice,  iMoney, iTax, iSum, iNatUnitPrice, iNatMoney, iNatTax, iNatSum, iSettleNum,  cBatch, bSettleAll, iTB, cInvName, iTaxRate, fOutQuantity, fOutNum, bIsSTQc,  cUnitID, bGsp, bQANeedCheck, bQAUrgency, bQAChecking, bQAChecked, KL, KL2,iCorID,cCorCode,   cordercode,iorderrowno,iSOsID,cSoCode, cItemCode,cItemName,cItem_class,cItem_CName,cMemo,  dMDate,dvDate,cExpirationdate, iMassDate, cMassUnit, bcosting, bIAcreatebill, bneedsign, cWhCode ,  cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 ) \r\n select   '" + mainVouchId + "'," + items[j].RowNo + ",'" + detailVouchId + "',1,  '" + invCode + "', " + iQuantity + "," + assQuantityNum + ", " + unitPrice + "," + taxUnitPrice + ",  " + untaxedAmount + ", " + taxValue + ", " + taxAmount + ",iNatUnitPrice , " + natUntaxedAmount + ", " + natTaxValue + "," + natTaxAmount + ", 0 ,  " + batchValue + ",0," + iTBValue + ", cInvName , iTaxRate , 0 ," + assQuantityNum + ",0,  cUnitID ,0,0,0,0,0, KL, KL2,iDLsID, '" + dispatchCode + "',  cSoCode , iRowNo, iSOsID, cSoCode, cItemCode,cItemName,cItem_class,cItem_CName,'" + items[j].cMemo + "',  " + madeDateValue + "," + vDateValue + "," + expDateValue + "," + massDateValue + "," + massUnitValue + ", 1," + iaCreateFlag + ",0, " + whCodeValue + ",  '" + items[j].cDefine22 + "','" + items[j].cDefine23 + "',cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,  cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 \r\n from dispatchlists  where iDLsID = '" + detailDLsID + "' ";
                sqlList.Add(sqlQuery);
                sqlQuery = " update so_sodetails set iFHQuantity=isnull(iFHQuantity,0)+ " + iQuantity + ",   iFHNum=isnull(iFHNum,0)+isnull(" + assQuantityNum + ",0) , iFHMoney=isnull(iFHMoney,0)+ " + untaxedAmount + "  where iSOsID = '" + soDetailId + "' ";
                sqlList.Add(sqlQuery);
                sqlQuery = " update dispatchlists set iRetQuantity=isnull(iRetQuantity,0) + " + Math.Abs(iQuantity) + " where iDLsID = '" + detailDLsID + "' ";
                sqlList.Add(sqlQuery);
                string busType = U8SqlDBHelper.GetString(" select cBusType from dispatchlist where DLID = '" + dispatchDLID + "'");
                sqlQuery = " insert into IA_SA_UnAccountVouch (IDUN,IDSUN,cVouTypeUN,cBustypeUN)  values ('" + mainVouchId + "','" + detailVouchId + "','05','" + busType + "' ) ";
                sqlList.Add(sqlQuery);
            }
            int executeResult = U8SqlDBHelper.ExecuteSqlTran(sqlList);
            if (executeResult > 0)
            {
                return "{\"Code\":\"200\",\"Msg\":\"U8退货单[" + d.cCode + "]创建成功！\",\"U8Code\":\"" + d.cCode + "\" }";
            }
            return "{\"Code\":\"400\",\"Msg\":\"U8退货单创建失败！\",\"U8Code\":\"\" }";
        }
        catch (Exception ex)
        {
            LogException.WriteLog(ex, sqlQuery);
            string errorMsg = "接口请求失败！原因：" + ex.Message;
            return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
        }
    }
}
}
