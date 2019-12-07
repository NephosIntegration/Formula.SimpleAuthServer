# Formula.SimpleAuthServer
A simple OAuth2 / OpenID Connect Authorization Server wrapper for Identity Server

# Adding Authentication Server
To in enable the auth server uncomment two sections in the Startup.cs
- **ConfigureServices** - services.AddSimpleAuthServer(this.Configuration);
- **Configure** - app.UseSimpleAuthServer();

## Defining Resources and Clients
You need to define the resources to protect, and clients that can access these protected resources 

This can be done by creating your own class derived from SimpleAuthServerConfigBase.

This can be passed as the second parameter to AddSimpleAuthServer.

For demo / testing purposes, an example that sets up a resource name "api", and defines one client (client id = "client", secret = "secret"), with one allowed scope of "api".  (see SimpleAuthServerConfigDemo.Get() for details).

## Viewing the discovery document
[http://localhost:5000/.well-known/openid-configuration](http://localhost:5000/.well-known/openid-configuration)

# Relavant Links
- [IdentityServer4 Docs](https://identityserver4.readthedocs.io)

# Packages / Projects Used
- [IdentityServer4](https://www.nuget.org/packages/IdentityServer4/)
