using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{

public class VouchsDAL
{
	private static string zt = ConfigurationManager.AppSettings["zt"].ToString();

	private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();

	public static string Get_Vendor(VouchsItem VI)
	{
		try
		{
			string safeSql = (" select cVenCode,cVenName,cVenAbbName,cVenAddress,cVenPerson,cVenIAddress,bVenCargo,bProxyForeign,bVenService \r\n from Vendor (nolock)  \r\n where ( ( convert(varchar(19),dVenCreateDatetime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dVenCreateDatetime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) ) \r\n or ( convert(varchar(19),dModifyDate,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dModifyDate,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n") ?? "";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_Customer(VouchsItem VI)
	{
		try
		{
			string safeSql = " select cCusCode,cCusName,cCusAbbName,cCusAddress,cCusPerson,cCusOAddress \r\n from customer (nolock)  \r\n where ( ( convert(varchar(19),dCusCreateDatetime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dCusCreateDatetime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) ) \r\n or ( convert(varchar(19),dModifyDate,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dModifyDate,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n  ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_Iventory(VouchsItem VI)
	{
		try
		{
			string safeSql = " select cInvCode,cInvName,cInvStd,a.cInvCCode ,b.cInvCName,a.cComUnitCode cUnitCode,c.cComUnitName cUnit,ud2.cAlias cInvDefine2,ud3.cAlias cInvDefine3 \r\n from inventory a (nolock)  \r\n left join InventoryClass b (nolock) on a.cinvccode=b.cinvccode \r\n left join ComputationUnit c (nolock) on a.cComUnitCode=c.cComUnitCode \r\n left join Inventory_sub d (nolock) on a.cInvCode = d.cInvSubCode \r\n left join UserDefine ud2 (nolock) on a.cInvDefine2=ud2.cValue and ud2.cID = 18  \r\n left join UserDefine ud3 (nolock) on a.cInvDefine3=ud3.cValue and ud3.cID = 19 \r\n where ( ( convert(varchar(19),dInvCreateDatetime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dInvCreateDatetime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dModifyDate,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dModifyDate,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cInvDefine2,'') <> '' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_ArrivalVouchs(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and a.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and a.cWhCode <> '03' ";
			}
			string safeSql = " select a.cAVCode cCode,a.irowno RowNo,a.Autoid,b.cDepCode,c.cDepName,null cVenCode,null cVenName,null cCusCode,null cCusName, \r\n a.igroupno iGroup,a.bAVType, CONVERT(varchar(10),b.dAVDate,121) dDate,b.dnverifytime dTime,CONVERT(varchar(10),b.dVerifyDate,121) dVeriDate, \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iAVQuantity as decimal(20,4)) iQuantity,b.cAVMemo cMemo,a.cbMemo,null cRdCode,a.cWhCode,e.cWhName, \r\n g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7, \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16  \r\n from AssemVouchs a (nolock)  left join AssemVouch b (nolock) on a.ID=b.ID   \r\n left join department c (nolock) on b.cDepCode = c.cDepCode   \r\n left join Warehouse e (nolock) on a.cwhcode=e.cwhcode  \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnverifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnverifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(b.cVerifyPerson,'') <> '' and b.cvouchtype = '15' \r\n" + whereClause + " order by Autoid ";
		DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_QMArrivalVouchs(VouchsItem VI)
	{
            string sql = "";
            string whereClause = "";
            try
		{
			string resultId = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				resultId = resultId + " and cCode = '" + VI.cCode + "' ";
			}
			if (!string.IsNullOrEmpty(VI.cWhCode))
			{
				resultId = resultId + " and a.cWhCode = '" + VI.cWhCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				resultId += " and a.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				resultId += " and a.cWhCode <> '03' ";
			}
			sql = " select cCode,ivouchrowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,null cCusCode,null cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,cMakeTime dTime,CONVERT(varchar(10),b.caudittime,121) dVeriDate,a.cWhCode,e.cWhName, \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.fValidQuantity-isnull(rds.rqty,0) as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cBusType cRdCode,i.ID checkid, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7, \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from PU_ArrivalVouchs a (nolock)  left join PU_ArrivalVouch b (nolock) on a.ID=b.ID \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join Warehouse e (nolock) on a.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n left join QMCHECKVOUCHER i (nolock) on a.autoid=i.SOURCEAUTOID \r\n left join (select iArrsId,sum(iQuantity) rqty from rdrecords01 (nolock) group by iArrsId) rds on a.Autoid=rds.iArrsId \r\n where ( ( convert(varchar(19),cMakeTime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),cMakeTime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  \r\n or ( convert(varchar(19),cModifyTime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),cModifyTime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),DMAKETIME,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),DMAKETIME,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and exists ( select 1 from QMCHECKVOUCHER qm (nolock) where qm.csource = '到货单' and qm.SOURCEAUTOID = a.autoid  )  and a.fValidQuantity > isnull(rds.rqty,0) \r\n" + resultId + " order by autoid ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_MomOrder(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and b.MoCode = '" + VI.cCode + "' ";
			}
			string safeSql = " select b.MoCode+'-'+convert(varchar(4),SortSeq) cCode,SortSeq RowNo,a.MoDId Autoid,a.MDeptCode cDepCode,c.cDepName,null cVenCode,null cVenName,null cCusCode,null cCusName, \r\n CONVERT(varchar(10),b.CreateDate,121) dDate,CreateTime dTime,CONVERT(varchar(10),a.RelsTime,121) dVeriDate,a.WhCode,e.cWhName, \r\n a.InvCode,f.cInvName,f.cInvStd,cast(a.Qty as decimal(20,4)) iQuantity,null cMemo,a.Remark cbMemo,null cRdCode,null checkid, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit,a.OrderCode cSoCode,QcFlag \r\n from mom_orderdetail a (nolock) left join mom_order b (nolock) on a.MoId = b.MoId  \r\n left join Department c (nolock) on a.MDeptCode = c.cdepcode \r\n left join Warehouse e (nolock) on a.WhCode=e.cwhcode  \r\n left join inventory f (nolock) on a.InvCode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),a.RelsTime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),a.RelsTime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  \r\n or ( convert(varchar(19),b.ModifyTime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),b.ModifyTime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(RelsUser,'') <> ''  and not exists ( select 1 from QMCHECKVOUCHER qm (nolock) where qm.csource = '生产订单' and qm.SOURCEAUTOID = a.MoDId   )  \r\n" + whereClause + " order by a.MoDId ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_QMMomOrder(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and b.MoCode = '" + VI.cCode + "' ";
			}
			string safeSql = " select b.MoCode+'-'+convert(varchar(4),SortSeq) cCode,SortSeq RowNo,a.MoDId Autoid,a.MDeptCode cDepCode,c.cDepName,null cVenCode,null cVenName,null cCusCode,null cCusName, \r\n CONVERT(varchar(10),b.CreateDate,121) dDate,CreateTime dTime,CONVERT(varchar(10),a.RelsTime,121) dVeriDate,a.WhCode,e.cWhName, \r\n a.InvCode,f.cInvName,f.cInvStd,cast(qms.qmqty-isnull(rds.rqty,0) as decimal(20,4)) iQuantity,null cMemo,a.Remark cbMemo,null cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit,a.OrderCode cSoCode,QcFlag \r\n from mom_orderdetail a (nolock) left join mom_order b (nolock) on a.MoId = b.MoId  \r\n left join Department c (nolock) on a.MDeptCode = c.cdepcode \r\n left join Warehouse e (nolock) on a.WhCode=e.cwhcode  \r\n left join inventory f (nolock) on a.InvCode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n left join (select iMPoIds,sum(iQuantity) rqty from rdrecords10 (nolock) group by iMPoIds) rds on a.MoDId=rds.iMPoIds \r\n left join (select SOURCEAUTOID,sum(FREGQUANTITY) qmqty from QMCHECKVOUCHER (nolock) group by SOURCEAUTOID ) qms on a.MoDId=qms.SOURCEAUTOID \r\n where ( ( convert(varchar(19),a.RelsTime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),a.RelsTime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) ) \r\n or ( convert(varchar(19),b.ModifyTime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),b.ModifyTime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) ) \r\n or ( convert(varchar(19),b.CreateTime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),b.CreateTime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(RelsUser,'') <> ''  and exists ( select 1 from QMCHECKVOUCHER qm (nolock) where qm.csource = '生产订单' and qm.SOURCEAUTOID = a.MoDId   )  \r\n" + whereClause + " order by a.MoDId ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_RedRD01(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,ABS(cast(a.iquantity as decimal(20,4))) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords01 a (nolock) left join RdRecord01 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and a.iQuantity < 0 " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_RD11(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (!string.IsNullOrEmpty(VI.cWhCode))
			{
				whereClause = whereClause + " and b.cWhCode = '" + VI.cWhCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords11 a (nolock) left join RdRecord11 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and a.iQuantity > 0 " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_RedRD11(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,ABS(cast(a.iquantity as decimal(20,4))) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords11 a (nolock) left join RdRecord11 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and a.iQuantity < 0 " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_RD10(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,a.cMoCode,a.iordercode cSoCode,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords10 a (nolock) left join RdRecord10 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and a.iQuantity > 0 " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_RedRD10(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,ABS(cast(a.iquantity as decimal(20,4))) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords10 a (nolock) left join RdRecord10 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and a.iQuantity < 0 " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_RD08(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords08 a (nolock) left join RdRecord08 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and b.csource <> '拆卸' and b.csource <> '形态转换' " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_RD09(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords09 a (nolock) left join RdRecord09 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and b.csource <> '拆卸' and b.csource <> '形态转换'  " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_TransVouchs(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and b.cTVCode = '" + VI.cCode + "' ";
			}
			string safeSql = " select b.cTVCode cCode,irowno RowNo,Autoid,cODepCode,c.cDepName C0DepName,cIDepCode, d.cDepName CIDepName,  \r\n '' cVenCode, '' cVenName,'' cCusCode,'' cCusName, convert(varchar(10),b.dTVDate,121) dDate,dnmaketime dTime, \r\n convert(varchar(10),b.dVerifyDate,121) dVeriDate,cOWhCode,cIWhCode,e.cWhName cOWhName,f.cWhName cIWhName, \r\n a.cInvCode,g.cInvName,g.cInvStd,a.iTVQuantity iQuantity,b.cTVMemo cMemo ,a.cbMemo cbMemo,b.cORdCode,b.cIRdCode, \r\n g.cComUnitCode cUnitCode,h.cComUnitName cUnit,i.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7, \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from TransVouchs a (nolock) left join TransVouch b (nolock)  on a.ID=b.ID \r\n left join Department c (nolock) on b.cODepCode=c.cDepCode \r\n left join Department d (nolock) on b.cIDepCode=d.cDepCode \r\n left join Warehouse e (nolock) on cOWhCode=e.cwhcode \r\n left join Warehouse f (nolock) on cIWhCode=f.cwhcode \r\n left join inventory g (nolock) on a.cinvcode=g.cinvcode \r\n left join ComputationUnit h (nolock) on g.cComUnitCode=h.cComunitCode  \r\n left join ComputationUnit i (nolock) on g.cAssComUnitCode=i.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cVerifyPerson ,'') = '' " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_DisPatch(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cDLCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and a.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and a.cWhCode <> '03' ";
			}
			string safeSql = " select cDLCode cCode,irowno RowNo,iDLsID Autoid,b.cDepCode,c.cDepName,null cVenCode,null cVenName,b.cCusCode,cCusName,   \r\n CONVERT(varchar(10),b.dDate,121) dDate,b.dcreatesystime dTime,null dVeriDate,a.cWhCode,e.cWhName,a.cSoCode,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cMemo cbMemo,b.cBusType cRdCode,  \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16  \r\n from DispatchLists a (nolock)  left join DispatchList b (nolock) on a.DLID=b.DLID  \r\n left join department c (nolock) on b.cDepCode = c.cDepCode  \r\n left join Warehouse e (nolock) on a.cwhcode=e.cwhcode  \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode  \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode  \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode  \r\n where ( ( convert(varchar(19),dcreatesystime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dcreatesystime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  \r\n or ( convert(varchar(19),dmodifysystime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dmodifysystime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  )  \r\n and isnull(b.cVerifier,'') = '' and bReturnFlag = 0 \r\n" + whereClause + " order by iDLsID ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_ReDisPatch(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cDLCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and a.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and a.cWhCode <> '03' ";
			}
			string safeSql = " select cDLCode cCode,irowno RowNo,iDLsID Autoid,b.cDepCode,c.cDepName,null cVenCode,null cVenName,b.cCusCode,cCusName,   \r\n CONVERT(varchar(10),b.dDate,121) dDate,b.dcreatesystime dTime,null dVeriDate,a.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,ABS(cast(a.iquantity as decimal(20,4))) iQuantity,b.cMemo,a.cMemo cbMemo,b.cBusType cRdCode,  \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16  \r\n from DispatchLists a (nolock)  left join DispatchList b (nolock) on a.DLID=b.DLID  \r\n left join department c (nolock) on b.cDepCode = c.cDepCode  \r\n left join Warehouse e (nolock) on a.cwhcode=e.cwhcode  \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode  \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode  \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode  \r\n where ( ( convert(varchar(19),dcreatesystime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dcreatesystime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  \r\n or ( convert(varchar(19),dmodifysystime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dmodifysystime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  )  \r\n and isnull(b.cVerifier,'') = '' and bReturnFlag = 1 \r\n" + whereClause + " order by iDLsID ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_Assems(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and a.cAVCode = '" + VI.cCode + "' ";
			}
			string safeSql = " select a.cAVCode cCode,a.irowno RowNo,a.Autoid,b.cDepCode,c.cDepName,null cVenCode,null cVenName,null cCusCode,null cCusName, \r\n a.igroupno iGroup,a.bAVType, CONVERT(varchar(10),b.dAVDate,121) dDate,b.dnverifytime dTime,CONVERT(varchar(10),b.dVerifyDate,121) dVeriDate, \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iAVQuantity as decimal(20,4)) iQuantity,b.cAVMemo cMemo,a.cbMemo,null cRdCode,a.cWhCode,e.cWhName, \r\n g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7, \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16  \r\n from AssemVouchs a (nolock)  left join AssemVouch b (nolock) on a.ID=b.ID   \r\n left join department c (nolock) on b.cDepCode = c.cDepCode   \r\n left join Warehouse e (nolock) on a.cwhcode=e.cwhcode  \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnverifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnverifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(b.cVerifyPerson,'') <> '' and b.cvouchtype = '15' \r\n" + whereClause + " order by Autoid ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_Assems08(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords08 a (nolock) left join RdRecord08 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and b.csource = '形态转换' " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_Assems09(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords09 a (nolock) left join RdRecord09 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and b.csource = '形态转换'  " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_ZZCX(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and a.cAVCode = '" + VI.cCode + "' ";
			}
			string safeSql = " select a.cAVCode cCode,a.irowno RowNo,a.Autoid,b.cDepCode,c.cDepName,null cVenCode,null cVenName,null cCusCode,null cCusName, \r\n case cvouchtype when '13' then '组装单' else '拆卸单'end bType,a.bAVType, CONVERT(varchar(10),b.dAVDate,121) dDate,b.dnverifytime dTime,CONVERT(varchar(10),b.dVerifyDate,121) dVeriDate, \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iAVQuantity as decimal(20,4)) iQuantity,b.cAVMemo cMemo,a.cbMemo,null cRdCode,a.cWhCode,e.cWhName, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7, \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16  \r\n from AssemVouchs a (nolock)  left join AssemVouch b (nolock) on a.ID=b.ID   \r\n left join department c (nolock) on b.cDepCode = c.cDepCode   \r\n left join Warehouse e (nolock) on a.cwhcode=e.cwhcode  \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnverifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnverifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(b.cVerifyPerson,'') <> '' and b.cvouchtype in('13','14') " + whereClause + " order by Autoid ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_ZZCX08(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords08 a (nolock) left join RdRecord08 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and b.csource = '拆卸' " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Get_ZZCX09(VouchsItem VI)
	{
		try
		{
			string whereClause = "";
			if (!string.IsNullOrEmpty(VI.cCode))
			{
				whereClause = whereClause + " and cCode = '" + VI.cCode + "' ";
			}
			if (VI.SolidWh == "1")
			{
				whereClause += " and b.cWhCode = '03' ";
			}
			if (VI.SolidWh == "0")
			{
				whereClause += " and b.cWhCode <> '03' ";
			}
			string safeSql = " select cCode,irowno RowNo,Autoid,b.cDepCode,c.cDepName,b.cVenCode,d.cVenName,b.cCusCode,cu.cCusName, \r\n CONVERT(varchar(10),b.dDate,121) dDate,dnmaketime dTime,null dVeriDate,b.cWhCode,e.cWhName,  \r\n a.cInvCode,f.cInvName,f.cInvStd,cast(a.iquantity as decimal(20,4)) iQuantity,b.cMemo,a.cbMemo,b.cRdCode, \r\n f.cComUnitCode cUnitCode,g.cComUnitName cUnit,h.cComUnitName cAccUnit, b.cDefine1,b.cDefine2,b.cDefine3,b.cDefine4,b.cDefine5,b.cDefine6,b.cDefine7,  \r\n b.cDefine8,b.cDefine9,b.cDefine10,b.cDefine11,b.cDefine12,b.cDefine13,b.cDefine14,b.cDefine15,b.cDefine16 \r\n from RdRecords09 a (nolock) left join RdRecord09 b (nolock) on a.id=b.id \r\n left join department c (nolock) on b.cDepCode = c.cDepCode \r\n left join Vendor d (nolock) on b.cvencode=d.cvencode \r\n left join customer cu (nolock) on b.cCusCode=cu.ccuscode \r\n left join Warehouse e (nolock) on b.cwhcode=e.cwhcode \r\n left join inventory f (nolock) on a.cinvcode=f.cinvcode \r\n left join ComputationUnit g (nolock) on f.cComUnitCode=g.cComunitCode \r\n left join ComputationUnit h (nolock) on f.cAssComUnitCode=h.cComunitCode \r\n where ( ( convert(varchar(19),dnmaketime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmaketime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dnmodifytime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dnmodifytime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n and isnull(cHandler  ,'') = '' and b.csource = '拆卸'  " + whereClause;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonResult = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonResult + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_RedRD01(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from rdrecord01 (nolock) where ccode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string vouchId = dataTable.Rows[0]["ID"].ToString();
			string value = dataTable.Rows[0]["cHandler"].ToString();
			string whCode = dataTable.Rows[0]["cwhcode"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			sql = " update rdrecord01 set dnverifytime=getdate(),cHandler='" + VI.cHandler + "',dVeriDate=ddate  where ccode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " select cValue from  AccInformation (nolock) where cName = 'bPurchaseInCheck' ";
			if (U8SqlDBHelper.GetString(sql).ToUpper() == "TRUE")
			{
				sql = "exec pro_uptcurrentStock '" + zt + "','01','" + vouchId + "'";
				sqlList.Add(sql);
				sql = " select * from rdrecords01 (nolock) where id = '" + vouchId + "' ";
				DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
				for (int i = 0; i < dataTable2.Rows.Count; i++)
				{
					sql = string.Concat(" update currentstock set fInQuantity=fInQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
					sqlList.Add(sql);
				}
			}
			int num = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + VI.cCode + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"审核失败！\",\"U8Code\":\"" + VI.cCode + "\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_RD11(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from rdrecord11 (nolock) where ccode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string vouchId = dataTable.Rows[0]["ID"].ToString();
			string value = dataTable.Rows[0]["cHandler"].ToString();
			string whCode = dataTable.Rows[0]["cwhcode"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			sql = " update rdrecord11 set dnverifytime=getdate(),cHandler='" + VI.cHandler + "',dVeriDate= ddate  where ccode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " select cValue from  AccInformation (nolock) where cName = 'bMaterialOutCheck' ";
			if (U8SqlDBHelper.GetString(sql).ToUpper() == "TRUE")
			{
				sql = "exec pro_uptcurrentStock '" + zt + "','11','" + vouchId + "'";
				sqlList.Add(sql);
				sql = " select * from rdrecords11 (nolock) where id = '" + vouchId + "' ";
				DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
				for (int i = 0; i < dataTable2.Rows.Count; i++)
				{
					sql = string.Concat(" update currentstock set fOutQuantity=fOutQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
					sqlList.Add(sql);
				}
			}
			int num = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + VI.cCode + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"审核失败！\",\"U8Code\":\"" + VI.cCode + "\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_RedRD11(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from rdrecord11 (nolock) where ccode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string vouchId = dataTable.Rows[0]["ID"].ToString();
			string value = dataTable.Rows[0]["cHandler"].ToString();
			string whCode = dataTable.Rows[0]["cwhcode"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			sql = " update rdrecord11 set dnverifytime=getdate(),cHandler='" + VI.cHandler + "',dVeriDate=ddate  where ccode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " select cValue from  AccInformation (nolock) where cName = 'bMaterialOutCheck' ";
			if (U8SqlDBHelper.GetString(sql).ToUpper() == "TRUE")
			{
				sql = "exec pro_uptcurrentStock '" + zt + "','11','" + vouchId + "'";
				sqlList.Add(sql);
				sql = " select * from rdrecords11 (nolock) where id = '" + vouchId + "' ";
				DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
				for (int i = 0; i < dataTable2.Rows.Count; i++)
				{
					sql = string.Concat(" update currentstock set fOutQuantity=fOutQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
					sqlList.Add(sql);
				}
			}
			int num = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + VI.cCode + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"审核失败！\",\"U8Code\":\"" + VI.cCode + "\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_RD10(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from rdrecord10 (nolock) where ccode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string vouchId = dataTable.Rows[0]["ID"].ToString();
			string value = dataTable.Rows[0]["cHandler"].ToString();
			string whCode = dataTable.Rows[0]["cwhcode"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			sql = " update rdrecord10 set dnverifytime=getdate(),cHandler='" + VI.cHandler + "',dVeriDate=ddate  where ccode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " select cValue from  AccInformation (nolock) where cName = 'bProductInCheck' ";
			if (U8SqlDBHelper.GetString(sql).ToUpper() == "TRUE")
			{
				sql = "exec pro_uptcurrentStock '" + zt + "','10','" + vouchId + "'";
				sqlList.Add(sql);
				sql = " select * from rdrecords10 (nolock) where id = '" + vouchId + "' ";
				DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
				for (int i = 0; i < dataTable2.Rows.Count; i++)
				{
					sql = string.Concat(" update currentstock set fInQuantity=fInQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
					sqlList.Add(sql);
				}
			}
			int num = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + VI.cCode + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"审核失败！\",\"U8Code\":\"" + VI.cCode + "\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_RedRD10(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from rdrecord10 (nolock) where ccode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string vouchId = dataTable.Rows[0]["ID"].ToString();
			string value = dataTable.Rows[0]["cHandler"].ToString();
			string whCode = dataTable.Rows[0]["cwhcode"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			sql = " update rdrecord10 set dnverifytime=getdate(),cHandler='" + VI.cHandler + "',dVeriDate=ddate  where ccode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " select cValue from  AccInformation (nolock) where cName = 'bProductInCheck' ";
			if (U8SqlDBHelper.GetString(sql).ToUpper() == "TRUE")
			{
				sql = "exec pro_uptcurrentStock '" + zt + "','10','" + vouchId + "'";
				sqlList.Add(sql);
				sql = " select * from rdrecords10 (nolock) where id = '" + vouchId + "' ";
				DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
				for (int i = 0; i < dataTable2.Rows.Count; i++)
				{
					sql = string.Concat(" update currentstock set fInQuantity=fInQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
					sqlList.Add(sql);
				}
			}
			int num = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + VI.cCode + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"审核失败！\",\"U8Code\":\"" + VI.cCode + "\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_RD08(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from rdrecord08 (nolock) where ccode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string vouchId = dataTable.Rows[0]["ID"].ToString();
			string value = dataTable.Rows[0]["cHandler"].ToString();
			string whCode = dataTable.Rows[0]["cwhcode"].ToString();
			string sourceType = dataTable.Rows[0]["csource"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			sql = " update rdrecord08 set dnverifytime=getdate(),cHandler='" + VI.cHandler + "',dVeriDate=ddate  where ccode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " select cValue from  AccInformation (nolock) where cName = 'bOtherInCheck' ";
			if (U8SqlDBHelper.GetString(sql).ToUpper() == "TRUE")
			{
				sql = "exec pro_uptcurrentStock '" + zt + "','08','" + vouchId + "'";
				sqlList.Add(sql);
				sql = " select * from rdrecords08 (nolock) where id = '" + vouchId + "' ";
				DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
				for (int i = 0; i < dataTable2.Rows.Count; i++)
				{
					if (sourceType == "调拨")
					{
						sql = string.Concat(" update currentstock set fTransInQuantity=fTransInQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
						sqlList.Add(sql);
					}
					else
					{
						sql = string.Concat(" update currentstock set fInQuantity=fInQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
						sqlList.Add(sql);
					}
				}
			}
			int num = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + VI.cCode + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"审核失败！\",\"U8Code\":\"" + VI.cCode + "\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_RD09(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from rdrecord09 (nolock) where ccode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string vouchId = dataTable.Rows[0]["ID"].ToString();
			string value = dataTable.Rows[0]["cHandler"].ToString();
			string whCode = dataTable.Rows[0]["cwhcode"].ToString();
			string sourceType = dataTable.Rows[0]["csource"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			sql = " update rdrecord09 set dnverifytime=getdate(),cHandler='" + VI.cHandler + "',dVeriDate=ddate  where ccode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " select cValue from  AccInformation (nolock) where cName = 'bOtherOutCheck' ";
			if (U8SqlDBHelper.GetString(sql).ToUpper() == "TRUE")
			{
				sql = "exec pro_uptcurrentStock '" + zt + "','09','" + vouchId + "'";
				sqlList.Add(sql);
				sql = " select * from rdrecords09 (nolock) where id = '" + vouchId + "' ";
				DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
				for (int i = 0; i < dataTable2.Rows.Count; i++)
				{
					if (sourceType == "调拨")
					{
						sql = string.Concat(" update currentstock set fTransOutQuantity=fTransOutQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
						sqlList.Add(sql);
					}
					else
					{
						sql = string.Concat(" update currentstock set fOutQuantity=fOutQuantity - ", dataTable2.Rows[i]["iquantity"], "  where cInvCode='", dataTable2.Rows[i]["cInvCode"], "' and cWhCode='", whCode, "' ");
						sqlList.Add(sql);
					}
				}
			}
			int num = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + VI.cCode + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"审核失败！\",\"U8Code\":\"" + VI.cCode + "\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_Trans(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
        string whereClause = "";
            try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from TransVouch (nolock) where cTVCode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string resultId = dataTable.Rows[0]["ID"].ToString();
			string value = dataTable.Rows[0]["cVerifyPerson"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			sql = " update TransVouch set cVerifyPerson='" + VI.cHandler + "',dVerifyDate=convert(varchar(10),getdate(),121) ,dnverifytime =getdate()   where cTVCode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			int num = 0;
			int num2 = 0;
			sql = " select iFatherId,iChildId from UFSystem..UA_Identity where cAcc_Id='" + cAcc_Id + "' and cVouchType='rd' ";
			DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable2.Rows.Count > 0)
			{
				num = Convert.ToInt32(dataTable2.Rows[0]["iFatherId"].ToString());
				num2 = Convert.ToInt32(dataTable2.Rows[0]["iChildId"].ToString());
			}
			else
			{
				sql = " insert into UFSystem..UA_Identity (cAcc_Id,cVouchType,iFatherId,iChildId)  values('" + cAcc_Id + "','rd',0,0) ";
				U8SqlDBHelper.ExecuteSql(sql);
			}
			string jsonResult = DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0');
			int num3 = 0;
			sql = " select cNumber from VoucherHistory where CardNumber ='0302' and cSeed = '" + jsonResult + "' ";
			DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable3.Rows.Count > 0)
			{
				num3 = Convert.ToInt32(dataTable3.Rows[0]["cNumber"].ToString());
			}
			else
			{
				U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('0302',null,'日期','月','" + jsonResult + "',0,0) ");
			}
			int num4 = 0;
			sql = " select cNumber from VoucherHistory where CardNumber ='0301' and cSeed = '" + jsonResult + "' ";
			DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable4.Rows.Count > 0)
			{
				num4 = Convert.ToInt32(dataTable4.Rows[0]["cNumber"].ToString());
			}
			else
			{
				U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('0301',null,'日期','月','" + jsonResult + "',0,0) ");
			}
			num++;
			U8SqlDBHelper.ExecuteSql(" update UFSystem..UA_Identity set iFatherId=iFatherId+1 where cAcc_Id='" + cAcc_Id + "' and cVouchType='rd' ");
			string autoidStr = "1" + $"{num:D9}";
			num3++;
			U8SqlDBHelper.ExecuteSql("update VoucherHistory set cNumber=cNumber+1 where CardNumber ='0302' and cSeed = '" + jsonResult + "'");
			string yearMonthStr = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + num3.ToString().PadLeft(4, '0');
			sql = string.Concat(" insert into rdrecord09(id,brdflag,cvouchtype,cbustype,csource,  cwhcode,ddate,ccode,crdcode,  chandler,cbuscode,cDepCode,  cmaker,bpufirst,biafirst,vt_id,bisstqc,ibg_overflag,cbg_auditor,cbg_audittime,controlresult,  iswfcontrolled,dnmaketime,dnmodifytime,dVeriDate,dnverifytime,iprintcount,cMemo)   values (", autoidStr, ",N'0',N'09',N'调拨出库',N'调拨',  '", dataTable.Rows[0]["cOWhCode"], "',convert(varchar(10),getdate(),121),'", yearMonthStr, "','", dataTable.Rows[0]["cordcode"], "',  '", VI.cHandler, "','", VI.cCode, "', '", dataTable.Rows[0]["cODepCode"], "',  N'", dataTable.Rows[0]["cMaker"], "',0,0,85,0,0,N'',N'',-1,  0,getdate(),Null, convert(varchar(10),getdate(),121) ,getdate(),0,'", dataTable.Rows[0]["cTVMemo"], "') ");
			sqlList.Add(sql);
			num++;
			U8SqlDBHelper.ExecuteSql(" update UFSystem..UA_Identity set iFatherId=iFatherId+1 where cAcc_Id='" + cAcc_Id + "' and cVouchType='rd' ");
			string detailIdStr = "1" + $"{num:D9}";
			num4++;
			U8SqlDBHelper.ExecuteSql("update VoucherHistory set cNumber=cNumber+1 where CardNumber ='0301' and cSeed = '" + jsonResult + "' ");
			string dateStr = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + num4.ToString().PadLeft(4, '0');
			sql = string.Concat(" insert into rdrecord08(id,brdflag,cvouchtype,cbustype,csource, cwhcode,ddate,ccode,  crdcode,cbuscode,cDepCode,  cmaker,bpufirst,biafirst,vt_id,bisstqc,bislsquery,iswfcontrolled,dnmaketime,dnmodifytime, cHandler,dVeriDate,dnverifytime,iprintcount,cMemo)  values('", detailIdStr, "', N'1', N'08', N'调拨入库', N'调拨',  '", dataTable.Rows[0]["cIWhCode"], "',convert(varchar(10),getdate(),121) , '", dateStr, "',  '", dataTable.Rows[0]["cirdcode"], "','", VI.cCode, "', '", dataTable.Rows[0]["cIDepCode"], "',  '", dataTable.Rows[0]["cMaker"], "', 0, 0, 67, 0, 0, 0, getdate(), Null, '", VI.cHandler, "', convert(varchar(10),getdate(),121) , getdate(), 0,'", dataTable.Rows[0]["cTVMemo"], "') ");
			sqlList.Add(sql);
			sql = " select * from TransVouchs (nolock) where ID = '" + resultId + "' ";
			DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
			int num5 = 0;
			for (int i = 0; i < dataTable5.Rows.Count; i++)
			{
				num2++;
				U8SqlDBHelper.ExecuteSql(" update UFSystem..UA_Identity set iChildId=iChildId+1 where cAcc_Id='" + cAcc_Id + "' and cVouchType='rd' ");
				string itemIdStr = "1" + $"{num2:D9}";
				sql = string.Concat(" Insert Into rdrecords09(autoid, id, cinvcode, inum, iquantity, cbatch,   bcosting, isotype, irowno,iinvexchrate,cassunit,iTrIds,cbMemo )  values(", itemIdStr, ", ", autoidStr, ", '", dataTable5.Rows[i]["cInvCode"], "', null, ", dataTable5.Rows[i]["iTVQuantity"], ",null,  1, 0, ", num5, ", null, null,'", dataTable5.Rows[i]["autoID"], "','", dataTable5.Rows[i]["cbMemo"], "' )");
				sqlList.Add(sql);
				sql = " Insert Into Rdrecords09sub(autoid,id,cbg_itemcode,cbg_itemname,cbg_caliberkey1,cbg_caliberkeyname1,cbg_caliberkey2,cbg_caliberkeyname2,  cbg_caliberkey3,cbg_caliberkeyname3,cbg_calibercode1,cbg_calibername1,cbg_calibercode2,cbg_calibername2,cbg_calibercode3,cbg_calibername3,  ibg_ctrl,cbg_auditopinion,ibgstsum,ibgiasum,cbg_caliberkey4,cbg_caliberkeyname4,cbg_caliberkey5,cbg_caliberkeyname5,cbg_caliberkey6,  cbg_caliberkeyname6,cbg_calibercode4,cbg_calibername4,cbg_calibercode5,cbg_calibername5,cbg_calibercode6,cbg_calibername6)  values(" + itemIdStr + ", " + autoidStr + ", Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, 0, Null, 0, Null, Null, Null, Null,  Null, Null, Null, Null, Null, Null, Null, Null, Null)  ";
				sqlList.Add(sql);
				sqlList.Add("insert into IA_ST_UnAccountVouch09(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values ('" + autoidStr + "','" + itemIdStr + "','09','其他出库')");
				num2++;
				U8SqlDBHelper.ExecuteSql(" update UFSystem..UA_Identity set iChildId=iChildId+1 where cAcc_Id='" + cAcc_Id + "' and cVouchType='rd' ");
				string voucherCode = "1" + $"{num2:D9}";
				sql = string.Concat(" Insert Into rdrecords08(autoid,id,cinvcode,inum,iquantity,cbatch,  bcosting,iexpiratdatecalcu,isotype,irowno,iinvexchrate,cassunit,iTrIds,cbMemo )  values('", voucherCode, "', '", detailIdStr, "', '", dataTable5.Rows[i]["cInvCode"], "', null, ", dataTable5.Rows[i]["iTVQuantity"], ",null,    1, 0,  0, ", num5, ", null, null,'", dataTable5.Rows[i]["autoID"], "','", dataTable5.Rows[i]["cbMemo"], "' )");
				sqlList.Add(sql);
				sql = " insert IA_ST_UnAccountVouch08(IDUN, IDSUN, cVouTypeUN, cBustypeUN)  values ('" + detailIdStr + "','" + voucherCode + "','08','其他入库') ";
				sqlList.Add(sql);
				sql = string.Concat(" update currentstock set fTransOutQuantity=fTransOutQuantity - ", dataTable5.Rows[i]["iTVQuantity"], "  where cInvCode='", dataTable5.Rows[i]["cInvCode"], "' and cWhCode='", dataTable.Rows[0]["cOWhCode"], "' ");
				sqlList.Add(sql);
				sql = string.Concat(" update currentstock set fTransInQuantity=fTransInQuantity - ", dataTable5.Rows[i]["iTVQuantity"], "  where cInvCode='", dataTable5.Rows[i]["cInvCode"], "' and cWhCode='", dataTable.Rows[0]["cIWhCode"], "' ");
				sqlList.Add(sql);
			}
			sql = "exec pro_uptcurrentStock '" + zt + "','09','" + autoidStr + "'";
			sqlList.Add(sql);
			sql = "exec pro_uptcurrentStock '" + zt + "','08','" + detailIdStr + "'";
			sqlList.Add(sql);
			int num6 = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num6 > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"U8调拨单[" + VI.cCode + "]审核成功！\",\"U8Code\":\"" + VI.cCode + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"U8调拨单审核失败！\",\"U8Code\":\"\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_Dis(VouchsItem VI)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from DispatchList (nolock) where cDLCode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string resultId = dataTable.Rows[0]["DLID"].ToString();
			string value = dataTable.Rows[0]["cVerifier"].ToString();
			if (!string.IsNullOrEmpty(value))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已审核，不可重复审核！\"}";
			}
			string jsonResult = "null,null,null ";
			if (VI.IsVerify == "1")
			{
				jsonResult = "convert(varchar(10),getdate(),121) ,cVerifier ,getdate() ";
			}
			sql = " update DispatchList set cVerifier='" + VI.cHandler + "',dverifydate=convert(varchar(10),getdate(),121) ,dverifysystime =getdate()   where cDLCode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " insert into IA_SA_UnAccountVouch (IDUN,IDSUN,cVouTypeUN,cBustypeUN)  select a.DLID,a.iDLsID,'05',cBusType  from DispatchLists a left join DispatchList b on a.DLID=b.DLID   where cDLCode = '" + VI.cCode + "' ";
			sqlList.Add(sql);
			sql = " select cValue from  AccInformation (nolock) where cName = 'bSaleOutCheck' ";
			string autoidStr = U8SqlDBHelper.GetString(sql).ToUpper();
			int num = 0;
			int num2 = 0;
			sql = " select iFatherId,iChildId from UFSystem..UA_Identity where cAcc_Id='" + cAcc_Id + "' and cVouchType='rd' ";
			DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable2.Rows.Count > 0)
			{
				num = Convert.ToInt32(dataTable2.Rows[0]["iFatherId"].ToString());
				num2 = Convert.ToInt32(dataTable2.Rows[0]["iChildId"].ToString());
			}
			string yearMonthStr = DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0');
			int num3 = 0;
			sql = " select cNumber from VoucherHistory where CardNumber ='0303' and cSeed = '" + yearMonthStr + "' ";
			DataTable dataTable3 = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable3.Rows.Count > 0)
			{
				num3 = Convert.ToInt32(dataTable3.Rows[0]["cNumber"].ToString());
			}
			else
			{
				U8SqlDBHelper.GetExecute("insert into VoucherHistory (CardNumber,iRdFlagSeed,cContent,cContentRule,cSeed,cNumber,bEmpty) values ('0303',null,'日期','月','" + yearMonthStr + "',0,0) ");
			}
			string detailIdStr = "";
			sql = " select distinct cwhcode from DispatchLists (nolock) where DLID = '" + resultId + "' ";
			DataTable dataTable4 = U8SqlDBHelper.GetDataTable(sql);
			for (int i = 0; i < dataTable4.Rows.Count; i++)
			{
				string dateStr = dataTable4.Rows[i]["cwhcode"].ToString();
				if (string.IsNullOrEmpty(dateStr))
				{
					return "{\"Code\":\"400\",\"Msg\":\"发货单[" + VI.cCode + "]未维护仓库，不可审核！\"}";
				}
				num++;
				sql = " update UFSystem..UA_Identity set iFatherId=iFatherId+1 where  cAcc_Id='" + cAcc_Id + "' and cVouchType = 'rd' ";
				U8SqlDBHelper.ExecuteSql(sql);
				string itemIdStr = "1" + $"{num:D9}";
				num3++;
				string voucherCode = "XSCK" + yearMonthStr + num3.ToString().PadLeft(4, '0');
				sql = " update VoucherHistory set cNumber=cNumber+1 where CardNumber = '0303' and cSeed='" + yearMonthStr + "' ";
				U8SqlDBHelper.ExecuteSql(sql);
				string sysBarcode = "||st32|" + voucherCode;
				detailIdStr = detailIdStr + voucherCode + ",";
				sql = "  insert rdrecord32  \r\n (id, brdflag, cvouchtype, cbustype, csource ,  \r\n cbuscode, cwhcode, ddate, ccode, crdcode ,  \r\n cdepcode, cpersoncode, cstcode, ccuscode, cdlcode ,  \r\n cmaker, vt_id, iswfcontrolled, dnmaketime,cShipAddress ,csysbarcode,  \r\n iprintcount, dVeriDate, cHandler, dnverifytime, cMemo , iflowid, \r\n cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,  \r\n cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 )  \r\n   select '" + itemIdStr + "' ,0 ,'32',cbustype ,'发货单' , \r\n cDLCode ,'" + dateStr + "' ,convert(varchar(10),getdate(),121) ,'" + voucherCode + "' ,'203' , \r\n cdepcode ,cpersoncode ,cstcode ,ccuscode ,'" + resultId + "' , \r\n cVerifier ,'87' ,0 ,getdate() ,cShipAddress ,'" + sysBarcode + "', \r\n 0 ," + jsonResult + " , cMemo , iflowid, \r\n cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7, \r\n cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16 \r\n from DispatchList  \r\n where cDLCode = '" + VI.cCode + "' ";
				sqlList.Add(sql);
				sql = " update DispatchList set cSaleOut = '" + voucherCode + "' where cdlcode = '" + VI.cCode + "'  ";
				sqlList.Add(sql);
				sql = " select * from DispatchLists where DLID = '" + resultId + "' and cwhcode = '" + dateStr + "' ";
				DataTable dataTable5 = U8SqlDBHelper.GetDataTable(sql);
				int num4 = 0;
				for (int j = 0; j < dataTable5.Rows.Count; j++)
				{
					string dlId = dataTable5.Rows[j]["iDLsID"].ToString();
					num4++;
					num2++;
					sql = " update UFSystem..UA_Identity set iChildId=iChildId+1 where  cAcc_Id='" + cAcc_Id + "' and cVouchType = 'rd' ";
					U8SqlDBHelper.ExecuteSql(sql);
					string rowIdStr = "1" + $"{num2:D9}";
					string rowBarcode = "||st32|" + voucherCode + "|" + num4;
					sql = string.Concat("  insert rdrecords32  \r\n (autoid, id, cinvcode, iNum, iquantity, iFlag, iDLsID ,  \r\n iNQuantity, bLPUseFree, iRSRowNO, iOriTrackID, bCosting ,  \r\n bVMIUsed, cbdlcode, ipesodid, ipesotype , \r\n cpesocode, ipesoseq, isotype, irowno , \r\n iorderseq,iordertype,iorderdid,csocode,iordercode,isodid,isoseq ,  \r\n cbsysbarcode, bIAcreatebill, bsaleoutcreatebill, bneedbill, iposflag, \r\n cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29, \r\n cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 ) \r\n   select '", rowIdStr, "','", itemIdStr, "',cinvcode, iNum, iquantity, 0, '", dlId, "',  iquantity, 0, 0, 0, 1, \r\n 0,'", VI.cCode, "','", dataTable5.Rows[j]["iSOsID"], "',1 , \r\n '", dataTable5.Rows[j]["cSoCode"], "','", dataTable5.Rows[j]["iorderrowno"], "',0, '", num4, "', \r\n '", dataTable5.Rows[j]["iorderrowno"], "',1,'", dataTable5.Rows[j]["iSOsID"], "',null,'", dataTable5.Rows[j]["cSoCode"], "',null,null,  \r\n '", rowBarcode, "',0, 0, 1, null, \r\n cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29, \r\n cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37 \r\n from DispatchLists  where iDLsID='", dlId, "' ");
					sqlList.Add(sql);
					sql = " update DispatchLists set fOutQuantity=iquantity where iDLsID='" + dlId + "' ";
					sqlList.Add(sql);
					sql = " select iquantity from DispatchLists where iDLsID='" + dlId + "' ";
					decimal num5 = BasicDAL.ToDec(U8SqlDBHelper.GetString(sql));
					sql = string.Concat(" update so_sodetails set foutquantity=isnull(foutquantity,0)+ ", num5, "  where isosid = '", dataTable5.Rows[j]["iSOsID"], "' ");
					sqlList.Add(sql);
					if ((autoidStr == "TRUE" && VI.IsVerify == "1") || (autoidStr != "TRUE" && VI.IsVerify != "1"))
					{
						sql = string.Concat(" update currentstock set fOutQuantity=fOutQuantity - ", dataTable5.Rows[j]["iquantity"], "  where cInvCode='", dataTable5.Rows[j]["cinvcode"], "' and cWhCode='", dateStr, "' ");
						sqlList.Add(sql);
					}
				}
				if ((autoidStr == "TRUE" && VI.IsVerify == "1") || (autoidStr != "TRUE" && VI.IsVerify != "1"))
				{
					sql = "exec pro_uptcurrentStock '" + zt + "','32','" + itemIdStr + "'";
					sqlList.Add(sql);
				}
			}
			detailIdStr = detailIdStr.Substring(0, detailIdStr.Length - 1);
			int num6 = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (num6 > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\",\"U8Code\":\"" + detailIdStr + "\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"U8发货单审核失败！\",\"U8Code\":\"\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string Post_Assems(VouchsItem VI)
	{
		List<string> sQLStringList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(VI.cCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[cCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(VI.cHandler))
			{
				return "{\"Code\":\"400\",\"Msg\":\"审核人[cHandler]未传递！\"}";
			}
			sql = " select * from AssemVouch (nolock) where cAVCode = '" + VI.cCode + "' ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]不存在！\"}";
			}
			string resultId = dataTable.Rows[0]["ID"].ToString();
			sql = " select 1 from AssemVouchs a where a.cAVCode = '" + VI.cCode + "'  and ( exists ( select 1 from RdRecord09 b where a.cAVCode = b.cbuscode and isnull(cHandler,'')<>'')  or exists ( select 1 from RdRecord08 b where a.cAVCode = b.cbuscode and isnull(cHandler,'')<>'') ) ";
			dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count > 0)
			{
				return "{\"Code\":\"400\",\"Msg\":\"单据编号[" + VI.cCode + "]已有审核的其他出入库单据，不可重复审核！\"}";
			}
			int num = U8SqlDBHelper.ExecuteSqlTran(sQLStringList);
			if (num > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"审核成功！\"}";
			}
			return "{\"Code\":\"400\",\"Msg\":\"审核失败！\"}";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}
}
}
