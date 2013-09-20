using System.Collections.Generic;
using System.Linq;
using Neo4jClient;
using Neo4jClient.Cypher;
using Social_GraphWeb.Controllers;

namespace Social_GraphWeb.Models
{
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
				.Match(string.Format("p=a-[:{0}*1..{1}]-(z)", new TRelationship().RelationshipTypeKey, levels))
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
				.Match(string.Format("p=shortestPath(a-[:{0}*]-(z))", new TRelationship().RelationshipTypeKey))
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

	public class IsMember : Relationship, IRelationshipAllowingSourceNode<Person>, IRelationshipAllowingTargetNode<Association>
	{
		public IsMember() : base(-1)
		{
			
		}
		public IsMember(NodeReference targetNode) : base(targetNode)
		{
		}

		public IsMember(NodeReference targetNode, object data) : base(targetNode, data)
		{
		}

		public override string RelationshipTypeKey
		{
			get { return "is_member"; }
		}
	}
}