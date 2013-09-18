using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Neo4jClient;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
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

	public static class Neo4JClientExtensions
	{
		public static ICollection<PathsResult<TNode, TRelationship>> Paths<TNode, TRelationship>(this IGraphClient client, NodeReference<TNode> rootNode, int levels = 1)
			where TRelationship : Relationship, new()
		{
			ICypherFluentQuery<PathsResult<TNode, TRelationship>> pathsQuery = client.Cypher
				.Start(new { n = rootNode })
				.Match(string.Format("p=n-[:{0}*1..{1}]->()", new TRelationship().RelationshipTypeKey, levels))
				.Return(p => new PathsResult<TNode, TRelationship>
				{
					Nodes = Return.As<IEnumerable<Node<TNode>>>("nodes(p)"),
					Relationships = Return.As<IEnumerable<RelationshipInstance<TRelationship>>>("rels(p)")
				});

			return pathsQuery.Results.ToList();
		}

		public static ICollection<PathsResult<TNode, TRelationship>> PathsBetween<TNode, TRelationship>(this IGraphClient client, NodeReference<TNode> startNode, NodeReference<TNode> endNode, int levels = 1)
			where TRelationship : Relationship, new()
		{
			ICypherFluentQuery<PathsResult<TNode, TRelationship>> pathsQuery = client.Cypher
				.Start(new { a = startNode, z = endNode })
				.Match(string.Format("p=a-[:{0}*1..{1}]->(z)", new TRelationship().RelationshipTypeKey, levels))
				.Return(p => new PathsResult<TNode, TRelationship>
				{
					Nodes = Return.As<IEnumerable<Node<TNode>>>("nodes(p)"),
					Relationships = Return.As<IEnumerable<RelationshipInstance<TRelationship>>>("rels(p)")
				});

			
			return pathsQuery.Results.ToList();
		}

		public static ICollection<PathsResult<TNode, TRelationship>> ShortesPathsBetween<TNode, TRelationship>(this IGraphClient client, NodeReference<TNode> startNode, NodeReference<TNode> endNode)
			where TRelationship : Relationship, new()
		{
			ICypherFluentQuery<PathsResult<TNode, TRelationship>> pathsQuery = client.Cypher
				.Start(new { a = startNode, z = endNode })
				.Match(string.Format("p=shortestPath(a-[:{0}*]->(z))", new TRelationship().RelationshipTypeKey))
				.Return(p => new PathsResult<TNode, TRelationship>
				{
					Nodes = Return.As<IEnumerable<Node<TNode>>>("nodes(p)"),
					Relationships = Return.As<IEnumerable<RelationshipInstance<TRelationship>>>("rels(p)")
				});


			return pathsQuery.Results.ToList();
		}
	}

	public class Knows : Relationship, IRelationshipAllowingSourceNode<Person>, IRelationshipAllowingTargetNode<Person>
	{
		public const string TypeKey = "knows";

		public Knows()
			: base(-1)
		{
		}

		public Knows(NodeReference targetNode)
			: base(targetNode)
		{
		}

		public Knows(NodeReference targetNode, object data)
			: base(targetNode, data)
		{
		}

		public override string RelationshipTypeKey
		{
			get { return TypeKey; }
		}
	}

	public class PathViewModel
	{
		public PathsResult RawPath { get; set; }

		public PathsResult<Person, Knows> Path { get; set; }

		public long MyId { get; set; }
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
