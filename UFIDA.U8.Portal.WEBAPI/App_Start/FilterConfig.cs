using System.Web;
using System.Web.Mvc;

namespace UFIDA.U8.Portal.WEBAPI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
