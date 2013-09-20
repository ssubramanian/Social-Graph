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
			// Example, path found:		http://localhost:56035/Home/KnownPeople?startNodeId=9&endNodeId=14
			// Example, no path found:	http://localhost:56035/Home/KnownPeople?startNodeId=2&endNodeId=14

			var pathViewModel = QueryForPath(startNodeId, endNodeId);
			if (pathViewModel.ShortestPath != null)
				return View("Path", pathViewModel);

			return View("NoPath", pathViewModel);
		}

		private PathViewModel QueryForPath(int startNodeId, int endNodeId)
		{
			var client = new GraphClient(new Uri("http://10.4.0.229:7474/db/data"));
			client.Connect();

			var startReference = new NodeReference<Person>(startNodeId, client);
			var endReference = new NodeReference<Person>(endNodeId, client);
			var start = client.Get<Person>(startReference);
			var end = client.Get<Person>(endReference);

			var shortestPaths = GetShortestPaths(client, startReference, endReference);

			var allPaths = GetAllPaths(client, startReference, endReference);

			int countOfHops = 0;
			var shortestPath = shortestPaths.FirstOrDefault();
			if (shortestPath != null && shortestPath.Nodes.Any())
				countOfHops = shortestPath.Nodes.Count() - 1;

			var pathViewModel = new PathViewModel()
				{
					MyId = startReference.Id,
					Me = start,
					Target = end,
					TargetId = endReference.Id,
					CountOfHops = countOfHops,
					ShortestPath = shortestPath,

					AllPaths = allPaths
				};

			return pathViewModel;
		}

	    private static IEnumerable<PathsResult<Person, Knows>> GetShortestPaths(GraphClient client, NodeReference<Person> startReference, NodeReference<Person> endReference)
	    {
		    var shortestPathsQuery = client.Cypher
		                                   .Start(new {a = startReference, z = endReference})
		                                   .Match("p=shortestPath(a-[:knows|teaches|takes_class_from|works_with*]->(z))")
		                                   .Return(p => new PathsResult<Person, Knows>
			                                   {
				                                   Nodes = Return.As<IEnumerable<Node<Person>>>("nodes(p)"),
				                                   Relationships = Return.As<IEnumerable<RelationshipInstance<Knows>>>("rels(p)")
			                                   });

		    ICollection<PathsResult<Person, Knows>> shortestPaths = shortestPathsQuery.Results.ToList();
		    return shortestPaths;
	    }

		private static IEnumerable<PathsResult<Person, Knows>> GetAllPaths(GraphClient client, NodeReference<Person> startReference, NodeReference<Person> endReference)
		{
			var allPathsQuery = client.Cypher
										   .Start(new { a = startReference, z = endReference })
										   .Match("p=a-[:knows|teaches|takes_class_from|works_with*1..7]->(z)")
										   .Where("ALL (x IN nodes(p) WHERE SINGLE (x2 IN nodes(p) WHERE x=x2))")
										   .Return(p => new PathsResult<Person, Knows>
										   {
											   Nodes = Return.As<IEnumerable<Node<Person>>>("nodes(p)"),
											   Relationships = Return.As<IEnumerable<RelationshipInstance<Knows>>>("rels(p)")
										   });

			ICollection<PathsResult<Person, Knows>> shortestPaths = allPathsQuery.Results.ToList();
			return shortestPaths;
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

		public PathsResult<Person, Knows> ShortestPath { get; set; }

		public long MyId { get; set; }

		public long TargetId { get; set; }

		public Node<Person> Me { get; set; }

		public Node<Person> Target { get; set; }

		public int CountOfHops { get; set; }

		public IEnumerable<PathsResult<Person, Knows>> AllPaths { get; set; }

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
