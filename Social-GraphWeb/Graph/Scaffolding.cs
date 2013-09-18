using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;

namespace Social_GraphWeb.Graph
{
    public class Scaffolding
    {
        GraphClient client;

        public Scaffolding()
        {
            client = new GraphClient(new Uri("http://localhost:7474/db/data"));

            client.Connect();

            //CreateNodesRelationshipsIndexes();
        }
    }
}