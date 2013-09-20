using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Neo4jClient;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Social_GraphWeb.Graph;
using Social_GraphWeb.Models;

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

		public ActionResult Path()
		{
			var client = new GraphClient(new Uri("http://10.4.0.229:7474/db/data"));

			client.Connect();

			var meReference = new NodeReference<Person>(15, client);
			var donorReference = new NodeReference<Person>(9, client);


			// shameless code stealing from http://geekswithblogs.net/cskardon/archive/2013/07/23/neo4jclient-ndash-getting-path-results.aspx
			ICollection<PathsResult<Person, Knows>> paths = client.ShortesPathsBetween<Person, Knows>(meReference, donorReference);


			var pathViewModel = new PathViewModel()
				{
					MyId = meReference.Id,
					Path = paths.First()
				};

			
			return View(pathViewModel);
		}
    }

	public class PathsResult<TNode, TRelationship> where TRelationship : Relationship, new()
	{
		public IEnumerable<Node<TNode>> Nodes { get; set; }
		public IEnumerable<RelationshipInstance<TRelationship>> Relationships { get; set; }
	}

	public class PathViewModel
	{
		public PathsResult RawPath { get; set; }

		public PathsResult<Person, Knows> Path { get; set; }

		public long MyId { get; set; }
	}
}
