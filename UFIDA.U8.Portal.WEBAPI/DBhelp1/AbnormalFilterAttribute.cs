using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Tracing;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;

namespace UFIDA.U8.Portal.WEBAPI.DBhelp1
{
    public class AbnormalFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new AppLog());
            ITraceWriter traceWriter = GlobalConfiguration.Configuration.Services.GetTraceWriter();
            traceWriter.Error(actionExecutedContext.Request, "Controller : " + actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerType.FullName + Environment.NewLine + "Action : " + actionExecutedContext.ActionContext.ActionDescriptor.ActionName, actionExecutedContext.Exception);
            Type type = actionExecutedContext.Exception.GetType();
            if (type == typeof(ValidationException))
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(actionExecutedContext.Exception.Message),
                    ReasonPhrase = "ValidationException"
                };
                throw new HttpResponseException(response);
            }
            if (type == typeof(UnauthorizedAccessException))
            {
                throw new HttpResponseException(actionExecutedContext.Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            throw new HttpResponseException(actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError));
        }
    }
}
