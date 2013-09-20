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
			var pathViewModel = QueryForPath(15, 9);

			return View("Path", pathViewModel);
		}

		public ActionResult KnownPeople(int startNodeId, int endNodeId)
		{
			// Example:  http://localhost:56035/Home/KnownPeople?startNodeId=15&endNodeId=9
			// Example:  http://localhost:56035/Home/KnownPeople?startNodeId=9&endNodeId=14

			var pathViewModel = QueryForPath(startNodeId, endNodeId);

			return View("Path", pathViewModel);
		}

		private PathViewModel QueryForPath(int startNodeId, int endNodeId)
		{
			var client = new GraphClient(new Uri("http://10.4.0.229:7474/db/data"));
			client.Connect();

			var startReference = new NodeReference<Person>(startNodeId, client);
			var endReference = new NodeReference<Person>(endNodeId, client);

			// shameless code stealing from http://geekswithblogs.net/cskardon/archive/2013/07/23/neo4jclient-ndash-getting-path-results.aspx
			ICollection<PathsResult<Person, Knows>> paths = client.ShortesPathsBetween<Person, Knows>(startReference, endReference);
			var me = client.Get<Person>(startReference);
			var donor = client.Get<Person>(endReference);

			var countOfHops = paths.First().Nodes.Count() - 1;

			var pathViewModel = new PathViewModel()
			{
				MyId = startReference.Id,
				Me = me,
				Target = donor,
				TargetId = endReference.Id,
				CountOfHops = countOfHops,
				Path = paths.First()
			};

			return pathViewModel;
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

		public long TargetId { get; set; }

		public Node<Person> Me { get; set; }

		public Node<Person> Target { get; set; }

		public int CountOfHops { get; set; }

		public string NodeName(Node<Person> node)
		{
			var firstName = node.Data.FirstName;

			if (node.Reference == Me.Reference)
				return firstName + " (you!)";

			if (node.Reference == Target.Reference)
				return firstName + " (big money!)";

			return firstName;
		}
	}
}
