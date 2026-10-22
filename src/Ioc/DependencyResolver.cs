using CrossCutting.Concerns;
using Data.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace Ioc
{
    public static class DependencyResolver
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

            return services;
        }
    }
}
