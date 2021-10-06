using Dapper.Alpha.Metadata;

namespace Dapper.Alpha.SqlBuilders.QueryExpressions
{
    public abstract class QueryExpression
    {
        /// <summary>
        /// Query Expression Node Type
        /// </summary>
        public QueryExpressionType NodeType { get; set; }

        /// <summary>
        /// Operator OR/AND
        /// </summary>
        public string LinkingOperator { get; set; }

        public override string ToString()
        {
            return $"[NodeType:{this.NodeType}, LinkingOperator:{LinkingOperator}]";
        }
    }
}
