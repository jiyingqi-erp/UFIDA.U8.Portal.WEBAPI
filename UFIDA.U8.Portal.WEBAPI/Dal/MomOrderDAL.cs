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
    public class MomOrderDAL
    {
        private static string zt = ConfigurationManager.AppSettings["zt"].ToString();

        private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();

        public static string UptSubs(UptMom um)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(um.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[cCode]未传递！\",\"U8Code\":\"\"}";
                }
                sql = " select * from mom_order where mocode = '" + um.cCode + "' ";
                DataTable orderTable = U8SqlDBHelper.GetDataTable(sql);
                if (orderTable.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + um.cCode + "]不存在！\",\"U8Code\":\"\"}";
                    //return "{\"Code\":\"400\",\"Msg\":\"数据库SQL查询["+ sql +"]不存在！\",\"U8Code\":\"\"}";
                }
                string orderId = orderTable.Rows[0]["MOID"].ToString();
                string closeUser = U8SqlDBHelper.GetString("select isnull(CloseUser,'') from mom_orderdetail where MOID='" + orderId + "'");
                string orderDetailId = U8SqlDBHelper.GetString("select MODID from mom_orderdetail where MOID='" + orderId + "'");
                if (!string.IsNullOrEmpty(closeUser))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + um.cCode + "]已关闭，不可变更！\",\"U8Code\":\"\"}";
                }
                if (string.IsNullOrEmpty(um.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"变更人[cMaker]未传递！\",\"U8Code\":\"\"}";
                }
                List<MomSubs> subs = um.Subs;
                for (int i = 0; i < subs.Count; i++)
                {
                    if (string.IsNullOrEmpty(subs[i].OpType))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"操作类型[OpType]未传递！\",\"U8Code\":\"\"}";
                    }
                    if (subs[i].OpType != "新增" && subs[i].OpType != "删除")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"操作类型[" + subs[i].OpType + "]值无效！\",\"U8Code\":\"\"}";
                    }
                    if (string.IsNullOrEmpty(subs[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"产品编码[cInvCode]未传递！\",\"U8Code\":\"\"}";
                    }
                    sql = " select * from inventory (nolock) where cInvCode = '" + subs[i].cInvCode + "' ";
                    DataTable inventoryTable = U8SqlDBHelper.GetDataTable(sql);
                    if (inventoryTable.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 物料编码[" + subs[i].cInvCode + "]不存在！\",\"U8Code\":\"\"}";
                    }
                    if (subs[i].OpType == "新增")
                    {
                        sql = " select 1 from mom_moallocate where MODID='" + orderDetailId + "' and invcode='" + subs[i].cInvCode + "' ";
                        DataTable addallocateTable = U8SqlDBHelper.GetDataTable(sql);
                        if (addallocateTable.Rows.Count > 0 )
                        {
                            Func<string, string, string> CheckField = (val, fieldName) =>
                            {
                                if (!string.IsNullOrEmpty(val) && val.Trim() != "0")
                                {
                                    return $"[{fieldName}]不能重复赋值; ";
                                }
                                return "";
                            };

                            string errorMsg = "";
                            // 1 cWhCodeStringN60 (默认仓库编码)
                            errorMsg += CheckField(subs[i].cWhCode, "默认仓库编码");

                            // 2. BaseQtyNDecimalN (基础用量-分子)
                            errorMsg += CheckField(subs[i].BaseQtyN, "基础用量-分子");

                            // 3. BaseQtyDDecimalN (基础用量-分母)
                            errorMsg += CheckField(subs[i].BaseQtyD, "基础用量-分母");

                            // 4. ParentScrapDecimalN (母件损耗率)
                            errorMsg += CheckField(subs[i].ParentScrap, "母件损耗率");

                            // 5. CompScrapDecimalN (子件损耗率)
                            errorMsg += CheckField(subs[i].CompScrap, "子件损耗率");

                            // 如果有错误信息，返回 JSON 错误
                            if (!string.IsNullOrEmpty(errorMsg))
                            {
                                return "{\"Code\":\"400\",\"Msg\":\"物料编码[" + subs[i].cInvCode + "]已在工单中，" + errorMsg + "\",\"U8Code\":\"\"}";
                            }
                        }

                        decimal quantity = subs[i].iQuantity;
                        if (quantity == 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"应领数量不能为0！\",\"U8Code\":\"\"}";
                        }
                        if (quantity < 0m)
                        {
                            sql = " select Qty from mom_moallocate where MODID='" + orderDetailId + "' and invcode='" + subs[i].cInvCode + "' ";
                            DataTable subAllocateTable = U8SqlDBHelper.GetDataTable(sql);
                            if (subAllocateTable.Rows.Count == 0)
                            {
                                return "{\"Code\":\"400\",\"Msg\":\"扣减物料编码[" + subs[i].cInvCode + "]不存在！\",\"U8Code\":\"\"}";
                            }
                            decimal existingQty = BasicDAL.ToDec(subAllocateTable.Rows[0]["Qty"].ToString());
                            if (existingQty + quantity < 0m)
                            {
                                return "{\"Code\":\"400\",\"Msg\":\"物料编码[" + subs[i].cInvCode + "]现有数量不足扣减！\",\"U8Code\":\"\"}";
                            }
                        }
                    }
                    if (subs[i].OpType == "删除")
                    {
                        sql = " select IssQty,AllocateId from mom_moallocate where MODID='" + orderDetailId + "' and invcode='" + subs[i].cInvCode + "' ";
                        DataTable allocateTable = U8SqlDBHelper.GetDataTable(sql);
                        if (allocateTable.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 物料编码[" + subs[i].cInvCode + "]工单中无数据！\",\"U8Code\":\"\"}";
                        }
                        decimal issuedQuantity = BasicDAL.ToDec(allocateTable.Rows[0]["IssQty"].ToString());
                        string allocateId = allocateTable.Rows[0]["AllocateId"].ToString();
                        if (issuedQuantity > 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 物料编码[" + subs[i].cInvCode + "]已领用，不可删除！\",\"U8Code\":\"\"}";
                        }
                        sql = " select 1 from rdrecords11 where iMPoIds = '" + allocateId + "' ";
                        DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable.Rows.Count > 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 物料编码[" + subs[i].cInvCode + "]已有领料数据，不可删除！\",\"U8Code\":\"\"}";
                        }
                    }
                }
                int maxSortSeq = BasicDAL.ToInt(U8SqlDBHelper.GetString("select max(sortseq) from mom_moallocate where MODID='" + orderDetailId + "'"));
                for (int j = 0; j < subs.Count; j++)
                {
                    if (subs[j].OpType == "删除")
                    {
                        sql = " delete from mom_moallocate where MODID='" + orderDetailId + "' and invcode='" + subs[j].cInvCode + "' ";
                        sqlList.Add(sql);
                    }
                    if (subs[j].OpType == "新增")
                    {
                        // 检查子件是否已存在
                        sql = " select * from mom_moallocate where MODID='" + orderDetailId + "' and invcode='" + subs[j].cInvCode + "' ";
                        DataTable existingAllocateTable = U8SqlDBHelper.GetDataTable(sql);
                        
                        string partId = U8SqlDBHelper.GetString("select partid from bas_part where invcode = '" + subs[j].cInvCode + "'");
                        sql = "select Qty,sortseq from mom_orderdetail where MODID='" + orderDetailId + "'";
                        DataTable orderDetailTable = U8SqlDBHelper.GetDataTable(sql);
                        decimal orderQuantity = BasicDAL.ToDec(orderDetailTable.Rows[0]["Qty"].ToString());
                        int orderSortSeq = BasicDAL.ToInt(orderDetailTable.Rows[0]["sortseq"].ToString());
                        sql = "select * from mom_morder where MODID='" + orderDetailId + "'";
                        DataTable morderTable = U8SqlDBHelper.GetDataTable(sql);
                        string startDate = Convert.ToDateTime(morderTable.Rows[0]["StartDate"]).ToString("yyyy-MM-dd");
                        string dueDate = Convert.ToDateTime(morderTable.Rows[0]["DueDate"]).ToString("yyyy-MM-dd");
                        decimal baseQtyN = 1m;
                        decimal baseQtyD = 1m;
                        decimal parentScrap = default(decimal);
                        decimal compScrap = default(decimal);
                        string whCode = "null";
                        int wipType = 3;
                        int opComponentId = 0;
                        if (!string.IsNullOrEmpty(subs[j].BaseQtyD))
                        {
                            baseQtyD = BasicDAL.ToDec(subs[j].BaseQtyD);
                        }
                        baseQtyN = (string.IsNullOrEmpty(subs[j].BaseQtyN) ? Math.Round(subs[j].iQuantity / orderQuantity / baseQtyD, 2) : BasicDAL.ToDec(subs[j].BaseQtyN));

                        if (subs[j].iQuantity > 0m)
                        {
                            if (existingAllocateTable.Rows.Count > 0)
                            {
                                   // 子件已存在，更新记录：叠加iQuantity并覆盖其他字段
                                string existingAllocateId = existingAllocateTable.Rows[0]["AllocateId"].ToString();
                                decimal existingQuantity = BasicDAL.ToDec(existingAllocateTable.Rows[0]["Qty"].ToString());
                                decimal newQuantity = existingQuantity + subs[j].iQuantity;

                                sql = " update mom_moallocate set  Qty=" + newQuantity + ", StartDemDate='" + startDate + "', enddemdate='" + dueDate + "', whcode=" + whCode + ", wiptype=" + wipType + ", opcomponentid=" + opComponentId + ", cSubSysBarCode='||MO21|" + um.cCode + "|" + orderSortSeq + "|" + existingAllocateTable.Rows[0]["sortseq"] + "' where AllocateId='" + existingAllocateId + "' ";
                            }
                            else
                            {
                                maxSortSeq += 10;
                                BasicDAL.GetVouchId("F", cAcc_Id, "mom_moallocate");
                                int vouchId = BasicDAL.GetVouchId("C", cAcc_Id, "mom_moallocate");
                                string allocateId = "1" + $"{vouchId:D9}";

                                sql = " insert into mom_moallocate (allocateid,modid,sortseq,opseq,componentid,fvflag,BaseQtyN,BaseQtyD,  ParentScrap,CompScrap,Qty,IssQty,DeclaredQty,StartDemDate,enddemdate,  whcode,lotno,wiptype,byproductflag,qcflag,offset, invcode,opcomponentid,  replenishqty,transqty,producttype,sotype,qmflag,orgqty,orgauxqty, RequisitionFlag,RequisitionQty,RequisitionIssQty,CostWIPRel,uppermoqty,invalloeflag,cSubSysBarCode  ) values (  '" + allocateId + "','" + orderDetailId + "'," + maxSortSeq + ",'0000','" + partId + "',1, " + baseQtyN + "," + baseQtyD + ",  " + parentScrap + "," + compScrap + "," + subs[j].iQuantity + ",0,0, '" + startDate + "','" + dueDate + "',  " + whCode + ",null, " + wipType + ", 0, 0,0, '" + subs[j].cInvCode + "'," + opComponentId + ",  0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '||MO21|" + um.cCode + "|" + orderSortSeq + "|" + maxSortSeq + "' ) ";
                            }
                            sqlList.Add(sql);
                        }
                        else
                        {
                            string existingAllocateId = existingAllocateTable.Rows[0]["AllocateId"].ToString();
                            decimal existingQuantity = BasicDAL.ToDec(existingAllocateTable.Rows[0]["Qty"].ToString());
                            decimal newQuantity = existingQuantity + subs[j].iQuantity;
                            if (newQuantity == 0m)
                            {
                                sql = " delete from mom_moallocate where AllocateId='" + existingAllocateId + "' ";
                            }
                            else
                            {
                                sql = " update mom_moallocate set  Qty=" + newQuantity + ", StartDemDate='" + startDate + "', enddemdate='" + dueDate + "', whcode=" + whCode + ", wiptype=" + wipType + ", opcomponentid=" + opComponentId + ", cSubSysBarCode='||MO21|" + um.cCode + "|" + orderSortSeq + "|" + existingAllocateTable.Rows[0]["sortseq"] + "' where AllocateId='" + existingAllocateId + "' ";
                                //sql = " update mom_moallocate set Qty=" + newQuantity + " where AllocateId='" + existingAllocateId + "' ";
                            }
                            sqlList.Add(sql);
                        }
                    }
                }
                int executeResult = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (executeResult > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"工单[" + um.cCode + "]子件更新成功！\",\"U8Code\":\"" + um.cCode + "\"}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单[" + um.cCode + "]子件更新失败！\",\"U8Code\":\"" + um.cCode + "\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }
        public static string AddMom(MomOrder mo)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(mo.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[cCode]未传递！\",\"U8Code\":\"\"}";
                }
                sql = " select 1 from mom_order where mocode = '" + mo.cCode + "' ";
                DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + mo.cCode + "]已存在！\",\"U8Code\":\"\"}";
                }
                if (string.IsNullOrEmpty(mo.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"创建人[cMaker]未传递！\",\"U8Code\":\"\"}";
                }
                string userId = U8SqlDBHelper.GetString(" select cuser_id from ua_user where cuser_name = '" + mo.cMaker + "'");
                if (string.IsNullOrEmpty(userId))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"创建人[" + mo.cMaker + "]无U8用户数据！\",\"U8Code\":\"\"}";
                }
                int moClass = 1;
                string auditUser = "";
                if (mo.MoClass == "标准")
                {
                    moClass = 1;
                    auditUser = U8SqlDBHelper.GetString(" select cuser_id from ua_user where cuser_name = '董龙威'");
                }
                else
                {
                    if (!(mo.MoClass == "非标准"))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"订单类型[MoClass]无效！\",\"U8Code\":\"\"}";
                    }
                    moClass = 2;
                    auditUser = U8SqlDBHelper.GetString(" select cuser_id from ua_user where cuser_name = '章圣林'");
                }
                if (string.IsNullOrEmpty(auditUser))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"审核人不存在！\",\"U8Code\":\"\"}";
                }
                List<MomOrders> items = mo.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"产品编码[cInvCode]未传递！\",\"U8Code\":\"\"}";
                    }
                    sql = " select * from inventory (nolock) where cInvCode = '" + items[i].cInvCode + "' ";
                    DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable2.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 产品编码[" + items[i].cInvCode + "]不存在！\",\"U8Code\":\"\"}";
                    }
                    string bSelf = dataTable2.Rows[0]["bSelf"].ToString();
                    if (bSelf == "False")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 产品编码[" + items[i].cInvCode + "]不是自制产品！\",\"U8Code\":\"\"}";
                    }
                    if (!string.IsNullOrEmpty(items[i].cDepCode))
                    {
                        sql = " select 1 from Department where cDepCode = '" + items[i].cDepCode + "' and bDepEnd=1 ";
                        dataTable = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 部门编码[" + items[i].cDepCode + "]不存在或不是末级部门！\",\"U8Code\":\"\"}";
                        }
                    }
                    if (!string.IsNullOrEmpty(items[i].cItem_class))
                    {
                        sql = " select 1 from fitem (nolock) where citem_class='" + items[i].cItem_class + "' ";
                        dataTable = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 项目大类编码[" + items[i].cItem_class + "]不存在或不是末级部门！\",\"U8Code\":\"\"}";
                        }
                        if (string.IsNullOrEmpty(items[i].cItemCode))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"项目编码[cItemCode]未传递！\",\"U8Code\":\"\"}";
                        }
                        if (string.IsNullOrEmpty(items[i].cItemName))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"项目名称[cItemName]未传递！\",\"U8Code\":\"\"}";
                        }
                    }
                    decimal iQuantity = items[i].iQuantity;
                    if (iQuantity <= 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 生产数量必须大于0！\",\"U8Code\":\"\"}";
                    }
                    if (!string.IsNullOrEmpty(items[i].StartDate))
                    {
                        try
                        {
                            Convert.ToDateTime(items[i].StartDate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 开工日期格式不正确！\",\"U8Code\":\"\"}";
                        }
                    }
                    if (!string.IsNullOrEmpty(items[i].DueDate))
                    {
                        try
                        {
                            Convert.ToDateTime(items[i].DueDate);
                        }
                        catch
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 完工日期格式不正确！\",\"U8Code\":\"\"}";
                        }
                    }
                    List<MomSubs> subs = items[i].Subs;
                    for (int j = 0; j < subs.Count; j++)
                    {
                        if (string.IsNullOrEmpty(subs[j].cInvCode))
                        {
                            return "{\"Code\":\"400\",\"Msg\":\"产品编码[cInvCode]未传递！\",\"U8Code\":\"\"}";
                        }
                        sql = " select * from inventory (nolock) where cInvCode = '" + subs[j].cInvCode + "' ";
                        dataTable2 = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable2.Rows.Count == 0)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 物料编码[" + subs[j].cInvCode + "]不存在！\",\"U8Code\":\"\"}";
                        }
                        iQuantity = subs[j].iQuantity;
                        if (iQuantity <= 0m)
                        {
                            return "{\"Code\":\"400\",\"Msg\":\" 应领数量必须大于0！\",\"U8Code\":\"\"}";
                        }
                    }
                }
                int moIdNum = 0;
                sql = " select iFatherId,iChildId from UFSystem..UA_Identity where cAcc_Id='" + cAcc_Id + "' and cVouchType='mom_order' ";
                dataTable = U8SqlDBHelper.GetDataTable(sql);
                if (dataTable.Rows.Count > 0)
                {
                    moIdNum = Convert.ToInt32(dataTable.Rows[0]["iFatherId"]) + 1;
                    sql = " update UFSystem..UA_Identity set iFatherId=iFatherId+1,iChildId=iChildId+1  where cAcc_Id='" + cAcc_Id + "' and cVouchType='mom_order' ";
                    U8SqlDBHelper.ExecuteSql(sql);
                }
                else
                {
                    moIdNum++;
                    sql = " insert into UFSystem..UA_Identity (cAcc_Id,cVouchType,iFatherId,iChildId)  values('" + cAcc_Id + "','mom_order',0,0) ";
                    U8SqlDBHelper.ExecuteSql(sql);
                }
                string moId = "1" + $"{moIdNum:D9}";
                sql = " insert into mom_order (MoId,MoCode,CreateDate,CreateUser,vtid,cSysBarCode)  values (  '" + moId + "','" + mo.cCode + "',convert(varchar(10),getdate(),121) ,'" + userId + "','30413', '||MO21|" + mo.cCode + "') ";
                sqlList.Add(sql);
                List<RetItems> retItemsList = new List<RetItems>();
                int sortSeq = 0;
                for (int k = 0; k < items.Count; k++)
                {
                    sortSeq++;
                    int detailIdNum = 0;
                    sql = " select iFatherId,iChildId from UFSystem..UA_Identity where cAcc_Id='" + cAcc_Id + "' and cVouchType='mom_orderdetail' ";
                    dataTable = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable.Rows.Count > 0)
                    {
                        detailIdNum = Convert.ToInt32(dataTable.Rows[0]["iChildId"]) + 1;
                        sql = " update UFSystem..UA_Identity set iFatherId=iFatherId+1,iChildId=iChildId+1  where cAcc_Id='" + cAcc_Id + "' and cVouchType='mom_orderdetail' ";
                        U8SqlDBHelper.ExecuteSql(sql);
                    }
                    else
                    {
                        detailIdNum++;
                        sql = " insert into UFSystem..UA_Identity (cAcc_Id,cVouchType,iFatherId,iChildId)  values('" + cAcc_Id + "','mom_orderdetail',0,0) ";
                        U8SqlDBHelper.ExecuteSql(sql);
                    }
                    string detailId = "1" + $"{detailIdNum:D9}";
                    string lotCode = "null";
                    if (!string.IsNullOrEmpty(items[k].MoLotCode))
                    {
                        lotCode = "'" + items[k].MoLotCode + "'";
                    }
                    sql = " select * from inventory (nolock) where cInvCode = '" + items[k].cInvCode + "' ";
                    DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
                    int leadTime = BasicDAL.ToInt(dataTable3.Rows[0]["iInvAdvance"].ToString());
                    string whCode = "null";
                    if (!string.IsNullOrEmpty(dataTable3.Rows[0]["cDefWareHouse"].ToString()))
                    {
                        whCode = string.Concat("'", dataTable3.Rows[0]["cDefWareHouse"], "'");
                    }
                    string deptCode = "null";
                    if (!string.IsNullOrEmpty(items[k].cDepCode))
                    {
                        deptCode = "'" + items[k].cDepCode + "'";
                    }
                    string itemCode = "null";
                    string itemName = "null";
                    string itemClass = "null";
                    if (!string.IsNullOrEmpty(items[k].cItem_class))
                    {
                        itemClass = "'" + items[k].cItem_class + "'";
                        sql = " select * from fitem (nolock) where citem_class='" + items[k].cItem_class + "' ";
                        dataTable = U8SqlDBHelper.GetDataTable(sql);
                        string itemTable = dataTable.Rows[0]["ctable"].ToString();
                        itemCode = "'" + items[k].cItemCode + "'";
                        sql = " select * from " + itemTable + " where citemcode = '" + items[k].cItemCode + "' ";
                        dataTable = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable.Rows.Count == 0)
                        {
                            sql = " insert into " + itemTable + "(citemcode,citemname,bclose,citemccode)  values ('" + items[k].cItemCode + "','" + items[k].cItemName + "',0,'01') ";
                            U8SqlDBHelper.ExecuteSql(sql);
                            itemName = "'" + items[k].cItemName + "'";
                        }
                        else
                        {
                            itemName = string.Concat("'", dataTable.Rows[0]["citemname"], "'");
                        }
                    }
                    string moTypeId = "1000000001";
                    string partId = U8SqlDBHelper.GetString("select partid from bas_part where invcode = '" + items[k].cInvCode + "' ");
                    string bomId = "";
                    sql = " select a.* from bom_bom a (nolock) left join bom_parent b (nolock) on a.BomId = b.BomId   where exists ( select 1 from (select ParentId,max(Version) ver from bom_bom bo (nolock)               left join bom_parent pa (nolock) on bo.bomid=pa.bomid where isnull(bo.CloseUser,'')='' and isnull(RelsUser,'')<>'' and bo.Status =3 group by ParentId) z                  where b.ParentId=z.ParentId and a.Version=z.ver )   and b.parentid = '" + partId + "' ";
                    DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
                    if (dataTable4.Rows.Count > 0)
                    {
                        bomId = dataTable4.Rows[0]["BomId"].ToString();
                    }
                    sql = " insert into mom_orderdetail (MoDid,MoId,sortseq,moclass,MoTypeId,Qty,MrpQty,MoLotCode,  WhCode,mdeptcode, sotype,status,OrgStatus, BomId, partid, invcode,  sfcflag,crpflag,qcflag,RelsDate,RelsUser,RelsTime,leadtime,opscheduletype,ordflag,wiptype,  Define25 ,Define24 , Define33 ,define23,  IsWFControlled,iVerifyState,iReturnCount,remark,AuditStatus,PAllocateId,demandcode,collectiveflag,ordertype,orderdid,  ReformFlag,SourceQCVouchType,OrgQty,FmFlag,bomtype,routingtype, AlloVTid,RelsAlloVTid,cbsysbarcode,custbomid )  values (  '" + detailId + "','" + moId + "'," + sortSeq + ", " + moClass + ",'" + moTypeId + "'," + items[k].iQuantity + "," + items[k].iQuantity + ", " + lotCode + ",  " + whCode + ", " + deptCode + ", 0 ,3,2, '" + bomId + "' ,'" + partId + "','" + items[k].cInvCode + "',  0,0,0,convert(varchar(10),getdate(),121) ,'" + auditUser + "',getdate()," + leadTime + ",2,0,5,  " + itemClass + " ," + itemCode + ", " + itemName + " , '" + items[k].cdefine23 + "', 0, 0, 0, '" + items[k].Remark + "', 1, 0, null, 0, 0, 0,  0, 0, 0, 0, 1, 0, '30417', '30426', '||MO21|" + mo.cCode + "|" + sortSeq + "',0 )";
                    sqlList.Add(sql);
                    string startDate = DateTime.Now.ToString("yyyy-MM-dd");
                    string dueDate = DateTime.Now.ToString("yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(items[k].StartDate))
                    {
                        startDate = Convert.ToDateTime(items[k].StartDate).ToString("yyyy-MM-dd");
                    }
                    if (!string.IsNullOrEmpty(items[k].DueDate))
                    {
                        dueDate = Convert.ToDateTime(items[k].DueDate).ToString("yyyy-MM-dd");
                    }
                    sql = " insert into mom_morder (MoDid,MoId,StartDate,DueDate)  values ('" + detailId + "','" + moId + "','" + startDate + "','" + dueDate + "') ";
                    sqlList.Add(sql);
                    int allocateSortSeq = 0;
                    List<MomSubs> subs2 = items[k].Subs;
                    for (int l = 0; l < subs2.Count; l++)
                    {
                        int allocateIdNum = 0;
                        sql = " select iFatherId,iChildId from UFSystem..UA_Identity where cAcc_Id='" + cAcc_Id + "' and cVouchType='mom_moallocate' ";
                        dataTable = U8SqlDBHelper.GetDataTable(sql);
                        if (dataTable.Rows.Count > 0)
                        {
                            allocateIdNum = Convert.ToInt32(dataTable.Rows[0]["iChildId"]) + 1;
                            sql = " update UFSystem..UA_Identity set iFatherId=iFatherId+1,iChildId=iChildId+1  where cAcc_Id='" + cAcc_Id + "' and cVouchType='mom_moallocate' ";
                            U8SqlDBHelper.ExecuteSql(sql);
                        }
                        else
                        {
                            allocateIdNum++;
                            sql = " insert into UFSystem..UA_Identity (cAcc_Id,cVouchType,iFatherId,iChildId)  values('" + cAcc_Id + "','mom_moallocate',0,0) ";
                            U8SqlDBHelper.ExecuteSql(sql);
                        }
                        allocateSortSeq += 10;
                        BasicDAL.GetVouchId("F", cAcc_Id, "mom_moallocate");
                        allocateIdNum = BasicDAL.GetVouchId("C", cAcc_Id, "mom_moallocate");
                        string allocateId = "1" + $"{allocateIdNum:D9}";
                        string subPartId = U8SqlDBHelper.GetString("select partid from bas_part where invcode = '" + subs2[l].cInvCode + "'");
                        sql = " select a.BomId,c.InvCode,a.Version,f.cInvCode,d.BaseQtyN,d.BaseQtyD,d.CompScrap,b.ParentScrap,op.WIPType,op.Whcode,d.OpComponentId  from bom_bom a (nolock) left join bom_parent b (nolock) on a.BomId = b.BomId  left join Bas_part c (nolock) on c.partid = b.parentid left join bom_opcomponent d (nolock) on a.BomId=d.BomId   left join bom_opcomponentopt op (nolock) on op.OptionsId = d.OptionsId  left join bas_part e (nolock) on e.PartId = d.ComponentId left join Inventory f (nolock) on e.InvCode=f.cInvCode   where c.InvCode = '" + items[k].cInvCode + "' and isnull(a.CloseUser,'')='' and isnull(RelsUser,'')<>'' and a.Status =3   and exists ( select 1 from (select ParentId,max(Version) ver from bom_bom bo (nolock)  left join bom_parent pa (nolock) on bo.bomid=pa.bomid where isnull(bo.CloseUser,'')='' and isnull(RelsUser,'')<>'' and bo.Status =3 group by ParentId) z   where b.ParentId=z.ParentId and a.Version=z.ver ) and e.InvCode = '" + subs2[l].cInvCode + "' ";
                        DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
                        decimal baseQtyN = 1m;
                        decimal baseQtyD = 1m;
                        decimal parentScrap = default(decimal);
                        decimal compScrap = default(decimal);
                        string subWhCode = "null";
                        int wipType = 3;
                        int opComponentId = 0;
                        if (dataTable5.Rows.Count > 0)
                        {
                            baseQtyN = BasicDAL.ToDec(dataTable5.Rows[0]["BaseQtyN"].ToString());
                            baseQtyD = BasicDAL.ToDec(dataTable5.Rows[0]["BaseQtyD"].ToString());
                            parentScrap = BasicDAL.ToDec(dataTable5.Rows[0]["ParentScrap"].ToString());
                            compScrap = BasicDAL.ToDec(dataTable5.Rows[0]["CompScrap"].ToString());
                            if (!string.IsNullOrEmpty(dataTable5.Rows[0]["Whcode"].ToString()))
                            {
                                subWhCode = string.Concat("'", dataTable5.Rows[0]["Whcode"], "'");
                            }
                            opComponentId = BasicDAL.ToInt(dataTable5.Rows[0]["OpComponentId"].ToString());
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(subs2[l].BaseQtyD))
                            {
                                baseQtyD = BasicDAL.ToDec(subs2[l].BaseQtyD);
                            }
                            baseQtyN = (string.IsNullOrEmpty(subs2[l].BaseQtyN) ? Math.Round(subs2[l].iQuantity / items[k].iQuantity / baseQtyD, 2) : BasicDAL.ToDec(subs2[l].BaseQtyN));
                        }
                        sql = " insert into mom_moallocate (allocateid,modid,sortseq,opseq,componentid,fvflag,BaseQtyN,BaseQtyD,  ParentScrap,CompScrap,Qty,IssQty,DeclaredQty,StartDemDate,enddemdate,  whcode,lotno,wiptype,byproductflag,qcflag,offset, invcode,opcomponentid,  replenishqty,transqty,producttype,sotype,qmflag,orgqty,orgauxqty, RequisitionFlag,RequisitionQty,RequisitionIssQty,CostWIPRel,uppermoqty,invalloeflag,cSubSysBarCode  ) values (  '" + allocateId + "','" + detailId + "'," + allocateSortSeq + ",'0000','" + subPartId + "',1, " + baseQtyN + "," + baseQtyD + ",  " + parentScrap + "," + compScrap + "," + subs2[l].iQuantity + ",0,0, '" + startDate + "','" + dueDate + "',  " + subWhCode + ",null, " + wipType + ", 0, 0,0, '" + subs2[l].cInvCode + "'," + opComponentId + ",  0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '||MO21|" + mo.cCode + "|" + sortSeq + "|" + allocateSortSeq + "' ) ";
                        sqlList.Add(sql);
                    }
                    RetItems retItems = new RetItems();
                    retItems.Autoid = "0";
                    retItems.RowNo = 0;
                    retItems.cInvCode = items[k].cInvCode;
                    retItems.U8ID = detailId;
                    retItems.U8RowNo = sortSeq;
                    retItemsList.Add(retItems);
                }
                int executeResult = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (executeResult > 0)
                {
                    string jsonResult = JsonConvert.SerializeObject(retItemsList);
                    return "{\"Code\":\"200\",\"Msg\":\"工单创建成功！\",\"U8Code\":\"" + mo.cCode + "\",\"Items\": " + jsonResult + "}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单创建失败！\",\"U8Code\":\"" + mo.cCode + "\",\"Items\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        public static string CloseMom(MomOrder mo)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(mo.cCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[cCode]未传递！\",\"U8Code\":\" }";
                }
                sql = " select * from mom_order where MoCode = '" + mo.cCode + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + mo.cCode + "]不存在！\",\"U8Code\":\"\"}";
                }
                string moId = dt.Rows[0]["MoId"].ToString();
                sql = " select 1 from mom_orderdetail where moid='" + moId + "' and isnull(CloseUser,'')='' ";
                dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + mo.cCode + "]已是关闭状态！\",\"U8Code\":\"\"}";
                }
                if (string.IsNullOrEmpty(mo.CloseUser))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"关闭人[CloseUser]未传递！\",\"U8Code\":\" }";
                }
                if (string.IsNullOrEmpty(mo.CloseTime))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"关闭日期[CloseTime]未传递！\",\"U8Code\":\"\"}";
                }
                DateTime closeDate;
                try
                {
                    closeDate = Convert.ToDateTime(mo.CloseTime);
                }
                catch
                {
                    return "{\"Code\":\"400\",\"Msg\":\"开票日期[" + mo.CloseTime + "]格式错误！\",\"U8Code\":\"\"}";
                }
                sql = string.Concat("update mom_orderdetail set CloseUser='", mo.CloseUser, "',CloseTime='", closeDate, "',Status='4' where MoId='", moId, "' ");
                sqlList.Add(sql);
                int affectedRows = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (affectedRows > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"工单关闭成功！\",\"U8Code\":\"" + mo.cCode + "\",\"Items\":\"\" }";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单关闭失败！\",\"U8Code\":\"" + mo.cCode + "\",\"Items\":\"\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        public static string AddMoall(Moallocate mo)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(mo.U8ID))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[U8ID]未传递！\"}";
                }
                if (string.IsNullOrEmpty(mo.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"建档人[cMaker]未传递！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                sql = " select * from mom_orderdetail (nolock) where MoDId = '" + mo.U8ID + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[" + mo.U8ID + "]无数据！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                decimal qualifiedInQty = BasicDAL.ToDec(dt.Rows[0]["QualifiedInQty"].ToString());
                if (qualifiedInQty > 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[" + mo.U8ID + "]已入库，不可新增子件！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                List<Moallocates> items = mo.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"产品编码[cInvCode]未传递！\",\"U8Code\":\"\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                    sql = " select * from inventory (nolock) where cInvCode = '" + items[i].cInvCode + "' ";
                    DataTable dtInventory = U8SqlDBHelper.GetDataTable(sql);
                    if (dtInventory.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 物料编码[" + items[i].cInvCode + "]不存在！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                    sql = " select 1 from mom_moallocate where MoDId = '" + mo.U8ID + "' and InvCode='" + items[i].cInvCode + "' ";
                    DataTable dtAllocate = U8SqlDBHelper.GetDataTable(sql);
                    if (dtAllocate.Rows.Count > 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 工单明细ID[" + mo.U8ID + "]已存在物料[" + items[i].cInvCode + "]的数据。不可重复添加！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                    decimal iQuantity = items[i].iQuantity;
                    if (iQuantity <= 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 应领数量必须大于0！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                }
                int sortSeq = BasicDAL.ToInt(U8SqlDBHelper.GetString("select max(sortseq) from mom_moallocate where MoDId = '" + mo.U8ID + "'"));
                string startDate = DateTime.Now.ToString("yyyy-MM-dd");
                string dueDate = DateTime.Now.ToString("yyyy-MM-dd");
                sql = " select * from mom_morder where MoDid = '" + mo.U8ID + "' ";
                DataTable dtMorder = U8SqlDBHelper.GetDataTable(sql);
                if (dtMorder.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dtMorder.Rows[0]["StartDate"].ToString()))
                    {
                        startDate = dtMorder.Rows[0]["StartDate"].ToString();
                    }
                    if (!string.IsNullOrEmpty(dtMorder.Rows[0]["DueDate"].ToString()))
                    {
                        dueDate = dtMorder.Rows[0]["DueDate"].ToString();
                    }
                }
                string moCode = U8SqlDBHelper.GetString(string.Concat("select MoCode from mom_order where MoId ='", dt.Rows[0]["MoId"], "' "));
                for (int j = 0; j < items.Count; j++)
                {
                    int nextChildId = 0;
                    sql = " select iFatherId,iChildId from UFSystem..UA_Identity where cAcc_Id='" + cAcc_Id + "' and cVouchType='mom_moallocate' ";
                    DataTable dtIdentity = U8SqlDBHelper.GetDataTable(sql);
                    if (dtIdentity.Rows.Count > 0)
                    {
                        nextChildId = Convert.ToInt32(dtIdentity.Rows[0]["iChildId"]) + 1;
                        sql = " update UFSystem..UA_Identity set iFatherId=iFatherId+1,iChildId=iChildId+1  where cAcc_Id='" + cAcc_Id + "' and cVouchType='mom_moallocate' ";
                        U8SqlDBHelper.ExecuteSql(sql);
                    }
                    else
                    {
                        nextChildId++;
                        sql = " insert into UFSystem..UA_Identity (cAcc_Id,cVouchType,iFatherId,iChildId)  values('" + cAcc_Id + "','mom_moallocate',0,0) ";
                        U8SqlDBHelper.ExecuteSql(sql);
                    }
                    sortSeq += 10;
                    string allocateId = "1" + $"{nextChildId:D9}";
                    string partId = U8SqlDBHelper.GetString("select partid from bas_part where invcode = '" + items[j].cInvCode + "'");
                    sql = string.Concat(" select a.BomId,c.InvCode,a.Version,f.cInvCode,d.BaseQtyN,d.BaseQtyD,d.CompScrap,b.ParentScrap,op.WIPType,op.Whcode,d.OpComponentId  from bom_bom a (nolock) left join bom_parent b (nolock) on a.BomId = b.BomId  left join Bas_part c (nolock) on c.partid = b.parentid left join bom_opcomponent d (nolock) on a.BomId=d.BomId   left join bom_opcomponentopt op (nolock) on op.OptionsId = d.OptionsId  left join bas_part e (nolock) on e.PartId = d.ComponentId left join Inventory f (nolock) on e.InvCode=f.cInvCode   where c.InvCode = '", dt.Rows[0]["InvCode"], "' and isnull(a.CloseUser,'')='' and isnull(RelsUser,'')<>'' and a.Status =3   and exists ( select 1 from (select ParentId,max(Version) ver from bom_bom bo (nolock)  left join bom_parent pa (nolock) on bo.bomid=pa.bomid where isnull(bo.CloseUser,'')='' and isnull(RelsUser,'')<>'' and bo.Status =3 group by ParentId) z   where b.ParentId=z.ParentId and a.Version=z.ver ) and e.InvCode = '", items[j].cInvCode, "' ");
                    DataTable dtBom = U8SqlDBHelper.GetDataTable(sql);
                    decimal baseQtyN = 1m;
                    decimal baseQtyD = 1m;
                    decimal parentScrap = default(decimal);
                    decimal compScrap = default(decimal);
                    string whCode = "null";
                    int wipType = 3;
                    int opComponentId = 0;
                    if (dtBom.Rows.Count > 0)
                    {
                        baseQtyN = BasicDAL.ToDec(dtBom.Rows[0]["BaseQtyN"].ToString());
                        baseQtyD = BasicDAL.ToDec(dtBom.Rows[0]["BaseQtyD"].ToString());
                        parentScrap = BasicDAL.ToDec(dtBom.Rows[0]["ParentScrap"].ToString());
                        compScrap = BasicDAL.ToDec(dtBom.Rows[0]["CompScrap"].ToString());
                        if (!string.IsNullOrEmpty(dtBom.Rows[0]["Whcode"].ToString()))
                        {
                            whCode = string.Concat("'", dtBom.Rows[0]["Whcode"], "'");
                        }
                        opComponentId = BasicDAL.ToInt(dtBom.Rows[0]["OpComponentId"].ToString());
                    }
                    else
                    {
                        baseQtyN = (string.IsNullOrEmpty(items[j].BaseQtyN) ? Math.Round(items[j].iQuantity / BasicDAL.ToDec(dt.Rows[0]["InvCode"].ToString()), 2) : BasicDAL.ToDec(items[j].BaseQtyN));
                        if (!string.IsNullOrEmpty(items[j].BaseQtyD))
                        {
                            baseQtyD = BasicDAL.ToDec(items[j].BaseQtyD);
                        }
                    }
                    sql = string.Concat(" insert into mom_moallocate (allocateid,modid,sortseq,opseq,componentid,fvflag,BaseQtyN,BaseQtyD,  ParentScrap,CompScrap,Qty,IssQty,DeclaredQty,StartDemDate,enddemdate,  whcode,lotno,wiptype,byproductflag,qcflag,offset, invcode,opcomponentid,  replenishqty,transqty,producttype,sotype,qmflag,orgqty,orgauxqty, RequisitionFlag,RequisitionQty,RequisitionIssQty,CostWIPRel,uppermoqty,invalloeflag,cSubSysBarCode  ) values (  '", allocateId, "','", mo.U8ID, "',", sortSeq, ",'0000','", partId, "',1, ", baseQtyN, ",", baseQtyD, ",  ", parentScrap, ",", compScrap, ",", items[j].iQuantity, ",0,0, '", startDate, "','", dueDate, "',  ", whCode, ",null, ", wipType, ", 0, 0,0, '", items[j].cInvCode, "',", opComponentId, ",  0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '||MO21|", moCode, "|", dt.Rows[0]["SortSeq"], "|", sortSeq, "' ) ");
                    sqlList.Add(sql);
                }
                int affectedRows = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (affectedRows > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"工单子件创建成功！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单子件创建失败！\",\"U8Code\":\"" + mo.U8ID + "\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        public static string DeleteMom(MomOrder mo)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(mo.U8Code))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单号[U8Code]未传递！\",\"U8Code\":\"\"}";
                }
                sql = " select * from mom_order where MoCode = '" + mo.U8Code + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单号[" + mo.U8Code + "]无数据！\",\"U8Code\":\"\"}";
                }
                sql = string.Concat(" select * from mom_orderdetail where MoId = '", dt.Rows[0]["MoId"], "' and isnull(CloseUser,'')<>''  ");
                DataTable dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单号[" + mo.U8Code + "]已关闭，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = string.Concat(" select * from mom_orderdetail a where MoId = '", dt.Rows[0]["MoId"], "' and exists (select 1 from mom_moallocate b where a.modid=b.modid and isnull(IssQty,0)>0 )  ");
                dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单号[" + mo.U8Code + "]已领料，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = string.Concat(" select * from mom_orderdetail where MoId = '", dt.Rows[0]["MoId"], "' and isnull(QualifiedInQty,0) > 0 ");
                dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单号[" + mo.U8Code + "]已入库，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = string.Concat(" select * from mom_orderdetail a where MoId = '", dt.Rows[0]["MoId"], "' and exists (select 1 from mom_moallocate b where a.modid=b.modid and isnull(RequisitionIssQty,0)>0 )  ");
                dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单号[" + mo.U8Code + "]已申请领料，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = string.Concat(" select * from mom_orderdetail where MoId = '", dt.Rows[0]["MoId"], "' and isnull(DeclaredQty,0) > 0 ");
                dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单号[" + mo.U8Code + "]已入库，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = string.Concat(" delete from mom_moallocate where modid in (select modid from mom_orderdetail where MoId = '", dt.Rows[0]["MoId"], "') ");
                sqlList.Add(sql);
                sql = string.Concat(" delete from mom_morder where moid = '", dt.Rows[0]["MoId"], "' ");
                sqlList.Add(sql);
                sql = string.Concat(" delete from mom_orderdetail where moid = '", dt.Rows[0]["MoId"], "' ");
                sqlList.Add(sql);
                sql = " delete from mom_order where MoCode = '" + mo.U8Code + "' ";
                sqlList.Add(sql);
                int affectedRows = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (affectedRows > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"工单删除成功！\",\"U8Code\":\"" + mo.cCode + "\"}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单删除失败！\",\"U8Code\":\"" + mo.cCode + "\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        public static string ModifyMom(MomOrder mo)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(mo.U8Code))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单号[U8Code]未传递！\",\"U8Code\":\"\"}";
                }
                sql = " select * from mom_order where mocode = '" + mo.U8Code + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单号[" + mo.U8Code + "]不存在！\",\"U8Code\":\"\"}";
                }
                List<MomOrders> items = mo.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].U8ID))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[U8ID]未传递！\",\"U8Code\":\"\"}";
                    }
                    sql = " select * from mom_orderdetail where MoDId = '" + items[i].U8ID + "'  ";
                    DataTable dtDetail = U8SqlDBHelper.GetDataTable(sql);
                    if (dtDetail.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[" + items[i].U8ID + "]无数据！\",\"U8Code\":\"\"}";
                    }
                    if (dtDetail.Rows[0]["CloseUser"].ToString() != "")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[" + items[i].U8ID + "]已关闭，不可变更！\",\"U8Code\":\"\"}";
                    }
                    sql = " select 1 from mom_moallocate where modid='" + items[i].U8ID + "' and isnull(IssQty,0)>0   ";
                    DataTable dtAllocate = U8SqlDBHelper.GetDataTable(sql);
                    if (dtAllocate.Rows.Count > 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[" + items[i].U8ID + "]已领料，不可变更！\",\"U8Code\":\"\"}";
                    }
                    if (BasicDAL.ToDec(dtDetail.Rows[0]["QualifiedInQty"].ToString()) > 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"U8工单号[" + mo.U8Code + "]已入库，不可变更！\",\"U8Code\":\"\"}";
                    }
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"产品编码[cInvCode]未传递！\",\"U8Code\":\"\"}";
                    }
                    sql = " select * from inventory (nolock) where cInvCode = '" + items[i].cInvCode + "' ";
                    DataTable dtInventory = U8SqlDBHelper.GetDataTable(sql);
                    if (dtInventory.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 产品编码[" + items[i].cInvCode + "]不存在！\",\"U8Code\":\"\"}";
                    }
                    string bSelf = dtInventory.Rows[0]["bSelf"].ToString();
                    if (bSelf == "False")
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 产品编码[" + items[i].cInvCode + "]不是自制产品！\",\"U8Code\":\"\"}";
                    }
                    decimal iQuantity = items[i].iQuantity;
                    if (iQuantity <= 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 生产数量必须大于0！\",\"U8Code\":\"\"}";
                    }
                }
                for (int j = 0; j < items.Count; j++)
                {
                    sql = " update mom_orderdetail set InvCode = '" + items[j].cInvCode + "',Qty=" + items[j].iQuantity + " where MoDId = '" + items[j].U8ID + "' ";
                    sqlList.Add(sql);
                }
                int affectedRows = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (affectedRows > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"工单修改成功！\",\"U8Code\":\"" + mo.U8Code + "\"}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单修改失败！\",\"U8Code\":\"" + mo.U8Code + "\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        public static string DeleteMoall(Moallocate mo)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(mo.U8ID))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单明细ID[U8ID]未传递！\",\"U8Code\":\"\"}";
                }
                if (string.IsNullOrEmpty(mo.cInvCode))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"子件物料编码[cInvCode]未传递！\",\"U8Code\":\"\"}";
                }
                sql = " select 1 from mom_moallocate where MoDId = '" + mo.U8ID + "' and InvCode = '" + mo.cInvCode + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单明细[" + mo.U8ID + "]物料[" + mo.cInvCode + "]无数据！\",\"U8Code\":\"\"}";
                }
                sql = " select * from mom_orderdetail where MoDId = '" + mo.U8ID + "' and isnull(CloseUser,'')<>''  ";
                DataTable dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单明细[" + mo.U8ID + "]已关闭，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = " select 1 from mom_moallocate where MoDId = '" + mo.U8ID + "' and InvCode = '" + mo.cInvCode + "' and isnull(IssQty,0)>0 ";
                dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"物料[" + mo.cInvCode + "]已领料，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = " select * from mom_orderdetail where MoDId = '" + mo.U8ID + "' and isnull(QualifiedInQty,0) > 0 ";
                dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"U8工单明细[" + mo.U8ID + "]已入库，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = " select 1 from mom_moallocate where MoDId = '" + mo.U8ID + "' and InvCode = '" + mo.cInvCode + "' and  and isnull(RequisitionIssQty,0)>0 ";
                dtCheck = U8SqlDBHelper.GetDataTable(sql);
                if (dtCheck.Rows.Count > 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"物料[" + mo.cInvCode + "]已申请领料，不可删除！\",\"U8Code\":\"\"}";
                }
                sql = " delete from mom_moallocate where MoDId = '" + mo.U8ID + "' and InvCode = '" + mo.cInvCode + "' ";
                sqlList.Add(sql);
                int affectedRows = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (affectedRows > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"工单创建成功！\",\"U8Code\":\"" + mo.U8ID + "\"}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单创建失败！\",\"U8Code\":\"" + mo.U8ID + "\"}";
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, sql);
                string errorMsg = "接口请求失败！原因：" + ex.Message;
                return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\",\"Items\":\"\"}";
            }
        }

        public static string ModifyMoall(Moallocate mo)
        {
            List<string> sqlList = new List<string>();
            string sql = "";
            try
            {
                if (string.IsNullOrEmpty(mo.U8ID))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[U8ID]未传递！\"}";
                }
                if (string.IsNullOrEmpty(mo.cMaker))
                {
                    return "{\"Code\":\"400\",\"Msg\":\"建档人[cMaker]未传递！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                sql = " select * from mom_orderdetail (nolock) where MoDId = '" + mo.U8ID + "' ";
                DataTable dt = U8SqlDBHelper.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[" + mo.U8ID + "]无数据！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                if (dt.Rows[0]["CloseUser"].ToString() != "")
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[" + mo.U8ID + "]已关闭，不可修改！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                decimal qualifiedInQty = BasicDAL.ToDec(dt.Rows[0]["QualifiedInQty"].ToString());
                if (qualifiedInQty > 0m)
                {
                    return "{\"Code\":\"400\",\"Msg\":\"工单明细ID[" + mo.U8ID + "]已入库，不可新增子件！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                List<Moallocates> items = mo.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.IsNullOrEmpty(items[i].cInvCode))
                    {
                        return "{\"Code\":\"400\",\"Msg\":\"产品编码[cInvCode]未传递！\",\"U8Code\":\"\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                    sql = " select * from inventory (nolock) where cInvCode = '" + items[i].cInvCode + "' ";
                    DataTable dtInventory = U8SqlDBHelper.GetDataTable(sql);
                    if (dtInventory.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 物料编码[" + items[i].cInvCode + "]不存在！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                    sql = " select * from mom_moallocate where MoDId = '" + mo.U8ID + "' and InvCode='" + items[i].cInvCode + "' ";
                    DataTable dtAllocate = U8SqlDBHelper.GetDataTable(sql);
                    if (dtAllocate.Rows.Count == 0)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 工单明细ID[" + mo.U8ID + "]不存在物料[" + items[i].cInvCode + "]的数据！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                    if (BasicDAL.ToDec(dtAllocate.Rows[0]["IssQty"].ToString()) > 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 工单明细ID[" + mo.U8ID + "]物料[" + items[i].cInvCode + "]已领料出库，不可变更！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                    decimal iQuantity = items[i].iQuantity;
                    if (iQuantity <= 0m)
                    {
                        return "{\"Code\":\"400\",\"Msg\":\" 应领数量必须大于0！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                    }
                }
                for (int j = 0; j < items.Count; j++)
                {
                    sql = " update mom_moallocate set Qty = " + items[j].iQuantity + " ";
                    if (!string.IsNullOrEmpty(items[j].cWhCode))
                    {
                        sql = sql + " ,WhCode= '" + items[j].cWhCode + "' ";
                    }
                    if (!string.IsNullOrEmpty(items[j].BaseQtyN))
                    {
                        sql = sql + " ,BaseQtyN = " + BasicDAL.ToDec(items[j].BaseQtyN) + " ";
                    }
                    if (!string.IsNullOrEmpty(items[j].BaseQtyD))
                    {
                        sql = sql + " ,BaseQtyD = " + BasicDAL.ToDec(items[j].BaseQtyD) + " ";
                    }
                    sql = sql + " where MoDId = '" + mo.U8ID + "' and InvCode='" + items[j].cInvCode + "' ";
                    sqlList.Add(sql);
                }
                int affectedRows = U8SqlDBHelper.ExecuteSqlTran(sqlList);
                if (affectedRows > 0)
                {
                    return "{\"Code\":\"200\",\"Msg\":\"工单子件修改成功！\",\"U8ID\":\"" + mo.U8ID + "\"}";
                }
                return "{\"Code\":\"400\",\"Msg\":\"工单子件修改失败！\",\"U8Code\":\"" + mo.U8ID + "\"}";
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
