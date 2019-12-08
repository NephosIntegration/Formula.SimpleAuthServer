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

            useInMemoryAuthProvider = true;

            if (authConfig == null) authConfig = SimpleAuthServerConfigDemo.Get();

            var builder = services.AddIdentityServer();

            if (useInMemoryAuthProvider) 
            {
                var identityResources = authConfig.GetIdentityResources();
                if (identityResources != null) builder.AddInMemoryIdentityResources(identityResources);

                var apiResources = authConfig.GetApiResources();
                if (apiResources != null) builder.AddInMemoryApiResources(apiResources);

                var clients = authConfig.GetClients();
                if (clients != null) builder.AddInMemoryClients(clients);

                var testUsers = authConfig.GetTestUsers();
                if (testUsers != null) builder.AddTestUsers(testUsers);
            }

            builder.AddDeveloperSigningCredential();

            return services;
        }

        public static IApplicationBuilder UseSimpleAuthServer(this IApplicationBuilder app) {
            return app.UseIdentityServer();
        }
    }
}
