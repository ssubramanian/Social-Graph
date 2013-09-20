using System;
using NUnit.Framework;
using Neo4jClient;

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


		 }
	}
}