using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Neo4jClient;
using Social_GraphWeb.Graph;

namespace Social_GraphWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            //var graphScaffolding = new Scaffolding();

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

		public ActionResult Registrations()
		{
			var client = new GraphClient(new Uri("http://10.4.0.229:7474/db/data"));

			client.Connect();

			var patrickReference = new NodeReference(2, client); //hard-coded
			var patrick = client.Get<Person>(patrickReference);

			Console.WriteLine(patrick.Data.FirstName);

			var fluentQuery = client.Cypher
				.Start("me", patrickReference)
				.Match("me-[:registered_for]->session<-[:registered_for]-others")
				.Return<Person>("others");

			return View(fluentQuery.Results);
		}
    }

	public class Person
	{
		public string FirstName { get; set; }
	}

	public class Session
	{
		public string SessionName { get; set; }
	}

	public class Program
	{
		public string ProgramName { get; set; }
	}
}
