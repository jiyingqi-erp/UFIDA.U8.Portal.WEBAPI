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
    /// <summary>
    /// 销售订单关闭接口
    /// 路由: /api/SaleOrderClose
    /// 方法: POST
    /// 
    /// 请求参数（JSON）:
    ///   {"saleorder":{"voucher_code":"0000000019","person_code":"00002"}}
    ///   
    ///   voucher_code - 销售订单号（必填）
    ///   person_code  - 关闭人员工编码（必填，对应 person.cPersonCode）
    /// 
    /// 成功响应:
    ///   {"Code":"200","Msg":"关闭成功！","U8Code":"0000000019","Items":""}
    /// 
    /// 失败响应:
    ///   {"Code":"400","Msg":"错误描述","U8Code":"","Items":""}
    /// </summary>
    public class SaleOrderCloseController : ApiController
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
                SaleOrderClose so;
                try
                {
                    so = JsonConvert.DeserializeObject<SaleOrderClose>(requestData);
                }
                catch
                {
                    responseData = "{\"Code\":\"400\",\"Msg\":\"接口请求失败！数据格式错误！\",\"Items\":\"\"}";
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(responseData, Encoding.UTF8, "application/json")
                    };
                }
                responseData = SaleOrderCloseDal.Close(so);
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
                LogException.WriteJSlog("SaleOrderClose", requestData, responseData);
            }
        }
    }
}
