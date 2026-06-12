using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Tracing;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;

namespace UFIDA.U8.Portal.WEBAPI.DBhelp1
{
    public class LogFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new AppLog());
            ITraceWriter traceWriter = GlobalConfiguration.Configuration.Services.GetTraceWriter();
        }
    }
}
