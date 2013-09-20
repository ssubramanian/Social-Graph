using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Neo4jClient;
using Neo4jClient.Cypher;
using Social_GraphWeb.Controllers;
using Social_GraphWeb.Models;

namespace Social_GraphWeb.Specs
{
	[TestFixture]
	public class BuildAGraph
	{
		 [Test]
		 public void TrySomething()
		 {
			 var client = new GraphClient(new Uri("http://10.4.0.229:7474/db/data"));
			 client.Connect();

			 var meReference = new NodeReference<Person>(15, client);
			 var donorReference = new NodeReference<Person>(9, client);


			 // shameless code stealing from http://geekswithblogs.net/cskardon/archive/2013/07/23/neo4jclient-ndash-getting-path-results.aspx
			 ICollection<PathsResult<Person, Knows>> paths = client.ShortesPathsBetween<Person, Knows>(meReference, donorReference);

			 var path = paths.First();

			 foreach (var relationshipInstance in path.Relationships)
			 {
				 var startNode = path.Nodes.First(n => n.Reference == relationshipInstance.StartNodeReference);
				 var endNode = path.Nodes.First(n => n.Reference == relationshipInstance.EndNodeReference);

				 Console.WriteLine("{0} {1} {2}", startNode.Data.FirstName, relationshipInstance.TypeKey, endNode.Data.FirstName);
			 }
		 }

		[Test]
		public void ConnectToAssociation()
		{
			var client = new GraphClient(new Uri("http://10.4.0.229:7474/db/data"));
			client.Connect();

			var query = client.Cypher
				  .Start(new { n = All.Nodes })
			      .Return<Node<Person>>("n");

			var associationNode = client.Get<Association>(new NodeReference(5));

			foreach (var person in query.Results)
			{
				if (!string.IsNullOrEmpty(person.Data.FirstName))
				{

					var relationship = client.CreateRelationship(person.Reference, new IsMember(associationNode.Reference));
					
				}
			}
		}
	}

	
}