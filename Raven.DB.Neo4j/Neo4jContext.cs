using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.Neo4j
{
    public class Neo4jContext
    {
        public IDriver driver;
        public static Dictionary<string, string> CypherQuerries { get; private set; }

        public Neo4jContext()
        {
            driver = GraphDatabase.Driver("bolt://neo4j:7687", AuthTokens.Basic("neo4j", "ravenpas"));
        }

        public static void InitRequests()
        {
            CypherQuerries = new Dictionary<string, string>();
            var querriesFiles = Directory.GetFiles("CypherRequests");
            foreach (var file in querriesFiles)
            {
                CypherQuerries.Add
                    (
                    file.Substring(file.LastIndexOf('/') + 1, file.LastIndexOf('.') - file.LastIndexOf('/') - 1),
                    File.ReadAllText(file)
                    );
            }
        }
    }
}
