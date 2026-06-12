using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;

namespace UFIDA.U8.Portal.WEBAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Filters.Add(new LogFilterAttribute());
            config.Filters.Add(new AbnormalFilterAttribute());
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new
            {
                id = RouteParameter.Optional
            });
            JsonSerializerSettings serializerSettings = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;
            serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            serializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
        }
    }
}