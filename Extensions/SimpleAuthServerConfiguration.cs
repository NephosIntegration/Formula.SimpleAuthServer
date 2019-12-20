using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Linq;

namespace Formula.SimpleAuthServer
{
    public static class SimpleAuthServerConfiguration
    {
        public static IIdentityServerBuilder AddSimpleAuthServer(this IServiceCollection services, IConfiguration configuration, String migrationsAssembly, String connectionName = "DefaultConnection", ISimpleAuthServerConfig authConfig  = null)
        {
            bool useInMemoryAuthProvider = bool.Parse(configuration.GetValue<String>("InMemoryAuthProvider"));

            if (authConfig == null) authConfig = SimpleAuthServerConfigDemo.Get();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            });

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
            else
            {
                var connectionString = configuration.GetValue<String>($"ConnectionStrings:{connectionName}");
                builder.AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                });

            }

            services.AddLocalApiAuthentication();

            builder.AddDeveloperSigningCredential();

            return builder;
        }

        public static void InitializeDatabase(IApplicationBuilder app, ISimpleAuthServerConfig authConfig  = null)
        {
            if (authConfig == null) authConfig = SimpleAuthServerConfigDemo.Get();

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in authConfig.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in authConfig.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in authConfig.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

        public static IApplicationBuilder UseSimpleAuthServer(this IApplicationBuilder app, IConfiguration configuration, ISimpleAuthServerConfig authConfig  = null) {
            bool useInMemoryAuthProvider = bool.Parse(configuration.GetValue<String>("InMemoryAuthProvider"));

            if (authConfig == null) authConfig = SimpleAuthServerConfigDemo.Get();

            if (!useInMemoryAuthProvider) SimpleAuthServerConfiguration.InitializeDatabase(app, authConfig);

            return app.UseIdentityServer();
        }
    }
}
