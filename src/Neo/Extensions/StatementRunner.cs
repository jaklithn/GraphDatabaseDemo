using System.Linq;
using Neo4j.Driver.V1;


namespace Neo.Extensions
{
    public static class StatementRunner
    {
        public static T GetObject<T>(this IStatementResult result, string key) where T : class
        {
            var node = (INode)result.SingleOrDefault().As<IRecord>()[key];
            return node?.AsObject<T>();
        }
    }
}