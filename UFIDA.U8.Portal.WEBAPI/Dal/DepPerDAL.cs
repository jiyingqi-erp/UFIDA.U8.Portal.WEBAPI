using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{

public class DepPerDAL
{
	private static string zt = ConfigurationManager.AppSettings["zt"].ToString();

	private static string cAcc_Id = ConfigurationManager.AppSettings["cAccId"].ToString();

	public static string Dep(Department dep)
	{
		List<string> sqlList = new List<string>();
		string sql = "";
		try
		{
			if (string.IsNullOrEmpty(dep.cDepCode))
			{
				return "{\"Code\":\"400\",\"Msg\":\"部门编码[cDepCode]未传递！\"}";
			}
			if (string.IsNullOrEmpty(dep.cDepName))
			{
				return "{\"Code\":\"400\",\"Msg\":\"部门名称[cDepName]未传递！\"}";
			}
			if (string.IsNullOrEmpty(dep.iDepGrade))
			{
				return "{\"Code\":\"400\",\"Msg\":\"部门级次[iDepGrade]未传递！\"}";
			}
			int grade = 0;
			try
			{
				grade = Convert.ToInt32(dep.iDepGrade);
				if (grade < 1)
				{
					return "{\"Code\":\"400\",\"Msg\":\"部门级次[" + dep.iDepGrade + "]数据错误！\",\"U8Code\":\"\"}";
				}
				if (grade > 1)
				{
					int parentGrade = grade - 1;
					sql = " select LEN(cDepCode) from Department (nolock) where iDepGrade = " + parentGrade + " ";
					int parentCodeLen = Convert.ToInt32(U8SqlDBHelper.GetString(sql));
					sql = " select 1 from Department (nolock) where cDepCode = left('" + dep.cDepCode + "'," + parentCodeLen + ") and bDepEnd = 0 and iDepGrade = " + parentGrade + " ";
					DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
					if (dataTable.Rows.Count == 0)
					{
						return "{\"Code\":\"400\",\"Msg\":\"部门[" + dep.cDepCode + "]没有匹配的上级部门！\",\"U8Code\":\"\"}";
					}
				}
			}
			catch
			{
				return "{\"Code\":\"400\",\"Msg\":\"部门级次[" + dep.iDepGrade + "]格式错误！\",\"U8Code\":\"\"}";
			}
			if (string.IsNullOrEmpty(dep.bDepEnd))
			{
				return "{\"Code\":\"400\",\"Msg\":\"是否末级[bDepEnd]未传递！\"}";
			}
			try
			{
				Convert.ToInt32(dep.bDepEnd);
				if (dep.bDepEnd != "0" && dep.bDepEnd != "1")
				{
					return "{\"Code\":\"400\",\"Msg\":\"是否末级[" + dep.bDepEnd + "]数据错误！\",\"U8Code\":\"\"}";
				}
			}
			catch
			{
				return "{\"Code\":\"400\",\"Msg\":\"是否末级[" + dep.bDepEnd + "]格式错误！\",\"U8Code\":\"\"}";
			}
			if (string.IsNullOrEmpty(dep.BeginDate))
			{
				return "{\"Code\":\"400\",\"Msg\":\"启用日期[dDate]未传递！\",\"U8Code\":\"\"}";
			}
			DateTime dateTime;
			try
			{
				dateTime = Convert.ToDateTime(dep.BeginDate);
			}
			catch
			{
				return "{\"Code\":\"400\",\"Msg\":\"启用日期[" + dep.BeginDate + "]格式错误！\",\"U8Code\":\"\"}";
			}
			sql = " select 1 from Department where cDepCode = '" + dep.cDepCode + "' ";
			DataTable dataTable2 = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable2.Rows.Count == 0)
			{
				string guid = U8SqlDBHelper.GetString("select NEWID()");
				int order = Convert.ToInt32(U8SqlDBHelper.GetString("select max(iDepOrder)+1 from Department"));
				sql = " insert into Department(cDepCode,bDepEnd,cDepName,iDepGrade,bShop,  cDepGUID,dDepBeginDate,bInheritDutyBasic,bInheritWorkCalendar,bIM,bRetail,cDepFullName,iDepOrder)  values ('" + dep.cDepCode + "','" + dep.bDepEnd + "','" + dep.cDepName + "','" + dep.iDepGrade + "',0,  '" + guid + "','" + dateTime.ToString("yyyy-MM-dd") + "', 1, 1, 1, 0,'" + dep.cDepName + "', " + order + ") ";
				sqlList.Add(sql);
			}
			else
			{
				if (dep.bDepEnd == "1")
				{
					grade++;
					sql = "select 1 from Department where cDepCode like '" + dep.cDepCode + "%' and iDepGrade = " + grade + "  ";
					dataTable2 = U8SqlDBHelper.GetDataTable(sql);
					if (dataTable2.Rows.Count > 0)
					{
						return "{\"Code\":\"400\",\"Msg\":\"部门编码[" + dep.cDepCode + "]已有下级部门，不可变更为末级！\",\"U8Code\":\"\"}";
					}
				}
				sql = " update  Department set bDepEnd='" + dep.bDepEnd + "',cDepName='" + dep.cDepName + "',cDepFullName='" + dep.cDepName + "'  where cDepCode = '" + dep.cDepCode + "' ";
				sqlList.Add(sql);
			}
			int result = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (result > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"U8部门档案[" + dep.cDepCode + "]上传成功！\",\"U8Code\":\"" + dep.cDepCode + "\" }";
			}
			return "{\"Code\":\"400\",\"Msg\":\"U8部门档案上传失败！\",\"U8Code\":\"\" }";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
		}
	}

	public static string Person(Person p)
	{
		string sql = "";
		List<string> sqlList = new List<string>();
		try
		{
			string validDate = "null";
			string invalidDate = "null";
			if (!string.IsNullOrEmpty(p.dPValidDate))
			{
				validDate = "'" + Convert.ToDateTime(p.dPValidDate).ToString("yyyy-MM-dd") + "'";
			}
			if (!string.IsNullOrEmpty(p.dPInValidDate))
			{
				invalidDate = "'" + Convert.ToDateTime(p.dPInValidDate).ToString("yyyy-MM-dd") + "'";
			}
			if (string.IsNullOrEmpty(p.cDepCode))
			{
				return "{\"Code\":400,\"Msg\":\"部门编码[cDepCode]未传递！\",\"U8Code\":\"\" }";
			}
			sql = " select 1 from Department where cDepCode = '" + p.cDepCode + "'  ";
			DataTable dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				return "{\"Code\":400,\"Msg\":\"部门编码[" + p.cDepCode + "]无数据！ \",\"U8Code\":\"\" }";
			}
			sql = " select 1 from Hr_hi_Person (nolock) where cPsn_Num = '" + p.cPersonCode + "' ";
			dataTable = U8SqlDBHelper.GetDataTable(sql);
			if (dataTable.Rows.Count == 0)
			{
				sql = "select newid()";
				dataTable = U8SqlDBHelper.GetDataTable(sql);
				string guid = dataTable.Rows[0][0].ToString();
				sql = " insert into Hr_hi_Person   ( cPsn_Num,cPsn_Name,cDept_num,rPersonType,rSex,rCheckInFlag,bPsnPerson,  cPsnMobilePhone,pk_hr_hi_person,bProbation,bTakeTM,rIDType,rFigure,EmploymentForm,  rPersonParameters,bDutyLock,bpsnshop,CardState,cShiftType,cDefaultBC,cRestType,  rEmployState,bLongIDCard,cpersonbarcode,MaxLeadNum,MaxPAccountNum,MaxAccountNum,MaxOpportunityNum )  values (  '" + p.cPersonCode + "','" + p.cPersonName + "','" + p.cDepCode + "','101','" + p.rSex + "','0','1',  '" + p.cPsnMobilePhone + "','" + guid + "', '0','1','0' ,'1', '001' , '00', '0', '0' , '0', '1', '0000', '00',  '10','0', '||HR31|" + p.cPersonCode + "', null, null, null, null ) ";
				sqlList.Add(sql);
				sql = " insert into person (cPersonCode,cPersonName,cDepCode,dPValidDate,dPInValidDate)  values('" + p.cPersonCode + "','" + p.cPersonName + "','" + p.cDepCode + "'," + validDate + "," + invalidDate + ") ";
				sqlList.Add(sql);
			}
			else
			{
				sql = " update Hr_hi_Person set cPsn_Name='" + p.cPersonName + "',cDept_num='" + p.cDepCode + "',  rSex='" + p.rSex + "',cPsnMobilePhone='" + p.cPsnMobilePhone + "'  where cPsn_Num = '" + p.cPersonCode + "' ";
				sqlList.Add(sql);
				sql = " update person set cPersonName='" + p.cPersonName + "',cDepCode='" + p.cDepCode + "' ,  dPValidDate = " + validDate + ", dPInValidDate = " + invalidDate + "   where cPersonCode = '" + p.cPersonCode + "' ";
				sqlList.Add(sql);
			}
			int result = U8SqlDBHelper.ExecuteSqlTran(sqlList);
			if (result > 0)
			{
				return "{\"Code\":\"200\",\"Msg\":\"U8人员档案[" + p.cPersonCode + "]上传成功！\",\"U8Code\":\"" + p.cPersonCode + "\" }";
			}
			return "{\"Code\":\"400\",\"Msg\":\"U8人员档案上传失败！\",\"U8Code\":\"\" }";
		}
		catch (Exception ex)
		{
			LogException.WriteLog(ex, sql);
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			return "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"U8Code\":\"\"}";
		}
	}
}
}
