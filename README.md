# Formula.SimpleAuthServer
A simple OAuth2 / OpenID Connect Authorization Server wrapper for Identity Server

# Adding Authentication Server
To in enable the auth server uncomment two sections in the Startup.cs
- **ConfigureServices** - services.AddSimpleAuthServer(this.Configuration);
- **Configure** - app.UseSimpleAuthServer();

## Defining Resources and Clients
You need to define the resources to protect (named resources like "My API"), clients that can access these protected resources (client id / secret), and identity resources to use (OpenID, or Profile based etc..).

This can be done by creating your own class that implements the ISimpleAuthServerConfig contract.

This can be passed as the second parameter to AddSimpleAuthServer.

### Demo configuration
*see SimpleAuthServerConfigDemo.Get() for details*

For demo / testing purposes, if you do not define your own resource and client list, there are some "defaults" enabled out of the box.

* 1 resource named "api".
* 2 Clients
    * Non-interactive user (used for authorization only)
        * Client ID / Secret = cliet / secret
        * Grant TYpe = Client Credentials
        * Allowed Scopes = "api"
    * Interactive user (used for authentication)
        * Client ID / Secret = OpenIDConnectDemo / secret
        * Grant Type = Code
        * Allowed Scopes = [openid, profile, api]
        * When SimpleServerUI is in use, you can log in with this user from the views provided


# Internal Routes 
* Discovery Document - [http://localhost:5000/.well-known/openid-configuration](http://localhost:5000/.well-known/openid-configuration)
* Identity Details - [http://localhost:5000/identity](http://localhost:5000/identity)

# Relavant Links
- [IdentityServer4 Docs](https://identityserver4.readthedocs.io)

# Packages / Projects Used
- [IdentityServer4](https://www.nuget.org/packages/IdentityServer4/)
