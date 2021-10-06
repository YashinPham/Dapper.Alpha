namespace Dapper.Alpha.Metadata
{
    public class QueryParameter
    {
        public QueryParameter(string propertyName, object propertyValue, string queryOperator)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            QueryOperator = queryOperator;
        }

        public string PropertyName { get; set; }

        public object PropertyValue { get; set; }

        public string QueryOperator { get; set; }
    }
}
