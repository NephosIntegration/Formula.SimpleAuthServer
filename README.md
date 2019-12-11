# Formula.SimpleAuthServer
A simple OAuth2 / OpenID Connect Authorization Server wrapper for Identity Server

# Adding Authentication Server
To in enable the auth server uncomment two sections in the Startup.cs
- **ConfigureServices** - services.AddSimpleAuthServer(this.Configuration);
- **Configure** - app.UseSimpleAuthServer();

## Defining Resources and Clients
You need to define the resources to protect (named resources like "My API"), clients that can access these protected resources (client id / secret), and identity resources to use (OpenID, or Profile based etc..).

This can be done by creating your own class that implements the ISimpleAuthServerConfig contract.

This can be passed as a parameter to AddSimpleAuthServer, within ConfigureServices, and UseSimpleAuthServer within Configure.

### Demo configuration
*see SimpleAuthServerConfigDemo.Get() for details*

For demo / testing purposes, if you do not define your own resource and client list, there are some "defaults" enabled out of the box.

* 1 resource named "api".
* 3 Clients
    * Non-interactive user (used for authorization only)
        * Client ID / Secret = cliet / secret
        * Grant TYpe = Client Credentials
        * Allowed Scopes = "api"
    * Interactive user (used for authentication)
        * Client ID / Secret = OpenIDConnectDemo / secret
        * Grant Type = Code
        * Allowed Scopes = [openid, profile, api]
        * When SimpleServerUI is in use, you can log in with this user from the views provided
    * Browser Based Client
        * Client ID = js
        * Grant Type = Implicit
        * Allowed Scopes = [openid, profile, api]
        * (expects the browser client to be running on localhost:8080)

### Configuring Stores and Migrations
To run without a persistent data store, you may run all operations in memory by setting 
InMemoryAuthProvider to true in the appSettings.

To work with persistent storage this must be set to false, and a connectionName (as also specified in the appSettings), might optionally be passed as a parameter to AddSimpleAuthServer (default connection string is DefaultConnection).

#### Running Migrations
To build and run migrations you must install the EF Core CLI tool on your machine, and add the Microsoft.EntityFrameworkCore.Design package to your project.

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
```

To generate the migrations in your project run;

```bash
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration --context IdentityServer4.EntityFramework.DbContexts.PersistedGrantDbContext --output-dir Data/Migrations/IdentityServer/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration --context IdentityServer4.EntityFramework.DbContexts.ConfigurationDbContext --output-dir Data/Migrations/IdentityServer/ConfigurationDb
dotnet ef database update --context IdentityServer4.EntityFramework.DbContexts.PersistedGrantDbContext
dotnet ef database update --context IdentityServer4.EntityFramework.DbContexts.ConfigurationDbContext
```

If you installed your migrations a different project, you may specify the "migrationsAssembly", as a parameter to AddSimpleAuthServer.

# Internal Routes 
* Discovery Document - [http://localhost:5000/.well-known/openid-configuration](http://localhost:5000/.well-known/openid-configuration)
* Identity Details - [http://localhost:5000/identity](http://localhost:5000/identity)

# Relavant Links
- [IdentityServer4 Docs](https://identityserver4.readthedocs.io)

# Packages / Projects Used
- [IdentityServer4](https://www.nuget.org/packages/IdentityServer4/)
- [IdentityServer4.EntityFramework](https://www.nuget.org/packages/IdentityServer4.EntityFramework)
- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/)
