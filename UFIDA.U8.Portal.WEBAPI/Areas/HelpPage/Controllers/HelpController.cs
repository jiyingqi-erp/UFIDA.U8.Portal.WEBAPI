using System;
using System.Web.Http;
using System.Web.Mvc;
using UFIDA.U8.Portal.WEBAPI.Areas.HelpPage.Models;

namespace UFIDA.U8.Portal.WEBAPI.Areas.HelpPage.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the help page.
    /// </summary>
    public class HelpController : Controller
    {
        public HttpConfiguration Configuration { get; private set; }

        public HelpController()
            : this(GlobalConfiguration.Configuration)
        {
        }

        public HelpController(HttpConfiguration config)
        {
            Configuration = config;
        }

        public ActionResult Index()
        {
            base.ViewBag.DocumentationProvider = Configuration.Services.GetDocumentationProvider();
            return View(Configuration.Services.GetApiExplorer().ApiDescriptions);
        }

        public ActionResult Api(string apiId)
        {
            if (!string.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel helpPageApiModel = Configuration.GetHelpPageApiModel(apiId);
                if (helpPageApiModel != null)
                {
                    return View(helpPageApiModel);
                }
            }
            return View("Error");
        }
    }
}