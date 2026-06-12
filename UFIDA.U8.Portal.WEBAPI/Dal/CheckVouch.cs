using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using UFIDA.U8.Portal.WEBAPI.DBHelp;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{

public class CheckVouch
{
	public static string CheckVouchAdd(string json)
	{
		Result result = new Result();
		DataResult dataResult = new DataResult();
		if (string.IsNullOrEmpty(json))
		{
			result.Code = "400";
			result.Msg = "请求参数为空";
		}
		else
		{
			try
			{
				List<string> list = new List<string>();
				string text = ConfigurationManager.AppSettings["DBNAME"].ToString().Substring(7, 3);
				int num = 0;
				string connStr = "";
				string dbName = ConfigurationManager.AppSettings["DBNAME"].ToString();
				string sqlQuery = "";
				string resultStr = "";
				bool flag = false;
				bool flag2 = false;
				UFIDA.U8.Portal.WEBAPI.Models.CheckVouch checkVouch = JsonConvert.DeserializeObject<UFIDA.U8.Portal.WEBAPI.Models.CheckVouch>(json);
				string ccvcode = checkVouch.ccvcode;
				resultStr = "select * from checkVouch where cCVCode='" + ccvcode + "'";
				DataTable dataTable = SQLHelper.GetDataTable(resultStr);
				if (dataTable.Rows.Count > 0)
				{
					num++;
					connStr = connStr + "单据号：" + ccvcode + "已存在！";
				}
				string dcvdate = checkVouch.dcvdate;
				if (string.IsNullOrEmpty(dcvdate))
				{
					num++;
					connStr += "盘点日期不能为空！";
				}
				string dacdate = checkVouch.dacdate;
				if (string.IsNullOrEmpty(dacdate))
				{
					num++;
					connStr += "账面日期不能为空！";
				}
				string cwhcode = checkVouch.cwhcode;
				if (string.IsNullOrEmpty(cwhcode))
				{
					num++;
					connStr += "仓库不能为空！";
				}
				else
				{
					resultStr = "select * from warehouse where cwhcode='" + cwhcode + "'";
					dataTable = SQLHelper.GetDataTable(resultStr);
					if (dataTable.Rows.Count == 0)
					{
						num++;
						connStr = connStr + "仓库：" + cwhcode + "不存在！";
					}
					else
					{
						flag = Convert.ToBoolean(dataTable.Rows[0]["bWhPos"]);
					}
				}
				string ccvmemo = checkVouch.ccvmemo;
				string cmaker = checkVouch.cmaker;
				if (string.IsNullOrEmpty(cmaker))
				{
					num++;
					connStr += "制单人不能为空！";
				}
				string caccounter = checkVouch.caccounter;
				if (string.IsNullOrEmpty(caccounter))
				{
					num++;
					connStr += "审核人不能为空！";
				}
				int iFatherId = 0;
				int iChildId = 0;
				string vouchType = "ch";
				string sqlment = VoucherCode.GetVoucherID(text, vouchType, out iFatherId, out iChildId, out sqlment);
				if (!string.IsNullOrEmpty(sqlment))
				{
					list.Add(sqlment);
				}
				resultStr = "update UFSystem..UA_Identity set iFatherId=iFatherId+1 where cAcc_Id = '" + text + "' and cVouchType='" + vouchType + "' ";
				list.Add(resultStr);
				iFatherId++;
				string fatherIdStr = (1000000000 + iFatherId).ToString();
				string barcodeStr = "||st18|" + ccvcode;
				resultStr = "Insert Into CheckVouch(\r\n                            ccvcode, dcvdate, cdepcode, cpersoncode, cirdcode,\r\n                            cordcode, cwhcode, ccvbatch, ccvmemo, cdefine1,\r\n                            cdefine2, cdefine3, cdefine4, cdefine5, cdefine6,\r\n                            cdefine7, cdefine8, cdefine9, cdefine10, caccounter,\r\n                            cmaker, cposition, dacdate, id, vt_id,\r\n                            btransflag, cdefine11, cdefine12, cdefine13, cdefine14,\r\n                            cdefine15, cdefine16, ccvtype, ccvperiod, csource,\r\n                            bposcheck, ireturncount, iverifystate, iswfcontrolled, cmodifyperson,\r\n                            dmodifydate, dnmaketime, dnmodifytime, dnverifytime, dveridate,\r\n                            cbustype, csourcecodels, iprintcount, csysbarcode, ccurrentauditor,\r\n                            bwirelessvouch\r\n                        )\r\n                        Values (\r\n                            N'" + ccvcode + "', N'" + dcvdate + "', Null, Null, Null,\r\n                            Null, N'" + cwhcode + "', Null, " + PMethod.setvalue(2, ccvmemo) + ", Null,\r\n                            Null, Null, Null, Null, Null,\r\n                            Null, Null, Null, Null, Null,\r\n                            N'" + cmaker + "', Null, N'" + dacdate + "', " + fatherIdStr + ", 29,\r\n                            0, Null, Null, Null, Null,\r\n                            Null, Null, N'普通仓库盘点', Null, N'1',\r\n                            N'0', Null, Null, 0, Null,\r\n                            Null, getdate(), Null, Null, Null,\r\n                            Null, Null, 0, '" + barcodeStr + "', Null,\r\n                            Null)";
				list.Add(resultStr);
				resultStr = "Update CheckVouch  WITH (UPDLOCK)  Set cAccounter=N'" + caccounter + "' , dVeriDate = N'" + dcvdate + "',dNVerifyTime=getdate() Where ID=" + fatherIdStr + " ";
				list.Add(resultStr);
				List<string> list2 = new List<string>();
				int iFatherId2 = 0;
				int iChildId2 = 0;
				string rdType = "rd";
				sqlment = VoucherCode.GetVoucherID(text, rdType, out iFatherId2, out iChildId2, out sqlment);
				if (!string.IsNullOrEmpty(sqlment))
				{
					list2.Add(sqlment);
				}
				iFatherId2++;
				string fatherId2Str = (1000000000 + iFatherId2).ToString();
				resultStr = "update UFSystem..UA_Identity set iFatherId=iFatherId+1 where cAcc_Id = '" + text + "' and cVouchType='" + rdType + "' ";
				list2.Add(resultStr);
				int cNumber;
				string sqlStatement;
				string voucherCode = VoucherCode.GetVoucherCode(dbName, "0301", out cNumber, out sqlStatement);
				list2.Add(sqlStatement);
				string barcodeStr2 = "||st08|" + voucherCode;
				resultStr = "insert into rdrecord08(id,brdflag,cvouchtype,cbustype,csource,cbuscode,cwhcode,ddate,ccode,cmaker,\r\n                            vt_id,bisstqc,bomfirst,iswfcontrolled,dnmaketime,dnmodifytime,dnverifytime,csysbarcode) \r\n                            values (N'" + fatherId2Str + "',N'1',N'08',N'盘盈入库',N'盘点',N'" + ccvcode + "',N'" + cwhcode + "',N'" + dcvdate + "',N'" + voucherCode + "',N'" + cmaker + "',\r\n                            67,0,0,0, getdate(), Null , Null,'" + barcodeStr2 + "' )";
				list2.Add(resultStr);
				List<string> list3 = new List<string>();
				iFatherId2++;
				string fatherId3Str = (1000000000 + iFatherId2).ToString();
				resultStr = "update UFSystem..UA_Identity set iFatherId=iFatherId+1 where cAcc_Id = '" + text + "' and cVouchType='" + rdType + "' ";
				list3.Add(resultStr);
				string voucherCode2 = VoucherCode.GetVoucherCode(dbName, "0302", out cNumber, out sqlStatement);
				list3.Add(sqlStatement);
				string barcodeStr3 = "||st09|" + voucherCode2;
				resultStr = "insert into rdrecord09(id,brdflag,cvouchtype,cbustype,csource,cbuscode,cwhcode,ddate,ccode,cmaker,\r\n                            vt_id,bisstqc,bomfirst,ibg_overflag,cbg_auditor,cbg_audittime,controlresult,iswfcontrolled,dnmaketime,dnmodifytime,\r\n                            dnverifytime,csysbarcode)\r\n                            values (N'" + fatherId3Str + "',N'0',N'09',N'盘亏出库',N'盘点',N'" + ccvcode + "',N'" + cwhcode + "',N'" + dcvdate + "',N'" + voucherCode2 + "',N'" + cmaker + "',\r\n                            85,0,0,0,N'',N'',-1,0, getdate(), Null ,\r\n                            Null ,'" + barcodeStr3 + "')";
				list3.Add(resultStr);
				List<ItemsItem> list4 = new List<ItemsItem>();
				int num2 = 0;
				int num3 = 0;
				int count = checkVouch.Items.Count;
				for (int i = 0; i < count; i++)
				{
					ItemsItem itemsItem = new ItemsItem();
					bool flag3 = false;
					string cinvcode = checkVouch.Items[i].cinvcode;
					if (string.IsNullOrEmpty(cinvcode))
					{
						num++;
						connStr = connStr + "第[" + (i + 1) + "]行存货编码不能为空！";
					}
					else
					{
						resultStr = "select * from inventory where cinvcode='" + cinvcode + "'";
						dataTable = SQLHelper.GetDataTable(resultStr);
						if (dataTable.Rows.Count == 0)
						{
							num++;
							connStr = connStr + "第[" + (i + 1) + "]行存货编码：" + cinvcode + "不存在！";
						}
						else
						{
							flag3 = Convert.ToBoolean(dataTable.Rows[0]["bInvBatch"]);
						}
					}
					string cbatch = checkVouch.Items[i].cbatch;
					if (flag3 && string.IsNullOrEmpty(cbatch))
					{
						num++;
						connStr = connStr + "第[" + (i + 1) + "]行存货编码：" + cinvcode + "需要批次！";
					}
					string adinQty = (string.IsNullOrEmpty(checkVouch.Items[i].iadinquantity) ? "0" : checkVouch.Items[i].iadinquantity);
					string adoutQty = (string.IsNullOrEmpty(checkVouch.Items[i].iadoutquantity) ? "0" : checkVouch.Items[i].iadoutquantity);
					string cvQty = (string.IsNullOrEmpty(checkVouch.Items[i].icvquantity) ? "0" : checkVouch.Items[i].icvquantity);
					string cvcQty = (string.IsNullOrEmpty(checkVouch.Items[i].icvcquantity) ? "0" : checkVouch.Items[i].icvcquantity);
					decimal num4 = Convert.ToDecimal(cvcQty) - Convert.ToDecimal(cvQty) - Convert.ToDecimal(adinQty) + Convert.ToDecimal(adoutQty);
					string cposition = checkVouch.Items[i].cposition;
					if (flag && string.IsNullOrEmpty(cposition))
					{
						num++;
						connStr = connStr + "第[" + (i + 1) + "]行存货编码：" + cinvcode + "需要货位！";
					}
					string cdefine = checkVouch.Items[i].cdefine23;
					iChildId++;
					string childIdStr = (1000000000 + iChildId).ToString();
					string childBarcode = "||st18|" + ccvcode + "| " + (i + 1);
					resultStr = "Insert Into CheckVouchs(\r\n                                ccvcode,cinvcode,rdsid,icvnum,icvquantity,\r\n                                icvcnum,icvcquantity,ccvbatch,cfree1,cfree2,\r\n                                ccvreason,ddisdate,ijhdj,ijhje,isjdj,\r\n                                isjje,cposition,cdefine22,cdefine23,cdefine24,\r\n                                cdefine25,cdefine26,cdefine27,citemcode,citem_class,\r\n                                cname,citemcname,autoid,id,cbarcode,\r\n                                iadinquantity,iadinnum,iadoutquantity,iadoutnum,iallowwaste,\r\n                                iactualwaste,cfree3,cfree4,cfree5,cfree6,\r\n                                cfree7,cfree8,cfree9,cfree10,cdefine28,\r\n                                cdefine29,cdefine30,cdefine31,cdefine32,cdefine33,\r\n                                cdefine34,cdefine35,cdefine36,cdefine37,cassunit,\r\n                                cbvencode,cinvouchcode,imassdate,dmadedate,cmassunit,\r\n                                isotype,isodid,cvmivencode,iinvexchrate,iexpiratdatecalcu,\r\n                                cexpirationdate,dexpirationdate,cbatchproperty1,cbatchproperty2,cbatchproperty3,\r\n                                cbatchproperty4,cbatchproperty5,cbatchproperty6,cbatchproperty7,cbatchproperty8,\r\n                                cbatchproperty9,cbatchproperty10,cciqbookcode,cbmemo,cwhpersoncode,\r\n                                cwhpersonname,irowno,cinvouchtype,strowguid,cbsysbarcode,\r\n                                bneedrecheck,recheckstatus,checkcode,checkautoid)\r\n                                Values (\r\n                                N'" + ccvcode + "',N'" + cinvcode + "',Null,0," + PMethod.setvalue(2, cvQty) + ",\r\n                                0," + PMethod.setvalue(2, cvcQty) + ",Null,Null,Null,\r\n                                Null,Null,0,0,0,\r\n                                0,Null,Null,Null,Null,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null,Null," + childIdStr + "," + fatherIdStr + ",Null,\r\n                                " + PMethod.setvalue(2, adinQty) + ",0," + PMethod.setvalue(2, adoutQty) + ",0,Null,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null,Null,Null,Null,0,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null,Null,Null,Null,Null,\r\n                                Null," + (i + 1) + ",Null,Null,'" + childBarcode + "',\r\n                                Null,Null,Null,Null)";
					list.Add(resultStr);
					itemsItem.U8IDS = childIdStr;
					itemsItem.FID = "";
					list4.Add(itemsItem);
					if (num4 > 0m)
					{
						iChildId2++;
						string connStr0 = (1000000000 + iChildId2).ToString();
						num2++;
						string connStr1 = ("||st08|" + voucherCode + "|" + num2) ?? "";
						resultStr = "Insert Into rdrecords08(autoid,id,cinvcode,inum,iquantity,iunitcost,iprice,ipunitcost,ipprice,cbatch,\r\n                                    cvouchcode,cinvouchcode,cinvouchtype,isoutquantity,isoutnum,cfree1,cfree2,dvdate,itrids,cposition,\r\n                                    cdefine22,cdefine23,cdefine24,cdefine25,cdefine26,cdefine27,citem_class,citemcode,cname,citemcname,\r\n                                    cfree3,cfree4,cfree5,cfree6,cfree7,cfree8,cfree9,cfree10,cbarcode,inquantity,\r\n                                    innum,cassunit,dmadedate,imassdate,cdefine28,cdefine29,cdefine30,cdefine31,cdefine32,cdefine33,\r\n                                    cdefine34,cdefine35,cdefine36,cdefine37,icheckids,cbvencode,ccheckcode,icheckidbaks,crejectcode,irejectids,\r\n                                    ccheckpersoncode,dcheckdate,cmassunit,irsrowno,ioritrackid,coritracktype,cbaccounter,dbkeepdate,bcosting,bvmiused,\r\n                                    ivmisettlequantity,ivmisettlenum,cvmivencode,iinvsncount,cserviceoid,cbserviceoid,iinvexchrate,corufts,iexpiratdatecalcu,cexpirationdate,\r\n                                    dexpirationdate,cciqbookcode,ibondedsumqty,isodid,isotype,csocode,isoseq,cbatchproperty1,cbatchproperty2,cbatchproperty3,\r\n                                    cbatchproperty4,cbatchproperty5,cbatchproperty6,cbatchproperty7,cbatchproperty8,cbatchproperty9,cbatchproperty10,cbmemo,irowno,strowguid,\r\n                                    cbsourcecodels,igroupno,idebitids,idebitchildids,cbsysbarcode,icrmvouchids,ccrmvouchcode,iposflag,csyssourceautoid)\r\n                                    Values (" + connStr0 + "," + fatherId2Str + ",N'" + cinvcode + "',0," + num4 + ",Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null," + childIdStr + "," + PMethod.setvalue(2, cposition) + ",\r\n                                    Null," + PMethod.setvalue(2, cdefine) + ",Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,0,Null,Null,Null,Null,Null,1,0,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,0,Null,\r\n                                    Null,Null,Null,Null,0,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null," + num2 + ",Null,\r\n                                    Null,Null,Null,Null,'" + connStr1 + "',Null,Null,Null,Null) ";
						list2.Add(resultStr);
						if (!string.IsNullOrEmpty(cposition))
						{
							resultStr = " insert into InvPosition (RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,  iExpiratDateCalcu,cvouchtype,dVouchDate,cfree1, cfree2,cfree3, cfree4,cfree5, cfree6, cfree7, cfree8, cfree9, cfree10 )  values('" + connStr0 + "','" + fatherId2Str + "','" + cwhcode + "','" + cposition + "','" + cinvcode + "', '" + cbatch + "',  '" + num4 + "','','" + dcvdate + "',1,0,  null,null,  1,'08','" + dcvdate + "',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL) ";
							list2.Add(resultStr);
							resultStr = "  select * from invpositionsum where cwhcode = '" + cwhcode + "' and cposcode = '" + cposition + "' and cinvcode = '" + cinvcode + "'  ";
							DataTable dataTable2 = SQLHelper.GetDataTable(resultStr);
							if (dataTable2.Rows.Count == 0)
							{
								resultStr = "  insert into invpositionsum (cWhCode,cPosCode,cInvCode,iQuantity,cBatch,iTrackid,cfree1, cfree2,cfree3, cfree4,cfree5, cfree6, cfree7, cfree8, cfree9, cfree10)   values('" + cwhcode + "','" + cposition + "','" + cinvcode + "','" + num4 + "','" + cbatch + "', 0 ,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL ) ";
								list2.Add(resultStr);
							}
							else
							{
								resultStr = "  update invpositionsum set iquantity = iquantity + " + num4 + "   where cwhcode = '" + cwhcode + "' and cposcode = '" + cposition + "' and cinvcode = '" + cinvcode + "' ";
								list2.Add(resultStr);
							}
						}
						resultStr = "update UFSystem..UA_Identity set iChildId=iChildId+1 where cAcc_Id = '" + text + "' and cVouchType='" + rdType + "'";
						list2.Add(resultStr);
					}
					if (num4 < 0m)
					{
						iChildId2++;
						string connStr2 = (1000000000 + iChildId2).ToString();
						num3++;
						string connStr3 = ("||st09|" + voucherCode2 + "|" + num3) ?? "";
						resultStr = "Insert Into rdrecords09(autoid,id,cinvcode,inum,iquantity,iunitcost,iprice,ipunitcost,ipprice,cbatch,\r\n                                    cvouchcode,cinvouchcode,cinvouchtype,isoutquantity,isoutnum,coutvouchid,coutvouchtype,isredoutquantity,isredoutnum,cfree1,\r\n                                    cfree2,dvdate,itrids,cposition,cdefine22,cdefine23,cdefine24,cdefine25,cdefine26,cdefine27,\r\n                                    citem_class,citemcode,idlsid,cname,citemcname,cfree3,cfree4,cfree5,cfree6,cfree7,\r\n                                    cfree8,cfree9,cfree10,cbarcode,inquantity,innum,cassunit,dmadedate,imassdate,cdefine28,\r\n                                    cdefine29,cdefine30,cdefine31,cdefine32,cdefine33,cdefine34,cdefine35,cdefine36,cdefine37,icheckids,\r\n                                    cbvencode,ccheckcode,icheckidbaks,crejectcode,irejectids,ccheckpersoncode,dcheckdate,cmassunit,ieqdid,cbaccounter,\r\n                                    dbkeepdate,bcosting,bvmiused,ivmisettlequantity,ivmisettlenum,cvmivencode,iinvsncount,cserviceoid,cbserviceoid,iinvexchrate,\r\n                                    cbdlcode,corufts,strcontractguid,iexpiratdatecalcu,cexpirationdate,dexpirationdate,cciqbookcode,ibondedsumqty,ccusinvcode,ccusinvname,\r\n                                    isodid,isotype,csocode,isoseq,cbatchproperty1,cbatchproperty2,cbatchproperty3,cbatchproperty4,cbatchproperty5,cbatchproperty6,\r\n                                    cbatchproperty7,cbatchproperty8,cbatchproperty9,cbatchproperty10,cbmemo,irowno,strowguid,cbsourcecodels,igroupno,idebitids,\r\n                                    idebitchildids,iimosid,cpoid,strcontractid,strcode,cinvoucherlineid,cinvouchercode,cinvouchertype,cbsysbarcode,ipickedquantity,\r\n                                    ipickednum,icrmvouchids,ccrmvouchcode,iposflag,isrcvouchids,csyssourceautoid)\r\n                                    Values (" + connStr2 + "," + fatherId3Str + ",N'" + cinvcode + "',0," + -num4 + ",Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null," + childIdStr + ",Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,0,Null,Null,\r\n                                    Null,1,0,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,0,Null,Null,Null,Null,Null,Null,\r\n                                    Null,0,Null,Null,Null,Null,Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null," + num3 + ",Null,Null,Null,Null,\r\n                                    Null,Null,Null,Null,Null,Null,Null,Null,'" + connStr3 + "',Null,\r\n                                    Null,Null,Null,Null,Null,Null)";
						list3.Add(resultStr);
						if (!string.IsNullOrEmpty(cposition))
						{
							resultStr = " insert into " + dbName + "..InvPosition (RdsID,RdID,cWhCode,cPosCode,cInvCode,cBatch,  iQuantity,cHandler,dDate,bRdFlag,iTrackId,  iMassDate,cMassUnit,  iExpiratDateCalcu,cvouchtype,dVouchDate,cfree1, cfree2,cfree3, cfree4,cfree5, cfree6, cfree7, cfree8, cfree9, cfree10 )  values('" + connStr2 + "','" + fatherId3Str + "','" + cwhcode + "','" + cposition + "','" + cinvcode + "', '" + cbatch + "',  '" + -num4 + "','','" + dcvdate + "',0,0,  null,null,  1,'09','" + dcvdate + "',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL) ";
							list3.Add(resultStr);
							resultStr = "  select * from " + dbName + "..invpositionsum where cwhcode = '" + cwhcode + "' and cposcode = '" + cposition + "' and cinvcode = '" + cinvcode + "' ";
							DataTable dataTable3 = SQLHelper.GetDataTable(resultStr);
							if (dataTable3.Rows.Count == 0)
							{
								resultStr = "  insert into " + dbName + "..invpositionsum (cWhCode,cPosCode,cInvCode,iQuantity,cBatch,iTrackid,cfree1, cfree2,cfree3, cfree4,cfree5, cfree6, cfree7, cfree8, cfree9, cfree10)   values('" + cwhcode + "','" + cposition + "','" + cinvcode + "','" + -num4 + "','" + cbatch + "', 0 ,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL) ";
								list3.Add(resultStr);
							}
							else
							{
								resultStr = "  update " + dbName + "..invpositionsum set iquantity = iquantity - " + -num4 + "   where cwhcode = '" + cwhcode + "' and cposcode = '" + cposition + "' and cinvcode = '" + cinvcode + "' ";
								list3.Add(resultStr);
							}
						}
						resultStr = "update UFSystem..UA_Identity set iChildId=iChildId+1 where cAcc_Id = '" + text + "' and cVouchType='" + rdType + "'";
						list3.Add(resultStr);
					}
					resultStr = "update UFSystem..UA_Identity set iChildId=iChildId+1 where cAcc_Id = '" + text + "' and cVouchType='" + vouchType + "'";
					list.Add(resultStr);
				}
				if (num2 > 0)
				{
					resultStr = "exec ST_SaveForStock N'08',N'" + fatherId2Str + "',0,0,1";
					list2.Add(resultStr);
					resultStr = "exec ST_SaveForTrackStock N'08',N'" + fatherId2Str + "',0,1";
					list2.Add(resultStr);
					resultStr = "exec IA_SP_WriteUnAccountVouchForST N'" + fatherId2Str + "',N'08'";
					list2.Add(resultStr);
					list = list.Concat(list2).ToList();
				}
				if (num3 > 0)
				{
					resultStr = "exec ST_SaveForStock N'09',N'" + fatherId3Str + "',0,0,1";
					list3.Add(resultStr);
					resultStr = "exec ST_SaveForTrackStock N'09',N'" + fatherId3Str + "',0,1";
					list3.Add(resultStr);
					resultStr = "exec IA_SP_WriteUnAccountVouchForST N'" + fatherId3Str + "',N'09'";
					list3.Add(resultStr);
					list = list.Concat(list3).ToList();
				}
				dataResult.U8Code = ccvcode;
				dataResult.U8ID = fatherIdStr;
				dataResult.Items = list4;
				if (num == 0)
				{
					if (SQLHelper.ExecuteSqlTran(list) > 0)
					{
						result.Code = "200";
						result.Msg = "新增成功";
						result.data = dataResult;
					}
					else
					{
						result.Code = "500";
						result.Msg = "新增失败" + connStr;
					}
				}
				else
				{
					result.Code = "500";
					result.Msg = connStr;
				}
			}
			catch (Exception ex)
			{
				result.Code = "500";
				result.Msg = "接口异常：" + ex.Message;
			}
		}
		return JsonConvert.SerializeObject((object)result);
	}
}
}
