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
    /// 销售订单审核接口
    /// 路由: /api/SaleOrderAudit
    /// 方法: POST
    /// 
    /// 请求参数（JSON）:
    ///   cCode       - 销售订单号（必填）       例: "SO20240725001"
    ///   cVerifier   - 审核人用户名（必填）     例: "张三"
    ///   dVerifyDate - 审核日期（可选）         例: "2024-07-25"（不传则取服务器当前时间）
    /// 
    /// 成功响应:
    ///   {"Code":"200","Msg":"审核成功！","U8Code":"SO20240725001","Items":""}
    /// 
    /// 失败响应:
    ///   {"Code":"400","Msg":"错误描述","U8Code":"","Items":""}
    /// </summary>
    public class SaleOrderAuditController : ApiController
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
                SaleOrderAudit so;
                try
                {
                    so = JsonConvert.DeserializeObject<SaleOrderAudit>(requestData);
                }
                catch
                {
                    responseData = "{\"Code\":\"400\",\"Msg\":\"接口请求失败！数据格式错误！\",\"Items\":\"\"}";
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(responseData, Encoding.UTF8, "application/json")
                    };
                }
                responseData = SaleOrderAuditDal.Audit(so);
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
                LogException.WriteJSlog("SaleOrderAudit", requestData, responseData);
            }
        }
    }
}
