using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Linq;
using Formula.SimpleMembership;

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

        public static void InitializeDatabase(IApplicationBuilder app, IConfiguration configuration, ISimpleAuthServerConfig authConfig  = null)
        {
            if (authConfig == null) authConfig = SimpleAuthServerConfigDemo.Get();

            bool useInMemoryAuthProvider = bool.Parse(configuration.GetValue<String>("InMemoryAuthProvider"));

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                if (serviceScope.ServiceProvider.GetServices<SimpleMembershipDbContext>().Count() > 0)
                {
                    var membershiDbpContext = serviceScope.ServiceProvider.GetRequiredService<SimpleMembershipDbContext>();

                    if (!useInMemoryAuthProvider) membershiDbpContext.Database.Migrate();

                    if (!membershiDbpContext.Users.Any())
                    {
                        var userMgr = serviceScope.ServiceProvider.GetRequiredService<AppUserManager>();
                        
                        foreach(var testUser in authConfig.GetTestUsers())
                        {
                            var user = new ApplicationUser();
                            var email = testUser.Claims.Where(t => t.Type == IdentityModel.JwtClaimTypes.Email).FirstOrDefault().Value;
                            user.ConcurrencyStamp = DateTime.Now.Ticks.ToString();
                            user.Email = email;
                            user.EmailConfirmed = true;
                            //user.Id = UserSettings.UserId;
                            user.NormalizedEmail = email;
                            user.NormalizedUserName = testUser.Username;
                            user.UserName = testUser.Username;

                            var result = userMgr.CreateAsync(user, testUser.Password).Result;
                            if (!result.Succeeded)
                            {
                                throw new Exception(result.Errors.First().Description);
                            }

                            var createdUser = userMgr.FindByNameAsync("alice").Result;

                            result = userMgr.AddClaimsAsync(createdUser, testUser.Claims).Result;
                            if (!result.Succeeded)
                            {
                                throw new Exception(result.Errors.First().Description);
                            }
                        }
                    }
                }

                if (serviceScope.ServiceProvider.GetServices<PersistedGrantDbContext>().Count() > 0)
                {
                    if (!useInMemoryAuthProvider) serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                }

                if (serviceScope.ServiceProvider.GetServices<ConfigurationDbContext>().Count() > 0)
                {
                    var configDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                    if (!useInMemoryAuthProvider) configDbContext.Database.Migrate();
                    if (!configDbContext.Clients.Any())
                    {
                        foreach (var client in authConfig.GetClients())
                        {
                            configDbContext.Clients.Add(client.ToEntity());
                        }
                        configDbContext.SaveChanges();
                    }

                    if (!configDbContext.IdentityResources.Any())
                    {
                        foreach (var resource in authConfig.GetIdentityResources())
                        {
                            configDbContext.IdentityResources.Add(resource.ToEntity());
                        }
                        configDbContext.SaveChanges();
                    }

                    if (!configDbContext.ApiResources.Any())
                    {
                        foreach (var resource in authConfig.GetApiResources())
                        {
                            configDbContext.ApiResources.Add(resource.ToEntity());
                        }
                        configDbContext.SaveChanges();
                    }
                }
            }
        }

        public static IApplicationBuilder UseSimpleAuthServer(this IApplicationBuilder app, IConfiguration configuration, ISimpleAuthServerConfig authConfig  = null) {
            bool useInMemoryAuthProvider = bool.Parse(configuration.GetValue<String>("InMemoryAuthProvider"));

            if (authConfig == null) authConfig = SimpleAuthServerConfigDemo.Get();

            SimpleAuthServerConfiguration.InitializeDatabase(app, configuration, authConfig);

            return app.UseIdentityServer();
        }
    }
}
