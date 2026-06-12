using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using UFIDA.U8.Portal.WEBAPI.Dal;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;
using UFIDA.U8.Portal.WEBAPI.Models;
using static System.Net.Mime.MediaTypeNames;

namespace UFIDA.U8.Portal.WEBAPI.Controllers
{
    public class OrderController : ApiController
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
                MomOrder mo;
                try
                {
                    mo = JsonConvert.DeserializeObject<MomOrder>(requestData);
                }
                catch
                {
                    responseData = "{\"Code\":\"400\",\"Msg\":\"接口请求失败！数据格式错误！\",\"Items\":\"\"}";
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(responseData, Encoding.UTF8, "application/json")
                    };
                }
                responseData = MomOrderDAL.AddMom(mo);
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
                LogException.WriteJSlog("Order", requestData, responseData);
            }
        }
    }
}
