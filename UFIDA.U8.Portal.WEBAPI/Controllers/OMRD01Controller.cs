using System;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Dal;
using UFIDA.U8.Portal.WEBAPI.Models;

namespace UFIDA.U8.Portal.WEBAPI.Controllers
{
    public class OMRD01Controller : ApiController
    {
	public HttpResponseMessage Post([FromBody] dynamic json)
	{
		string requestData = "";
		string responseData = "";
		try
		{
			requestData = Convert.ToString(json);
			requestData = BasicDAL.PreTrans(requestData);
			requestData = HttpUtility.UrlDecode(requestData, Encoding.UTF8);
			RD01 rd;
			try
			{
				rd = JsonConvert.DeserializeObject<RD01>(requestData);
			}
			catch
			{
				responseData = "{\"Code\":\"400\",\"Msg\":\"接口请求失败！数据格式错误！\",\"Items\":\"\"}";
				return new HttpResponseMessage
				{
					Content = new StringContent(responseData, Encoding.UTF8, "application/json")
				};
			}
			responseData = RDSDAL.OMRD01(rd);
			return new HttpResponseMessage
			{
				Content = new StringContent(responseData, Encoding.UTF8, "application/json")
			};
		}
		catch (Exception ex)
		{
			string errorMsg = "接口请求失败！原因：" + ex.Message;
			responseData = "{\"Code\":\"400\",\"Msg\":\"" + errorMsg + "\",\"Items\":\"\"}";
			return new HttpResponseMessage
			{
				Content = new StringContent(responseData, Encoding.UTF8, "application/json")
			};
		}
		finally
		{
			LogException.WriteJSlog("OMRD01", requestData, responseData);
		}
	}
}
}
