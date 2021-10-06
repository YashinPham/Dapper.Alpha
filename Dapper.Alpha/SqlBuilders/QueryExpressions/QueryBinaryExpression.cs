using Dapper.Alpha.Metadata;
using System.Collections.Generic;

namespace Dapper.Alpha.SqlBuilders.QueryExpressions
{
    internal class QueryBinaryExpression : QueryExpression
    {
        public QueryBinaryExpression()
        {
            NodeType = QueryExpressionType.Binary;
        }

        public List<QueryExpression> Nodes { get; set; }

        public override string ToString()
        {
            return $"[{base.ToString()} ({string.Join(",", Nodes)})]";
        }
    }
}
