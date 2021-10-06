using System.Collections.Generic;

namespace Dapper.Alpha.Extensions
{
    public static class IEnumerableExtensions
    {
        public static DynamicParameters ToDynamicParameters(this IEnumerable<KeyValuePair<string, object>> self)
        {
            var dymParams = new DynamicParameters();
            foreach (var item in self)
            {
                dymParams.Add(item.Key, item.Value);
            }
            return dymParams;
        }
    }
}
