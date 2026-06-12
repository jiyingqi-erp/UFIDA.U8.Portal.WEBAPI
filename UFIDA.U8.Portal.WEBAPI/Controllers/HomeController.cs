using System.Web.Mvc;

namespace UFIDA.U8.Portal.WEBAPI.Controllers
{
    public class HomeController : Controller
    {
	public ActionResult Index()
	{
		ViewBag.Title = "Home Page";
		return View();
	}
}
}
