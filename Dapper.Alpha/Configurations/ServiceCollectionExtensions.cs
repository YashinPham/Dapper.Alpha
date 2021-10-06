using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dapper.Alpha.Configurations
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbSession(this IServiceCollection collection, Action<DbSessiontOptionsBuilder> optionsAction)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            optionsAction.Invoke(DbSessiontOptionsBuilder.GetInstance());
            return collection.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
