using System;
using System.Configuration;
using System.Data;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{

public class DataDownLoadDAL
{
	private static string zt = ConfigurationManager.AppSettings["zt"].ToString();

	private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();

	public static string GetInventory(VouchsItem VI)
	{
		try
		{
			string filterSql = "";
			if (!string.IsNullOrEmpty(VI.cInvCode))
			{
				filterSql = filterSql + " and a.cinvcode like '%" + VI.cInvCode + "%' ";
			}
			if (!string.IsNullOrEmpty(VI.cInvCCode))
			{
				filterSql = filterSql + " and a.cInvCCode = '" + VI.cInvCCode + "' ";
			}
			if (!string.IsNullOrEmpty(VI.cInvName))
			{
				filterSql = filterSql + " and a.cInvName like '%" + VI.cInvName + "%' ";
			}
			if (!string.IsNullOrEmpty(VI.cInvStd))
			{
				filterSql = filterSql + " and a.cInvStd like '%" + VI.cInvStd + "%' ";
			}
			string safeSql = " select cInvCode,cInvName,cInvStd,a.cInvCCode ,b.cInvCName,a.cComUnitCode cUnitCode,c.cComUnitName cUnit, \r\n bInvBatch,dSDate,dEDate,cDefWareHouse,bPurchase,bSelf,bSale,bProxyForeign,bComsume,  \r\n case when isnull(dEDate,'1900-01-01')< GETDATE() then 0 else 1 end as bEnd  from inventory a (nolock)  \r\n left join InventoryClass b (nolock) on a.cinvccode=b.cinvccode \r\n left join ComputationUnit c (nolock) on a.cComUnitCode=c.cComUnitCode \r\n left join Inventory_sub d (nolock) on a.cInvCode = d.cInvSubCode \r\n where ( ( convert(varchar(19),dInvCreateDatetime,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dInvCreateDatetime,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )   \r\n or ( convert(varchar(19),dModifyDate,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) and convert(varchar(19),dModifyDate,121) <= convert(varchar(19),'" + VI.VouchTimeE + "',121) )  ) \r\n" + filterSql;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonData = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonData + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string GetWH(VouchsItem VI)
	{
		try
		{
			string filterSql = "";
			if (!string.IsNullOrEmpty(VI.cWhCode))
			{
				filterSql = filterSql + " and cWhCode = '" + VI.cWhCode + "' ";
			}
			if (!string.IsNullOrEmpty(VI.cWhName))
			{
				filterSql = filterSql + " and cWhCode like '%" + VI.cWhName + "%' ";
			}
			if (!string.IsNullOrEmpty(VI.VouchTimeS))
			{
				filterSql = filterSql + " and convert(varchar(19),dModifyDate,121) >= convert(varchar(19),'" + VI.VouchTimeS + "',121) ";
			}
			if (!string.IsNullOrEmpty(VI.VouchTimeE))
			{
				filterSql = filterSql + " and convert(varchar(19),dModifyDate,121) =< convert(varchar(19),'" + VI.VouchTimeE + "',121) ";
			}
			string safeSql = " select cWhCode,cWhName,bWhPos,cDepCode,cWhMemo  \r\n from warehouse a (nolock) where 1=1 \r\n" + filterSql;
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonData = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonData + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string GetBom(VouchsItem VI)
	{
		try
		{
			if (string.IsNullOrEmpty(VI.cInvCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"存货编码[cInvCode]未传递！\",\"Items\":\"\"}";
			}
			string filterSql = "";
			string safeSql = string.Concat(str3: (!string.IsNullOrEmpty(VI.Ver)) ? " and exists ( select 1 from (select ParentId,max(Version) ver from bom_bom bo (nolock)  left join bom_parent pa (nolock) on bo.bomid=pa.bomid where isnull(bo.CloseUser,'')='' and isnull(RelsUser,'')<>'' and bo.Status =3 group by ParentId) z   where b.ParentId=z.ParentId and a.Version=z.ver ) " : (" and a.Version = '" + VI.Ver + "' "), str0: " select a.BomId,c.InvCode cInvCode,a.Version Ver,f.cInvCode InvCode,f.cInvName InvName,f.cInvStd InvStd,d.BaseQtyN,d.BaseQtyD,d.CompScrap  from bom_bom a (nolock) left join bom_parent b (nolock) on a.BomId = b.BomId  left join Bas_part c (nolock) on c.partid = b.parentid left join bom_opcomponent d (nolock) on a.BomId=d.BomId   left join bas_part e (nolock) on e.PartId = d.ComponentId left join Inventory f (nolock) on e.InvCode=f.cInvCode   where c.InvCode = '", str1: VI.cInvCode, str2: "' and isnull(a.CloseUser,'')='' and isnull(RelsUser,'')<>'' and a.Status =3  ");
			DataTable dataTable = U8SqlDBHelper.GetDataTable(safeSql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonData = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonData + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}

	public static string GetCurrent(VouchsItem VI)
	{
		try
		{
			if (string.IsNullOrEmpty(VI.cWhCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"仓库编码[cWhCode]未传递！\",\"Items\":\"\"}";
			}
			string filterSql = " and a.cWhCode = '" + VI.cWhCode + "' ";
			if (!string.IsNullOrEmpty(VI.cInvCode))
			{
				filterSql = filterSql + " and a.cinvcode = '" + VI.cInvCode + "' ";
			}
			if (!string.IsNullOrEmpty(VI.cBatch))
			{
				filterSql = filterSql + " and a.cbatch = '" + VI.cBatch + "' ";
			}
			string sql = "";
			if (BasicDAL.bWhPos(VI.cWhCode))
			{
				if (!string.IsNullOrEmpty(VI.cPosCode))
				{
					filterSql = filterSql + " and a.cPosCode = '" + VI.cPosCode + "' ";
				}
				sql = " select a.cInvCode,b.cInvName,b.cInvStd,a.cWhCode,c.cWhName,a.cPosCode,d.cPosName,a.cBatch,a.iQuantity   from InvPositionSum a (nolock)  left join inventory b (nolock) on a.cinvcode = b.cinvcode  left join warehouse c (nolock) on a.cwhcode = c.cwhcode  left join position d (nolock) on a.cposcode = d.cposcode  where a.iquantity > 0 " + filterSql;
			}
			else
			{
				sql = " select a.cInvCode,b.cInvName,b.cInvStd,a.cWhCode,c.cWhName,'' cPosCode,'' cPosName,a.cBatch,a.iQuantity   from currentstock a (nolock)  left join inventory b (nolock) on a.cinvcode = b.cinvcode   left join warehouse c (nolock) on a.cwhcode = c.cwhcode  where a.iQuantity > 0  " + filterSql;
			}
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"无数据！\",\"Items\":\"\"}";
			}
			string jsonData = transform.DataTableToJsonWithJsonNet(dataTable);
			return "{\"Code\":\"200\",\"Msg\":\"查询成功！\",\"Items\": " + jsonData + " }";
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
		}
	}
}
}
