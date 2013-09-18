using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Social_GraphWeb.Graph;

namespace Social_GraphWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            var graphScaffolding = new Scaffolding();

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
