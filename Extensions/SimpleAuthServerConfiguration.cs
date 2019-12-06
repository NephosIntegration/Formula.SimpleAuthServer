using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace Formula.SimpleAuthServer
{
    public static class SimpleAuthServerConfiguration
    {
        public static IServiceCollection AddSimpleAuthServer(this IServiceCollection services, IConfiguration configuration, ISimpleAuthServerConfig authConfig  = null)
        {
            bool useInMemoryAuthProvider = bool.Parse(configuration.GetValue<String>("InMemoryAuthProvider"));

            // uncomment, if you want to add an MVC-based UI
            //services.AddControllersWithViews();

            if (authConfig == null) authConfig = SimpleAuthServerConfigDemo.Get();

            var builder = services.AddIdentityServer()
                .AddInMemoryApiResources(authConfig.GetApiResources())
                .AddInMemoryClients(authConfig.GetClients());

            builder.AddDeveloperSigningCredential();

            return services;
        }

        public static IApplicationBuilder UseSimpleAuthServer(this IApplicationBuilder app) {
            return app.UseIdentityServer();
        }
    }
}
